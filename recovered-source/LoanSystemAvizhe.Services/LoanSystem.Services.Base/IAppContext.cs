using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAppContext
{
	User User { get; }

	bool TryGetGuestUserId(out long guestId);

	void EnsureGuestCookieRemoved();

	void ClearCache();

	string GetSystemCurrency();
}
