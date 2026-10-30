#!/usr/bin/env python3
import argparse, datetime as dt, decimal, json, os, re, sys, uuid
from pathlib import Path
import pymssql, pymysql

SRC=os.environ.get("VAMCITY_SOURCE_DB","vamcityc_vamcity")
DST=os.environ.get("VAMCITY_TARGET_DB","vamcitydigibilepay")
STATE=Path(os.environ.get("VAMCITY_MIGRATION_STATE","/home/digibail/.vamcity-migration/state.json"))
PLAN=Path(os.environ.get("VAMCITY_MIGRATION_PLAN","/home/digibail/.vamcity-migration/plan.json"))
BATCH=int(os.environ.get("VAMCITY_BATCH_SIZE","250"))
TICK=chr(96)

def qs(x): return "["+str(x).replace("]","]]")+"]"
def qm(x): return TICK+str(x).replace(TICK,TICK+TICK)+TICK

def write_json(path,obj):
    path.parent.mkdir(parents=True,exist_ok=True)
    tmp=path.with_suffix(path.suffix+".tmp")
    tmp.write_text(json.dumps(obj,ensure_ascii=False,indent=2,default=str),encoding="utf-8")
    os.chmod(tmp,0o600); tmp.replace(path); os.chmod(path,0o600)

def strip_parens(s):
    s=(s or "").strip()
    while s.startswith("(") and s.endswith(")"):
        depth=0; quote=False; valid=True
        for i,ch in enumerate(s):
            if ch=="'": quote=not quote
            if quote: continue
            if ch=="(": depth+=1
            elif ch==")":
                depth-=1
                if depth==0 and i<len(s)-1: valid=False; break
        if valid and depth==0: s=s[1:-1].strip()
        else: break
    return s

def map_default(raw):
    if not raw: return None,None
    s=strip_parens(raw); u=s.upper()
    known={"GETDATE()":"CURRENT_TIMESTAMP","CURRENT_TIMESTAMP":"CURRENT_TIMESTAMP",
           "SYSDATETIME()":"CURRENT_TIMESTAMP(6)","GETUTCDATE()":"UTC_TIMESTAMP()",
           "SYSUTCDATETIME()":"UTC_TIMESTAMP(6)","NEWID()":"UUID()",
           "NEWSEQUENTIALID()":"UUID()","NULL":"NULL"}
    if u in known:
        return known[u],("NEWSEQUENTIALID approximated by UUID()" if u=="NEWSEQUENTIALID()" else None)
    if re.fullmatch(r"[-+]?\d+(?:\.\d+)?(?:e[-+]?\d+)?",s,re.I): return s,None
    cv=re.fullmatch(r"CONVERT\(\[?(?:bit|tinyint|bigint|int|decimal|numeric|float|real)\]?,\s*\(?\s*([-+]?\d+(?:\.\d+)?(?:e[-+]?\d+)?)\s*\)?\)",s,re.I)
    if cv: return cv.group(1),None
    m=re.fullmatch(r"N?'((?:''|[^'])*)'",s,re.S)
    if m: return "'"+m.group(1).replace("''","'").replace("\\","\\\\").replace("'","''")+"'",None
    return None,"unmapped default: "+raw[:160]

def mt(c):
    t=c["type_name"].lower(); n=int(c["max_length"]); p=int(c["precision"]); s=int(c["scale"])
    if t in ("nvarchar","nchar"):
        if n==-1:return "LONGTEXT"
        return ("CHAR" if t=="nchar" else "VARCHAR")+f"({max(1,n//2)})"
    if t in ("varchar","char"):
        if n==-1:return "LONGTEXT"
        return ("CHAR" if t=="char" else "VARCHAR")+f"({max(1,n)})"
    if t=="uniqueidentifier":return "CHAR(36) CHARACTER SET ascii COLLATE ascii_bin"
    if t=="datetimeoffset":return "VARCHAR(40) CHARACTER SET ascii COLLATE ascii_bin"
    if t=="datetime2":return f"DATETIME({s})" if s<=6 else "VARCHAR(27)"
    if t=="datetime":return "DATETIME(6)"
    if t=="smalldatetime":return "DATETIME"
    if t=="time":return f"TIME({s})" if s<=6 else "VARCHAR(16)"
    if t in ("decimal","numeric"):return f"DECIMAL({p},{s})"
    if t=="float":return "DOUBLE" if p>24 else "FLOAT"
    if t=="real":return "FLOAT"
    if t=="bigint":return "BIGINT"
    if t=="int":return "INT"
    if t=="smallint":return "SMALLINT"
    if t=="tinyint":return "TINYINT UNSIGNED"
    if t=="bit":return "TINYINT(1)"
    if t=="money":return "DECIMAL(19,4)"
    if t=="smallmoney":return "DECIMAL(10,4)"
    if t in ("binary","varbinary"):
        if n==-1:return "LONGBLOB"
        return ("BINARY" if t=="binary" else "VARBINARY")+f"({max(1,n)})"
    if t in ("timestamp","rowversion"):return "BINARY(8)"
    if t in ("text","ntext","xml"):return "LONGTEXT"
    if t=="image":return "LONGBLOB"
    raise ValueError("unsupported SQL Server type: "+t)

def rows(conn,sql):
    c=conn.cursor(as_dict=True); c.execute(sql); out=c.fetchall(); c.close(); return out

def connect_src():
    return pymssql.connect(server=os.environ["VAMCITY_SQLSERVER_IP"],port="1433",user="sa",
      password=os.environ["VAMCITY_SQLSERVER_PASSWORD"],database=SRC,login_timeout=15,timeout=120,charset="UTF-8")
def connect_dst():
    return pymysql.connect(unix_socket="/var/lib/mysql/mysql.sock",user=os.environ["VAMCITY_MARIADB_USER"],
      password=os.environ["VAMCITY_MARIADB_PASSWORD"],database=DST,charset="utf8mb4",
      autocommit=False,connect_timeout=15,read_timeout=600,write_timeout=600)

def metadata(src):
    ts=rows(src,"""SELECT s.name schema_name,t.name table_name FROM sys.tables t
      JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE t.is_ms_shipped=0 ORDER BY s.name,t.name""")
    cs=rows(src,"""SELECT s.name schema_name,t.name table_name,c.column_id,c.name column_name,
      ty.name type_name,c.max_length,c.precision,c.scale,c.is_nullable,c.is_identity,c.is_computed,
      dc.definition default_definition
      FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
      JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
      LEFT JOIN sys.default_constraints dc ON dc.object_id=c.default_object_id
      WHERE t.is_ms_shipped=0 ORDER BY s.name,t.name,c.column_id""")
    ix=rows(src,"""SELECT s.name schema_name,t.name table_name,i.name index_name,i.is_primary_key,
      i.is_unique,i.type_desc,i.is_disabled,i.filter_definition,ic.key_ordinal,ic.is_included_column,
      ic.is_descending_key,c.name column_name
      FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id
      JOIN sys.schemas s ON s.schema_id=t.schema_id
      JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
      JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
      WHERE t.is_ms_shipped=0 AND i.index_id>0 AND i.is_hypothetical=0
      ORDER BY s.name,t.name,i.name,ic.key_ordinal,ic.index_column_id""")
    fk=rows(src,"""SELECT fk.name fk_name,ps.name parent_schema,pt.name parent_table,pc.name parent_column,
      rs.name ref_schema,rt.name ref_table,rc.name ref_column,fkc.constraint_column_id,
      fk.delete_referential_action_desc,fk.update_referential_action_desc,fk.is_disabled
      FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
      JOIN sys.tables pt ON pt.object_id=fkc.parent_object_id JOIN sys.schemas ps ON ps.schema_id=pt.schema_id
      JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
      JOIN sys.tables rt ON rt.object_id=fkc.referenced_object_id JOIN sys.schemas rs ON rs.schema_id=rt.schema_id
      JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
      ORDER BY ps.name,pt.name,fk.name,fkc.constraint_column_id""")
    for t in ts:
        c=src.cursor(); c.execute(f"SELECT COUNT_BIG(*) FROM {qs(t['schema_name'])}.{qs(t['table_name'])}")
        t["source_rows"]=int(c.fetchone()[0]); c.close()
    return ts,cs,ix,fk

def group_indexes(ix):
    out={}
    for r in ix:
        k=(r["schema_name"],r["table_name"],r["index_name"])
        g=out.setdefault(k,{"schema":r["schema_name"],"table":r["table_name"],"name":r["index_name"],
          "primary":bool(r["is_primary_key"]),"unique":bool(r["is_unique"]),"type":r["type_desc"],
          "disabled":bool(r["is_disabled"]),"filter":r["filter_definition"],"columns":[]})
        if not r["is_included_column"] and int(r["key_ordinal"])>0:
            g["columns"].append({"name":r["column_name"],"ordinal":int(r["key_ordinal"]),"desc":bool(r["is_descending_key"])})
    for g in out.values():g["columns"].sort(key=lambda x:x["ordinal"])
    return list(out.values())

def group_fks(rs):
    out={}
    for r in rs:
        k=(r["parent_schema"],r["parent_table"],r["fk_name"])
        g=out.setdefault(k,{"name":r["fk_name"],"parent_schema":r["parent_schema"],"parent_table":r["parent_table"],
          "ref_schema":r["ref_schema"],"ref_table":r["ref_table"],"delete":r["delete_referential_action_desc"],
          "update":r["update_referential_action_desc"],"disabled":bool(r["is_disabled"]),"cols":[],"refs":[]})
        g["cols"].append((int(r["constraint_column_id"]),r["parent_column"]))
        g["refs"].append((int(r["constraint_column_id"]),r["ref_column"]))
    for g in out.values():
        g["cols"]=[x[1] for x in sorted(g["cols"])];g["refs"]=[x[1] for x in sorted(g["refs"])]
    return list(out.values())

def build_plan(ts,cs,ix,fk):
    type_counts={}; unsupported=[]; defaults=[]; computed=[]
    for c in cs:
        try:c["mysql_type"]=mt(c)
        except Exception as e:unsupported.append({"table":c["schema_name"]+"."+c["table_name"],"column":c["column_name"],"error":str(e)})
        typ=c["type_name"].lower();type_counts[typ]=type_counts.get(typ,0)+1
        c["mysql_default"],warn=map_default(c.get("default_definition"))
        if warn:defaults.append({"table":c["schema_name"]+"."+c["table_name"],"column":c["column_name"],"warning":warn})
        if c["is_computed"]:computed.append(c["schema_name"]+"."+c["table_name"]+"."+c["column_name"])
    names=[t["table_name"].casefold() for t in ts]
    dup=sorted({n for n in names if names.count(n)>1})
    ig=group_indexes(ix); fg=group_fks(fk)
    return {"source_database":SRC,"target_database":DST,"tables":ts,"columns":cs,"indexes":ig,"foreign_keys":fg,
      "summary":{"table_count":len(ts),"source_rows":sum(t["source_rows"] for t in ts),"column_count":len(cs),
      "type_counts":type_counts,"index_count":len(ig),"primary_keys":sum(i["primary"] for i in ig),
      "unique_indexes":sum(i["unique"] and not i["primary"] for i in ig),
      "secondary_indexes":sum(not i["unique"] and not i["primary"] and not i["disabled"] for i in ig),
      "foreign_keys":len(fg),"computed_columns":computed,"unsupported_types":unsupported,
      "unmapped_defaults":defaults,"duplicate_table_names_case_insensitive":dup}}

def safe_name(s):
    if len(s)<=64:return s
    suffix="_"+uuid.uuid5(uuid.NAMESPACE_DNS,s).hex[:8]
    return s[:64-len(suffix)]+suffix

def ddl_table(t,cs,ix):
    schema,name=t["schema_name"],t["table_name"]
    cols=[c for c in cs if c["schema_name"]==schema and c["table_name"]==name]
    pk=next((i for i in ix if i["schema"]==schema and i["table"]==name and i["primary"] and not i["disabled"]),None)
    parts=[]
    for c in cols:
        line=qm(c["column_name"])+" "+c["mysql_type"]
        if c["is_identity"]:line+=" AUTO_INCREMENT"
        line+=" NULL" if c["is_nullable"] else " NOT NULL"
        if c.get("mysql_default") is not None and not c["is_identity"]:line+=" DEFAULT "+c["mysql_default"]
        parts.append(line)
    if pk:parts.append("PRIMARY KEY ("+",".join(qm(x["name"]) for x in pk["columns"])+")")
    return "CREATE TABLE "+qm(name)+" (\\n  "+",\\n  ".join(parts)+"\\n) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC"

def select_expr(c):
    n=qs(c["column_name"]);t=c["type_name"].lower()
    if t=="datetimeoffset":return f"CONVERT(varchar(40),{n},127) AS {n}"
    if t=="datetime2" and int(c["scale"])>6:return f"CONVERT(varchar(27),{n},126) AS {n}"
    if t=="time" and int(c["scale"])>6:return f"CONVERT(varchar(16),{n},126) AS {n}"
    return n

def norm(v,c):
    if v is None:return None
    if c["type_name"].lower()=="uniqueidentifier" or isinstance(v,uuid.UUID):return str(v)
    if isinstance(v,memoryview):return v.tobytes()
    if isinstance(v,bytearray):return bytes(v)
    return v

def add_indexes(dst,ix):
    made=0
    for i in ix:
        if i["disabled"] or i["primary"]:continue
        if i["filter"]:raise RuntimeError("Filtered index needs manual mapping: "+i["name"])
        if i["type"] not in ("CLUSTERED","NONCLUSTERED"):raise RuntimeError("Unsupported index type: "+i["name"])
        if not i["columns"]:continue
        name=safe_name(i["name"])
        with dst.cursor() as c:
            c.execute("SELECT COUNT(*) FROM information_schema.STATISTICS WHERE TABLE_SCHEMA=%s AND TABLE_NAME=%s AND INDEX_NAME=%s",(DST,i["table"],name))
            if c.fetchone()[0]:continue
            cols=",".join(qm(x["name"])+(" DESC" if x["desc"] else " ASC") for x in i["columns"])
            c.execute("CREATE "+("UNIQUE " if i["unique"] else "")+"INDEX "+qm(name)+" ON "+qm(i["table"])+" ("+cols+")")
        dst.commit();made+=1
    return made

def add_fks(dst,fks):
    made=0
    actions={"NO_ACTION":"RESTRICT","CASCADE":"CASCADE","SET_NULL":"SET NULL"}
    for f in fks:
        if f["disabled"]:raise RuntimeError("Disabled source FK needs review: "+f["name"])
        name=safe_name(f["name"])
        with dst.cursor() as c:
            c.execute("SELECT COUNT(*) FROM information_schema.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=%s AND TABLE_NAME=%s AND CONSTRAINT_NAME=%s",(DST,f["parent_table"],name))
            if c.fetchone()[0]:continue
        if f["delete"] not in actions or f["update"] not in actions:raise RuntimeError("Unsupported FK action: "+f["name"])
        ddl="ALTER TABLE "+qm(f["parent_table"])+" ADD CONSTRAINT "+qm(name)+" FOREIGN KEY ("+",".join(qm(x) for x in f["cols"])+") REFERENCES "+qm(f["ref_table"])+" ("+",".join(qm(x) for x in f["refs"])+") ON DELETE "+actions[f["delete"]]+" ON UPDATE "+actions[f["update"]]
        with dst.cursor() as c:c.execute(ddl)
        dst.commit();made+=1
    return made

def target_names(dst):
    with dst.cursor() as c:
        c.execute("SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA=%s AND TABLE_TYPE='BASE TABLE'",(DST,))
        return {x[0] for x in c.fetchall()}

def apply(src,dst,p):
    summary=p["summary"]
    if summary["unsupported_types"] or summary["unmapped_defaults"] or summary["computed_columns"] or summary["duplicate_table_names_case_insensitive"]:
        raise RuntimeError("Plan has unresolved type/default/computed/table-name mappings; refusing apply.")
    if any(i["filter"] for i in p["indexes"]):raise RuntimeError("Filtered indexes require explicit mapping.")
    if any(i["disabled"] for i in p["foreign_keys"]):raise RuntimeError("Disabled foreign keys require explicit review.")
    names=target_names(dst);expected={t["table_name"] for t in p["tables"]}
    if STATE.exists():
        st=json.loads(STATE.read_text(encoding="utf-8"))
        if st.get("source_database")!=SRC or st.get("target_database")!=DST or not names.issubset(expected):
            raise RuntimeError("Private migration state/target mismatch; refusing.")
    elif names:raise RuntimeError("Target is not empty and no private migration state exists.")
    else:
        st={"source_database":SRC,"target_database":DST,"created_tables":[],"completed_tables":{}}
        write_json(STATE,st)
    for t in p["tables"]:
        if t["table_name"] not in names:
            with dst.cursor() as c:c.execute(ddl_table(t,p["columns"],p["indexes"]))
            dst.commit();names.add(t["table_name"]);st["created_tables"].append(t["table_name"]);write_json(STATE,st)
    for t in p["tables"]:
        schema,name=t["schema_name"],t["table_name"]
        cols=[c for c in p["columns"] if c["schema_name"]==schema and c["table_name"]==name]
        with dst.cursor() as c:c.execute("SELECT COUNT(*) FROM "+qm(name));existing=int(c.fetchone()[0])
        expected_rows=int(t["source_rows"])
        if existing==expected_rows:
            st["completed_tables"][name]=existing;write_json(STATE,st)
            print(f"TABLE_MATCHED={name}:{existing}",flush=True);continue
        if existing:
            with dst.cursor() as c:c.execute("TRUNCATE TABLE "+qm(name))
            dst.commit()
        expr=",".join(select_expr(c) for c in cols)
        query="SELECT "+expr+" FROM "+qs(schema)+"."+qs(name)
        ins="INSERT INTO "+qm(name)+" ("+",".join(qm(c["column_name"]) for c in cols)+") VALUES ("+",".join(["%s"]*len(cols))+")"
        cur=src.cursor();cur.execute(query);copied=0
        while True:
            batch=cur.fetchmany(BATCH)
            if not batch:break
            vals=[tuple(norm(v,c) for v,c in zip(row,cols)) for row in batch]
            with dst.cursor() as c:c.executemany(ins,vals)
            dst.commit();copied+=len(vals)
            if copied%(BATCH*20)==0:print(f"PROGRESS={name}:{copied}/{expected_rows}",flush=True)
        cur.close()
        with dst.cursor() as c:c.execute("SELECT COUNT(*) FROM "+qm(name));actual=int(c.fetchone()[0])
        if copied!=expected_rows or actual!=expected_rows:raise RuntimeError(f"Row mismatch {schema}.{name}: source={expected_rows},copied={copied},target={actual}")
        st["completed_tables"][name]=actual;write_json(STATE,st)
        print(f"TABLE_IMPORTED={schema}.{name}:{actual}",flush=True)
    indexes=add_indexes(dst,p["indexes"])
    fks=add_fks(dst,p["foreign_keys"])
    result=[];total=0
    for t in p["tables"]:
        with dst.cursor() as c:c.execute("SELECT COUNT(*) FROM "+qm(t["table_name"]));n=int(c.fetchone()[0])
        result.append({"schema":t["schema_name"],"table":t["table_name"],"source_rows":t["source_rows"],"target_rows":n,"match":n==int(t["source_rows"])})
        total+=n
    if not all(x["match"] for x in result):raise RuntimeError("Final table row-count reconciliation failed.")
    report={"source_database":SRC,"target_database":DST,"tables":result,"total_source_rows":sum(x["source_rows"] for x in result),"total_target_rows":total,"secondary_indexes_created":indexes,"foreign_keys_created":fks,"completed_at":dt.datetime.now(dt.timezone.utc).isoformat()}
    write_json(PLAN.with_name("result.json"),report)
    print("MIGRATION_DATA_RECONCILIATION=SUCCESS",flush=True)
    print("TOTAL_SOURCE_ROWS="+str(report["total_source_rows"]),flush=True)
    print("TOTAL_TARGET_ROWS="+str(report["total_target_rows"]),flush=True)

def main():
    ap=argparse.ArgumentParser();ap.add_argument("--mode",choices=("dry-run","apply"),default="dry-run");args=ap.parse_args()
    src=connect_src();dst=connect_dst()
    try:
        with dst.cursor() as c:
            c.execute("SELECT VERSION(),DATABASE()");print("TARGET="+repr(c.fetchone()),flush=True)
            c.execute("SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA=%s AND TABLE_TYPE='BASE TABLE'",(DST,));target_count=int(c.fetchone()[0])
        ts,cs,ix,fk=metadata(src);p=build_plan(ts,cs,ix,fk);p["summary"]["target_table_count_before"]=target_count
        write_json(PLAN,p)
        print("MIGRATION_PLAN="+str(PLAN),flush=True)
        print("SUMMARY="+json.dumps(p["summary"],ensure_ascii=False,default=str),flush=True)
        if args.mode=="dry-run":print("DRY_RUN_COMPLETE=1",flush=True)
        else:apply(src,dst,p)
    finally:src.close();dst.close()

if __name__=="__main__":
    try:main()
    except Exception as e:
        print("MIGRATION_ERROR="+str(e),file=sys.stderr,flush=True);raise
