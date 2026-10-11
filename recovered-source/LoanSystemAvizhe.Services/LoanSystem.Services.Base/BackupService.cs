using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class BackupService : BaseService, IBackupService
{
	private readonly IAppContext _appContext;

	private readonly IHostingEnvironment _hostingEnvironment;

	private readonly IConfiguration _configuration;

	public BackupService(AppDbContext context, IAppContext appContext, IHostingEnvironment hostingEnvironment, IConfiguration configuration, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
		_appContext = appContext;
		_hostingEnvironment = hostingEnvironment;
		_configuration = configuration;
	}

	public Task ImportStarter()
	{
		return Table<Loan>().ToListAsync();
	}

	public Task<string> Create()
	{
		return Task.Run(async () => await CreateBySqlServerBackupQuery());
	}

	public Task<List<dynamic>> GetBackupList()
	{
		List<object> list = new List<object>();
		string path = Path.Combine(_hostingEnvironment.WebRootPath, GetBaseFolder());
		if (!Directory.Exists(path))
		{
			return Task.FromResult(list);
		}
		foreach (FileInfo item in from x in new DirectoryInfo(path).GetFiles()
			orderby x.LastWriteTime descending
			select x)
		{
			list.Add(new
			{
				FileName = item.Name,
				CreateDate = item.CreationTime.ToPersianDateTime(),
				FileSize = GetFormatedSize(item.Length)
			});
		}
		return Task.FromResult(list);
	}

	public byte[] Download(string fileName)
	{
		string path = Path.Combine(_hostingEnvironment.WebRootPath, GetBaseFolder(), fileName);
		if (!File.Exists(path))
		{
			throw new CustomException("فایل یافت نشد.");
		}
		return File.ReadAllBytes(path);
	}

	public async Task Restore(RestoreBackupModel model)
	{
		await RestoreByJsonFile(model.File);
	}

	private async Task RestoreByJsonFile(IFormFile file)
	{
		if (file == null)
		{
			return;
		}
		(string, string, string) paths = await PrepareFileExtract(file);
		string value = await CreateDeleteTablesQueries();
		SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection"));
		using (SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString))
		{
			connection.Open();
			SqlTransaction sqlTransaction = connection.BeginTransaction();
			try
			{
				ExecuteSqlQuery(new StringBuilder(value), connection, sqlTransaction);
				List<string> existingMigrationHistory = GetExistingMigrationHistory(connection, sqlTransaction);
				string lastMigartionInBackup = GetLastMigartionInBackup(paths.Item1);
				RollBackMigrationIfRequired(existingMigrationHistory, lastMigartionInBackup, connection, sqlTransaction);
				Dictionary<string, List<SqlTableVColumnInfoModel>> tableColumnsInfo = GetTableColumnsInfo(connection, sqlTransaction);
				Dictionary<string, List<string>> tableFkConstrains = GetTableFkConstrains(connection, sqlTransaction);
				using (JsonTextReader jsonTextReader = new JsonTextReader(new StreamReader(paths.Item1, detectEncodingFromByteOrderMarks: true)))
				{
					while (jsonTextReader.Read())
					{
						if (jsonTextReader.Value == null || !(jsonTextReader.Value.ToString() == "tableName"))
						{
							continue;
						}
						jsonTextReader.Read();
						string text = jsonTextReader.Value.ToString();
						if (text.Contains("Migrations") || text.Contains("HangFire") || !tableColumnsInfo.ContainsKey(text))
						{
							continue;
						}
						jsonTextReader.Read();
						jsonTextReader.Read();
						JArray jArray = (JArray)JToken.ReadFrom(jsonTextReader);
						if (!jArray.Any())
						{
							continue;
						}
						StringBuilder stringBuilder = new StringBuilder();
						if (tableFkConstrains.ContainsKey(text))
						{
							foreach (string item in tableFkConstrains[text])
							{
								stringBuilder.Append("ALTER TABLE " + text + " NOCHECK CONSTRAINT " + item + ";");
							}
						}
						stringBuilder.Append("IF (OBJECTPROPERTY(OBJECT_ID('" + text + "'), 'TableHasIdentity') = 1)\r\n                                        BEGIN\r\n                                          SET IDENTITY_INSERT " + text + " ON\r\n                                        END;");
						ExecuteSqlQuery(stringBuilder, connection, sqlTransaction);
						for (int i = 0; i <= jArray.Count / 1000; i++)
						{
							StringBuilder stringBuilder2 = new StringBuilder("INSERT INTO " + text);
							stringBuilder2.Append(" (" + string.Join(",", tableColumnsInfo[text].Select((SqlTableVColumnInfoModel x) => "[" + x.Name + "]")) + ")");
							stringBuilder2.Append(" VALUES ");
							foreach (JToken item2 in jArray.Skip(i * 1000).Take(1000))
							{
								string text2 = "(";
								foreach (SqlTableVColumnInfoModel item3 in tableColumnsInfo[text])
								{
									string[] source = new string[4] { "decimal", "bigint", "float", "int" };
									JToken jToken = item2[item3.Name];
									if (jToken != null && !string.IsNullOrEmpty((string?)jToken))
									{
										if (item3.DataType == "bit")
										{
											text2 = text2 + ((bool.TryParse((string?)jToken, out var result) & result) ? "1" : "0") + ",";
										}
										else
										{
											text2 = ((!Enumerable.Contains(source, item3.DataType)) ? (text2 + $"N'{jToken}',") : (text2 + $"{jToken},"));
										}
									}
									else if (item3.DataType == "uniqueidentifier")
									{
										text2 = ((!item3.IsNullable) ? (text2 + "'00000000-0000-0000-0000-000000000000',") : (text2 + "NULL,"));
									}
									else
									{
										text2 = ((!Enumerable.Contains(source, item3.DataType) && !(item3.DataType == "bit")) ? (text2 + "'',") : (text2 + "0,"));
									}
								}
								text2 = text2.TrimEnd(',');
								text2 += "),";
								stringBuilder2.Append(text2);
							}
							stringBuilder2 = new StringBuilder(stringBuilder2.ToString().TrimEnd(','));
							ExecuteSqlQuery(stringBuilder2, connection, sqlTransaction);
						}
						if (tableFkConstrains.ContainsKey(text))
						{
							foreach (string item4 in tableFkConstrains[text])
							{
								stringBuilder.Append("ALTER TABLE " + text + " CHECK CONSTRAINT " + item4 + ";");
							}
						}
						stringBuilder.Append("IF (OBJECTPROPERTY(OBJECT_ID('" + text + "'), 'TableHasIdentity') = 1)\r\n                                        BEGIN\r\n                                            SET IDENTITY_INSERT " + text + " OFF\r\n                                        END;");
						ExecuteSqlQuery(stringBuilder, connection, sqlTransaction);
					}
				}
				sqlTransaction.Commit();
				if (_context.Database.GetPendingMigrations().Any())
				{
					await _context.Database.MigrateAsync();
				}
			}
			catch (Exception ex)
			{
				try
				{
					sqlTransaction.Rollback();
				}
				catch (Exception)
				{
				}
				throw new CustomException(ex.Message);
			}
		}
		if (File.Exists(paths.Item2))
		{
			File.Delete(paths.Item2);
		}
		if (Directory.Exists(paths.Item3))
		{
			Directory.Delete(paths.Item3, recursive: true);
		}
	}

	private void RollBackMigrationIfRequired(List<string> existingMigrations, string lastMigartionInBackup, SqlConnection connection, SqlTransaction transaction)
	{
		int num = existingMigrations.IndexOf(lastMigartionInBackup);
		if (num < 0 || num == existingMigrations.Count - 1)
		{
			return;
		}
		for (int num2 = existingMigrations.Count - 1; num2 > num; num2--)
		{
			string text = existingMigrations[num2].Substring(existingMigrations[num2].IndexOf('_') + 1);
			Type? type = Type.GetType("LoanSystem.Data.Migrations." + text + ", LoanSystemAvizhe.Data");
			if (type == null)
			{
				throw new CustomException("کلاس مرتبط با مایگریشن " + text + " یافت نشد.");
			}
			object obj = type.GetMethod("ScriptToPrevious")?.Invoke(null, null);
			if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
			{
				ExecuteSqlQuery(new StringBuilder(obj.ToString()), connection, transaction);
			}
		}
	}

	private string GetLastMigartionInBackup(string path)
	{
		string result = "";
		using (JsonTextReader jsonTextReader = new JsonTextReader(new StreamReader(path, detectEncodingFromByteOrderMarks: true)))
		{
			while (jsonTextReader.Read())
			{
				if (jsonTextReader.Value == null || !(jsonTextReader.Value.ToString() == "tableName"))
				{
					continue;
				}
				jsonTextReader.Read();
				if (jsonTextReader.Value.ToString().Contains("Migrations"))
				{
					jsonTextReader.Read();
					jsonTextReader.Read();
					JArray source = (JArray)JToken.ReadFrom(jsonTextReader);
					if (source.Any())
					{
						result = source.Last()["MigrationId"].ToString();
					}
					break;
				}
			}
		}
		return result;
	}

	private List<string> GetExistingMigrationHistory(SqlConnection connection, SqlTransaction transaction)
	{
		using SqlCommand sqlCommand = new SqlCommand("SELECT * From __EFMigrationsHistory", connection, transaction);
		if (connection.State == ConnectionState.Closed)
		{
			connection.Open();
		}
		List<string> list = new List<string>();
		using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
		{
			while (sqlDataReader.Read())
			{
				list.Add(sqlDataReader[0].ToString());
			}
		}
		return list;
	}

	private async Task<(string, string, string)> PrepareFileExtract(IFormFile file)
	{
		string filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
		if (Path.GetExtension(filename) != ".zip")
		{
			throw new CustomException("فرمت فایل باید zip باشد.");
		}
		string tempDirectoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "temp");
		string tempFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "temp", filename);
		if (!Directory.Exists(tempDirectoryPath))
		{
			Directory.CreateDirectory(tempDirectoryPath);
		}
		using (FileStream output = File.Create(tempFilePath))
		{
			await file.CopyToAsync(output);
		}
		string path = filename.Replace(Path.GetExtension(filename), "");
		string text = Path.Combine(tempDirectoryPath, path);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		ZipFile.ExtractToDirectory(tempFilePath, text, overwriteFiles: true);
		return (new DirectoryInfo(text).GetFiles()[0].FullName, tempFilePath, text);
	}

	private void ExecuteSqlQuery(StringBuilder query, SqlConnection connection = null, SqlTransaction transaction = null)
	{
		if (connection != null)
		{
			using (SqlCommand sqlCommand = new SqlCommand(query.ToString(), connection, transaction))
			{
				if (connection.State == ConnectionState.Closed)
				{
					connection.Open();
				}
				sqlCommand.ExecuteNonQuery();
				return;
			}
		}
		SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection"));
		try
		{
			using SqlConnection sqlConnection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
			using SqlCommand sqlCommand2 = new SqlCommand(query.ToString(), sqlConnection, transaction);
			sqlConnection.Open();
			sqlCommand2.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			throw new CustomException(ex.Message);
		}
	}

	private Dictionary<string, List<string>> GetTableFkConstrains(SqlConnection connection, SqlTransaction transaction)
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		using SqlCommand sqlCommand = new SqlCommand("SELECT obj.name AS FK_NAME,\r\n    sch.name AS [schema_name],\r\n    tab1.name AS [table],\r\n    col1.name AS [column],\r\n    tab2.name AS [referenced_table],\r\n    col2.name AS [referenced_column]\r\nFROM sys.foreign_key_columns fkc\r\nINNER JOIN sys.objects obj\r\n    ON obj.object_id = fkc.constraint_object_id\r\nINNER JOIN sys.tables tab1\r\n    ON tab1.object_id = fkc.parent_object_id\r\nINNER JOIN sys.schemas sch\r\n    ON tab1.schema_id = sch.schema_id\r\nINNER JOIN sys.columns col1\r\n    ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id\r\nINNER JOIN sys.tables tab2\r\n    ON tab2.object_id = fkc.referenced_object_id\r\nINNER JOIN sys.columns col2\r\n    ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id", connection, transaction);
		if (connection.State == ConnectionState.Closed)
		{
			connection.Open();
		}
		new List<string>();
		using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		while (sqlDataReader.Read())
		{
			List<string> value = null;
			if (dictionary.TryGetValue(sqlDataReader.GetString(2), out value))
			{
				value.Add(sqlDataReader.GetString(0));
				continue;
			}
			dictionary.Add(sqlDataReader.GetString(2), new List<string> { sqlDataReader.GetString(0) });
		}
		return dictionary;
	}

	private Dictionary<string, List<SqlTableVColumnInfoModel>> GetTableColumnsInfo(SqlConnection connection, SqlTransaction transaction)
	{
		Dictionary<string, List<SqlTableVColumnInfoModel>> dictionary = new Dictionary<string, List<SqlTableVColumnInfoModel>>();
		SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection"));
		using SqlCommand sqlCommand = new SqlCommand("SELECT TABLE_NAME, COLUMN_NAME, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS where TABLE_CATALOG = '" + sqlConnectionStringBuilder.InitialCatalog + "'", connection, transaction);
		if (connection.State == ConnectionState.Closed)
		{
			connection.Open();
		}
		using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		while (sqlDataReader.Read())
		{
			List<SqlTableVColumnInfoModel> value = null;
			if (dictionary.TryGetValue(sqlDataReader.GetString(0), out value))
			{
				value.Add(new SqlTableVColumnInfoModel
				{
					Name = sqlDataReader.GetString(1),
					Default = ((sqlDataReader[2] != null) ? sqlDataReader[2].ToString() : ""),
					IsNullable = (sqlDataReader.GetString(3) == "YES"),
					DataType = sqlDataReader.GetString(4)
				});
			}
			else
			{
				dictionary.Add(sqlDataReader.GetString(0), new List<SqlTableVColumnInfoModel>
				{
					new SqlTableVColumnInfoModel
					{
						Name = sqlDataReader.GetString(1),
						Default = ((sqlDataReader[2] != null) ? sqlDataReader[2].ToString() : ""),
						IsNullable = (sqlDataReader.GetString(3) == "YES"),
						DataType = sqlDataReader.GetString(4)
					}
				});
			}
		}
		return dictionary;
	}

	private Task<string> CreateDeleteTablesQueries()
	{
		return Task.Run(() =>
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>
			{
				{ "AspNetRoles", 1 },
				{ "UserLoanPaymentRequests", 1 },
				{ "AffiliateCommissions", 1 },
				{ "AppSettings", 1 },
				{ "SmsPatterns", 2 },
				{ "AspNetUsers", 2 },
				{ "MemberPurchases", 2 },
				{ "UserLoanPayments", 4 },
				{ "UserLoans", 6 },
				{ "CustomerPurchasePlan", 7 },
				{ "Purchases", 8 },
				{ "Loans", 9 },
				{ "Customers", 10 }
			};
			string? connectionString = _configuration.GetConnectionString("DefaultConnection");
			SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
			using SqlConnection sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			string result = "";
			List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
			using (SqlCommand sqlCommand = new SqlCommand("SELECT TABLE_NAME FROM information_schema.tables where TABLE_CATALOG = @database and TABLE_SCHEMA != 'HangFire'", sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@database", sqlConnectionStringBuilder.InitialCatalog);
				using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					list.Add(new KeyValuePair<string, int>(sqlDataReader.GetString(0), dictionary.ContainsKey(sqlDataReader.GetString(0)) ? dictionary[sqlDataReader.GetString(0)] : 0));
				}
				result = string.Join(';', from x in list
					orderby x.Value
					where !x.Key.Contains("Migrations")
					select "DELETE FROM " + x.Key);
			}
			return result;
		});
	}

	private async Task<string> CreateByJsonStreaming()
	{
		string connectionString = _configuration.GetConnectionString("DefaultConnection");
		SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
		string text = sqlConnectionStringBuilder.InitialCatalog + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
		string text2 = Path.Combine(_hostingEnvironment.WebRootPath, GetBaseFolder(), text);
		string path = text2 + "\\" + sqlConnectionStringBuilder.InitialCatalog + "(" + text + ").bak";
		try
		{
			using (SqlConnection sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();
				List<string> list = new List<string>();
				using (SqlCommand sqlCommand = new SqlCommand("SELECT TABLE_NAME FROM information_schema.tables where TABLE_CATALOG = @database and TABLE_SCHEMA != 'HangFire'", sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@database", sqlConnectionStringBuilder.InitialCatalog);
					using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
					while (sqlDataReader.Read())
					{
						list.Add(sqlDataReader.GetString(0));
					}
				}
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
				}
				using StreamWriter textWriter = new StreamWriter(path);
				using JsonTextWriter jsonTextWriter = new JsonTextWriter(textWriter)
				{
					Formatting = Formatting.Indented,
					Indentation = 2,
					IndentChar = ' '
				};
				jsonTextWriter.WriteStartArray();
				foreach (string item in list)
				{
					jsonTextWriter.WriteStartObject();
					jsonTextWriter.WritePropertyName("tableName");
					jsonTextWriter.WriteValue(item);
					jsonTextWriter.WritePropertyName("rows");
					jsonTextWriter.WriteStartArray();
					using (SqlCommand sqlCommand2 = new SqlCommand("SELECT * FROM " + item, sqlConnection))
					{
						using SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
						while (sqlDataReader2.Read())
						{
							jsonTextWriter.WriteStartObject();
							for (int i = 0; i < sqlDataReader2.FieldCount; i++)
							{
								jsonTextWriter.WritePropertyName(sqlDataReader2.GetName(i));
								jsonTextWriter.WriteValue(sqlDataReader2.GetValue(i));
							}
							jsonTextWriter.WriteEndObject();
						}
					}
					jsonTextWriter.WriteEndArray();
					jsonTextWriter.WriteEndObject();
				}
				jsonTextWriter.WriteEndArray();
			}
			ZipFile.CreateFromDirectory(text2, Path.Combine(_hostingEnvironment.WebRootPath, GetBaseFolder(), text + ".zip"));
			if (Directory.Exists(text2))
			{
				Directory.Delete(text2, recursive: true);
			}
			return "Backup done";
		}
		catch (Exception ex)
		{
			if (!File.Exists(path))
			{
				File.Delete(path);
			}
			throw new CustomException(ex.Message);
		}
	}

	private async Task<string> CreateBySqlServerBackupQuery()
	{
		string text = Path.Combine(_hostingEnvironment.WebRootPath, GetBaseFolder());
		SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection"));
		string text2 = text + "\\" + sqlConnectionStringBuilder.InitialCatalog + "(" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ").bak";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		using SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
		string cmdText = "BACKUP DATABASE [" + sqlConnectionStringBuilder.InitialCatalog + "] TO DISK='" + text2 + "'";
		using SqlCommand command = new SqlCommand(cmdText, connection);
		_ = 1;
		try
		{
			await connection.OpenAsync();
			await command.ExecuteNonQueryAsync();
			return "Backup done";
		}
		catch (Exception ex)
		{
			return "Backup encountered a problem\n" + ex.Message;
		}
	}

	private string GetBaseFolder()
	{
		return "backups";
	}

	private string GetFormatedSize(long length)
	{
		string[] array = new string[5] { "بایت", "کیلو بایت", "مگابایت", "گیگابایت", "ترابایت" };
		int num = 0;
		while (length >= 1024 && num < array.Length - 1)
		{
			num++;
			length /= 1024;
		}
		return $"{length:0.##} {array[num]}";
	}
}
