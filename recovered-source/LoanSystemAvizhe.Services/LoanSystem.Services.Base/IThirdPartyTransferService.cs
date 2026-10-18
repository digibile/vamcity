using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.Landa;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Models.Digibile;
using LoanSystemAvizhe.Services.Models.DynamicQueryModel;

namespace LoanSystem.Services.Base;

public interface IThirdPartyTransferService
{
    Task<Guid?> GetLandaId(string entityId, string entityType);

    Task<List<ThirdPartyTransfer>> GetThirdPartyTransfers(string entityId, string entityType, ThirdPart? thirdParty);

    Task<(int, List<ThirdPartyTransferModel>)> GetNotSentMembers(DynamicQueryForThirdPartyModel dynamicQuery);

    Task<(int, List<ThirdPartyTransferModel>)> GetNotSentLoans(DynamicQueryForThirdPartyModel dynamicQuery);

    Task<(int, List<ThirdPartyTransferModel>)> GetNotSentLoanPayments(DynamicQueryForThirdPartyModel dynamicQuery);

    Task<Dictionary<string, dynamic>> SendMember(List<string> ids, ThirdPart thirdParty, List<GetCustomerResponse> currentLandaCustomers = null, Dictionary<string, string> extraData = null, List<ShowUserResponse> currentDigibileCustomers = null);

    Task<Dictionary<string, dynamic>> SendInvoice(List<string> ids, ThirdPart thirdParty);

    Task<Dictionary<string, dynamic>> SendInvoicePayment(List<string> ids, ThirdPart thirdParty);

    Task<ThirdPartyTransfer> GetById(Guid id, params string[] includs);

    Task RegisterOrUpdateDigibileMember(string id, string password);

    Task Insert(ThirdPartyTransfer entity);

    Task Update(ThirdPartyTransfer entity);

    Task Delete(ThirdPartyTransfer entity);
}
