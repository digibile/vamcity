using Ghestine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using __DBNS__;
namespace Ghestine.API.Core;

public interface IVirtualCardService
{
    /// از داخل سرویس وام صدا بزنید: بعد از «تأیید و امضای قرارداد» یا هر وضعیتی که تعیین کردید.
    Task<VirtualCard> IssueForLoanAsync(Guid loanId, Guid userId, string holderName, decimal limit, Guid tenantId, CancellationToken ct = default);
}
public class VirtualCardService : IVirtualCardService
{
    private readonly AppDbContext _db; private readonly ICardCrypto _c; private readonly IConfiguration _cfg;
    public VirtualCardService(AppDbContext db, ICardCrypto c, IConfiguration cfg) { _db = db; _c = c; _cfg = cfg; }

    public async Task<VirtualCard> IssueForLoanAsync(Guid loanId, Guid userId, string holderName, decimal limit, Guid tenantId, CancellationToken ct = default)
    {
        var ex = await _db.Set<VirtualCard>().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.LoanId == loanId && x.Status != "Closed", ct);
        if (ex != null) return ex;
        var bin = _cfg["Cards:Bin"] ?? "910100";
        string pan; do { pan = _c.NewPan(bin); } while (await _db.Set<VirtualCard>().AnyAsync(x => x.PanHash == _c.Hash(pan), ct));
        var cvv = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100, 1000).ToString();
        var months = int.TryParse(_cfg["Cards:ValidMonths"], out var m) ? m : 36;
        var exp = DateTime.UtcNow.AddMonths(months);
        var card = new VirtualCard
        {
            Id = Guid.NewGuid(), TenantId = tenantId, LoanId = loanId, UserId = userId, HolderName = holderName, Limit = limit,
            PerTxnLimit = limit, Bin = bin[..4], Last4 = pan[^4..], PanHash = _c.Hash(pan),
            PanEnc = _c.Protect(pan), CvvEnc = _c.Protect(cvv), ExpMonth = exp.Month, ExpYear = exp.Year
        };
        _db.Set<VirtualCard>().Add(card);
        _db.Set<VirtualCardTxn>().Add(new VirtualCardTxn { CardId = card.Id, Type = "Issue", Amount = limit, Note = "صدور خودکار برای وام" });
        await _db.SaveChangesAsync(ct);
        return card;
    }
}
