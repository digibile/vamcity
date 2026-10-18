using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ExcelDataReader;
using LoanSystem.Core;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class ExcelImportService : BaseService, IExcelImportService
{
    private readonly IAppContext _appContext;

    private readonly UserManager _userManager;

    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly ICustomerService _customerService;

    private readonly IAgentService _agentService;

    private readonly IUserPaymentService _userPaymentService;

    private readonly AppConfig _appConfig;

    public ExcelImportService(AppDbContext context, IAppContext appContext, UserManager userManager, IHostingEnvironment hostingEnvironment, ICurrentUserService currentUserService, ICustomerService customerService, IAgentService agentService, IUserPaymentService userPaymentService, IOptions<AppConfig> appConfig)
        : base(context, currentUserService)
    {
        _appContext = appContext;
        _userManager = userManager;
        _hostingEnvironment = hostingEnvironment;
        _customerService = customerService;
        _agentService = agentService;
        _userPaymentService = userPaymentService;
        _appConfig = appConfig.Value;
    }

    public Task ImportStarter()
    {
        return Table<Loan>().ToListAsync();
    }

    public Task<List<MemberImportPreviewModel>> PreviewImportMembers(ImportFileModel model)
    {
        return Task.Run(async () =>
        {
            List<MemberImportPreviewModel> models = new List<MemberImportPreviewModel>();
            IFormFile file = model.File;
            string path = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            path = Guid.NewGuid().ToString() + Path.GetExtension(path);
            string text = Path.Combine(_hostingEnvironment.WebRootPath, "temp");
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            string path2 = Path.Combine(text, path);
            FileInfo fileInfo = new FileInfo(path2);
            if (fileInfo.Extension != ".xlsx" && fileInfo.Extension != ".xls")
            {
                throw new CustomException("فرمت فایل باید xlsx یا xls اکسل باشد.");
            }
            (await _userManager.GetAllUser()).ToList();
            List<User> agents = await _agentService.GetAllAgents();
            using (FileStream output = File.Create(path2))
            {
                await file.CopyToAsync(output);
            }
            using (FileStream fileStream = File.Open(path2, FileMode.Open, FileAccess.Read))
            {
                using IExcelDataReader excelDataReader = ExcelReaderFactory.CreateReader(fileStream);
                do
                {
                    int num = 0;
                    while (excelDataReader.Read())
                    {
                        num++;
                        if (num != 1)
                        {
                            try
                            {
                                MemberImportPreviewModel memberImportPreviewModel = ConvertFromExcelReader_Member(excelDataReader);
                                string text2 = ValidateMemberModel(memberImportPreviewModel, agents);
                                if (!string.IsNullOrWhiteSpace(text2))
                                {
                                    memberImportPreviewModel.SystemMsg = text2;
                                }
                                models.Add(memberImportPreviewModel);
                            }
                            catch (Exception innerException)
                            {
                                throw new CustomException("خطا در خواندن اطلاعات از فایل اکسل", innerException);
                            }
                        }
                    }
                }
                while (excelDataReader.NextResult());
            }
            if (File.Exists(path2))
            {
                File.Delete(path2);
            }
            if (models.Any((MemberImportPreviewModel x) => x.SystemMsg != ""))
            {
                models = models.OrderByDescending((MemberImportPreviewModel x) => x.SystemMsg).ToList();
            }
            return models;
        });
    }

    public Task<MemberImportResultModel> SubmitImportMembers(MemberImportSubmitModel submitModel)
    {
        return Task.Run(async () =>
        {
            MemberImportResultModel result = new MemberImportResultModel();
            if (!submitModel.ImportPreviewModels.Any())
            {
                return result;
            }
            try
            {
                List<User> members = await _context.Users.ToListAsync();
                List<User> agents = (await _userManager.GetUsersInRoleAsync("Agent")).ToList();
                Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
                foreach (MemberImportPreviewModel model in submitModel.ImportPreviewModels)
                {
                    string text = ValidateMemberModel(model, agents);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        result.Failed++;
                        result.Errors.Add("کد ملی: " + model.NationalCode + text);
                    }
                    else
                    {
                        User user = members.FirstOrDefault((User x) => x.NationalCode == model.NationalCode);
                        if (user == null)
                        {
                            user = new User
                            {
                                UserRoleNames = new List<string> { "Member" }
                            };
                            string passwordHash = new PasswordHasher<User>().HashPassword(user, model.NationalCode);
                            user.PasswordHash = passwordHash;
                        }
                        user.CustomerId = _currentUserService.CustomerId;
                        user.UserName = GenerateUserName(customer.Code, model.NationalCode);
                        user.FirstName = model.FirstName;
                        user.LastName = model.LastName;
                        user.FatherName = model.FatherName;
                        user.Mobile = model.Mobile;
                        user.Phone = model.Phone;
                        user.NationalCode = model.NationalCode;
                        user.Address = model.Address;
                        user.Gender = model.Gender == "زن";
                        user.RegDate2 = (model.RegDate2 ?? "").ToGregorianDateTime().Value;
                        user.BirthDate = (model.BirthDate ?? "").ToGregorianDateTime().Value;
                        user.Familiar1FirstName = model.Familiar1FirstName;
                        user.Familiar1LastName = model.Familiar1LastName;
                        user.Familiar1Mobile = model.Familiar1Mobile;
                        user.Familiar2FirstName = model.Familiar2FirstName;
                        user.Familiar2LastName = model.Familiar2LastName;
                        user.Familiar2Mobile = model.Familiar2Mobile;
                        user.IsActive = true;
                        user.LockoutEnabled = false;
                        if (!string.IsNullOrWhiteSpace(model.Agent))
                        {
                            user.AgentId = agents.FirstOrDefault((User x) => x.UserName == model.Agent)?.Id;
                        }
                        if (user.Id == 0L)
                        {
                            user.RegDate = DateTime.Now;
                            await _userManager.Insert(user, model.NationalCode, user.UserRoleNames);
                            result.MemberNew++;
                        }
                        else
                        {
                            await _userManager.UpdateAsync(user);
                            result.MemberUpdate++;
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception innerException)
            {
                throw new CustomException("خطا در ذخیره سازی اطلاعات", innerException);
            }
            return result;
        });
    }

    public Task<List<InstallmentImportPreviewModel>> PreviewImportInstallments(ImportFileModel importModel)
    {
        return Task.Run(async () =>
        {
            List<InstallmentImportPreviewModel> models = new List<InstallmentImportPreviewModel>();
            IFormFile file = importModel.File;
            string path = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            path = Guid.NewGuid().ToString() + Path.GetExtension(path);
            string text = Path.Combine(_hostingEnvironment.WebRootPath, "temp");
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            string path2 = Path.Combine(text, path);
            FileInfo fileInfo = new FileInfo(path2);
            if (fileInfo.Extension != ".xlsx" && fileInfo.Extension != ".xls")
            {
                throw new CustomException("فرمت فایل باید xlsx یا xls اکسل باشد.");
            }
            await _context.Loans.AsNoTracking().ToListAsync();
            List<User> members = await _context.Users.AsNoTracking().ToListAsync();
            List<UserLoan> userLoans = await _context.UserLoans.AsNoTracking().ToListAsync();
            List<UserLoanPayment> userLoanPayments = await _context.UserLoanPayments.AsNoTracking().ToListAsync();
            using (FileStream output = File.Create(path2))
            {
                await file.CopyToAsync(output);
            }
            using (FileStream fileStream = File.Open(path2, FileMode.Open, FileAccess.Read))
            {
                using IExcelDataReader excelDataReader = ExcelReaderFactory.CreateReader(fileStream);
                do
                {
                    int num = 0;
                    while (excelDataReader.Read())
                    {
                        num++;
                        if (num != 1)
                        {
                            try
                            {
                                InstallmentImportPreviewModel installmentImportPreviewModel = ConvertFromExcelReader_Installment(excelDataReader);
                                string text2 = ValidateInstallmentModel(installmentImportPreviewModel, members, userLoans, userLoanPayments);
                                if (!string.IsNullOrWhiteSpace(text2))
                                {
                                    installmentImportPreviewModel.SystemMsg = text2;
                                }
                                models.Add(installmentImportPreviewModel);
                            }
                            catch (Exception innerException)
                            {
                                throw new CustomException("خطا در خواندن اطلاعات از فایل اکسل", innerException);
                            }
                        }
                    }
                }
                while (excelDataReader.NextResult());
            }
            if (File.Exists(path2))
            {
                File.Delete(path2);
            }
            if (models.Any((InstallmentImportPreviewModel x) => x.SystemMsg != ""))
            {
                models = models.OrderByDescending((InstallmentImportPreviewModel x) => x.SystemMsg).ToList();
            }
            return models;
        });
    }

    public Task<InstallmentImportResultModel> SubmitImportInstallments(InstallmentImportSubmitModel submitModel)
    {
        return Task.Run(async () =>
        {
            InstallmentImportResultModel result = new InstallmentImportResultModel();
            if (!submitModel.InstallmentImportPreviewModels.Any())
            {
                return result;
            }
            try
            {
                List<string> nationalCodes = submitModel.InstallmentImportPreviewModels.Select((InstallmentImportPreviewModel x) => x.NationalCode).ToList();
                List<User> members = await (from x in _context.Users.AsNoTracking()
                                            where nationalCodes.Contains(x.NationalCode)
                                            select x).ToListAsync();
                List<int> userLoanCodes = submitModel.InstallmentImportPreviewModels.Select((InstallmentImportPreviewModel x) => x.UserLoanCode).ToList();
                List<UserLoan> userLoans = await (from x in _context.UserLoans.AsNoTracking()
                                                  where userLoanCodes.Contains(x.Code)
                                                  select x).ToListAsync();
                List<Guid> userLoanIds = userLoans.Select((UserLoan x) => x.Id).ToList();
                List<UserLoanPayment> userLoanPayments = await (from x in _context.UserLoanPayments.AsNoTracking().Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan)
                        .Include((UserLoanPayment x) => x.UserLoanPaymentOthers)
                                                                where userLoanIds.Contains(x.UserLoanId)
                                                                select x).ToListAsync();
                foreach (InstallmentImportPreviewModel model in submitModel.InstallmentImportPreviewModels)
                {
                    string text = ValidateInstallmentModel(model, members, userLoans, userLoanPayments);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        result.Failed++;
                        result.Errors.Add("کد ملی: " + model.NationalCode + " *** " + text);
                    }
                    else
                    {
                        User member = members.FirstOrDefault((User x) => x.NationalCode == model.NationalCode);
                        UserLoan userLoan = userLoans.FirstOrDefault((UserLoan x) => x.Code == model.UserLoanCode && x.UserId == member.Id && (!x.IsCheckouted.HasValue || !x.IsCheckouted.Value));
                        UserLoanPayment userLoanPayment = userLoanPayments.FirstOrDefault((UserLoanPayment x) => x.UserLoanId == userLoan.Id && x.InstallmentNumber == model.InstallmentNumber);
                        userLoanPayment.Amount = model.Installment;
                        userLoanPayment.SaveDate = model.PaymentDate.ToGregorianDateTime().Value;
                        userLoanPayment.SavedBy = "دریافت فایل توسط [" + _appContext.User?.FullName + "]";
                        userLoanPayment.PaymentMethod = model.PaymentMethod;
                        LoanPaymentModel userLoanPaymentModel = new LoanPaymentModel(userLoanPayment);
                        UserLoanPayment userLoanPayment2 = userLoanPayment;
                        userLoanPayment2.DelayDay = await _userPaymentService.CalcDelayDay(userLoanPaymentModel);
                        userLoanPayment2 = userLoanPayment;
                        userLoanPayment2.DelayPrice = await _userPaymentService.CalcDelayPrice(userLoanPaymentModel);
                        await _userPaymentService.Update(userLoanPayment, sendSms: false);
                        result.PaymentUpdate++;
                    }
                }
            }
            catch (Exception innerException)
            {
                throw new CustomException("خطا در ذخیره اطلاعات", innerException);
            }
            return result;
        });
    }

    public Task<List<SavedMoneyImportPreviewModel>> PreviewImportSavedMoney(ImportFileModel model)
    {
        return Task.Run(async () =>
        {
            List<SavedMoneyImportPreviewModel> models = new List<SavedMoneyImportPreviewModel>();
            IFormFile file = model.File;
            string path = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            path = Guid.NewGuid().ToString() + Path.GetExtension(path);
            string text = Path.Combine(_hostingEnvironment.WebRootPath, "temp");
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            string path2 = Path.Combine(text, path);
            FileInfo fileInfo = new FileInfo(path2);
            if (fileInfo.Extension != ".xlsx" && fileInfo.Extension != ".xls")
            {
                throw new CustomException("فرمت فایل باید xlsx یا xls اکسل باشد.");
            }
            List<UserSavedMoney> userSavedMonies = await _context.UserSavedMonies.AsNoTracking().ToListAsync();
            using (FileStream output = File.Create(path2))
            {
                await file.CopyToAsync(output);
            }
            using (FileStream output = File.Open(path2, FileMode.Open, FileAccess.Read))
            {
                using IExcelDataReader reader = ExcelReaderFactory.CreateReader(output);
                do
                {
                    int row = 0;
                    while (reader.Read())
                    {
                        row++;
                        if (row != 1)
                        {
                            try
                            {
                                SavedMoneyImportPreviewModel model2 = ConvertFromExcelReader_SavedMoney(reader);
                                User member = await _context.Users.FirstOrDefaultAsync((User x) => x.NationalCode == model2.NationalCode);
                                string text2 = ValidateSavedMoneyModel(model2, member, userSavedMonies);
                                if (!string.IsNullOrWhiteSpace(text2))
                                {
                                    model2.SystemMsg = text2;
                                }
                                models.Add(model2);
                            }
                            catch (Exception innerException)
                            {
                                throw new CustomException("خطا در خواندن اطلاعات از فایل اکسل", innerException);
                            }
                        }
                    }
                }
                while (reader.NextResult());
            }
            if (File.Exists(path2))
            {
                File.Delete(path2);
            }
            if (models.Any((SavedMoneyImportPreviewModel x) => x.SystemMsg != ""))
            {
                models = models.OrderByDescending((SavedMoneyImportPreviewModel x) => x.SystemMsg).ToList();
            }
            return models;
        });
    }

    public Task<SavedMoneyImportResultModel> SubmitImportSavedMoney(SavedMoneyImportSubmitModel submitModel)
    {
        return Task.Run(async () =>
        {
            SavedMoneyImportResultModel result = new SavedMoneyImportResultModel();
            if (!submitModel.ImportPreviewModels.Any())
            {
                return result;
            }
            try
            {
                List<User> members = await _context.Users.ToListAsync();
                List<UserSavedMoney> list = await _context.UserSavedMonies.ToListAsync();
                foreach (SavedMoneyImportPreviewModel model in submitModel.ImportPreviewModels)
                {
                    User member = members.FirstOrDefault((User x) => x.NationalCode == model.NationalCode);
                    string text = ValidateSavedMoneyModel(model, member, list);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        result.Failed++;
                        result.Errors.Add("کد ملی: " + model.NationalCode + text);
                    }
                    else
                    {
                        UserSavedMoney userSavedMoney = list.FirstOrDefault((UserSavedMoney x) => x.MemberId == member.Id && x.FileMonth == model.Month && !x.Description.Contains("اول دوره"));
                        if (userSavedMoney == null)
                        {
                            userSavedMoney = CreateNewUserSavedMoney(model);
                            userSavedMoney.Member = member;
                            member.TotalSavedMoney += model.Amount;
                            if (!list.Any((UserSavedMoney x) => x.MemberId == member.Id))
                            {
                                UserSavedMoney userSavedMoney2 = CreateNewUserSavedMoney(model, isFirstPeriod: true);
                                userSavedMoney2.Member = member;
                                _context.UserSavedMonies.Add(userSavedMoney2);
                                member.TotalSavedMoney = model.TotalSavedMoney;
                            }
                        }
                        else
                        {
                            UserSavedMoney userSavedMoney3 = list.FirstOrDefault((UserSavedMoney x) => x.MemberId == member.Id && x.FileMonth == model.Month && x.Description.Contains("اول دوره"));
                            if (userSavedMoney3 != null)
                            {
                                userSavedMoney3.Amount = model.TotalSavedMoney - model.Amount;
                                _context.UserSavedMonies.Update(userSavedMoney3);
                            }
                        }
                        if (userSavedMoney.Id != Guid.Empty)
                        {
                            userSavedMoney.Amount = model.Amount;
                            userSavedMoney.Description = "فایل - " + model.Month;
                            userSavedMoney.FileMonth = model.Month;
                            userSavedMoney.TargetMonth = (model.Month + "/01").ToGregorianDateTime().Value;
                            userSavedMoney.SaveDate = DateTime.Now;
                            userSavedMoney.SavedBy = "دریافت فایل توسط [" + _appContext.User?.FullName + "]";
                            userSavedMoney.Member = member;
                            _context.UserSavedMonies.Update(userSavedMoney);
                            result.SavedMoneyUpdate++;
                            member.TotalSavedMoney = _context.UserSavedMonies.Local.Where((UserSavedMoney x) => x.MemberId == member.Id).Sum((UserSavedMoney x) => x.Amount);
                        }
                        else
                        {
                            _context.UserSavedMonies.Add(userSavedMoney);
                            result.SavedMoneyNew++;
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception innerException)
            {
                throw new CustomException("خطا در ذخیره اطلاعات", innerException);
            }
            return result;
        });
    }

    public Task<List<CompareRemainingsPreviewModel>> PreviewCompareRemainings(ImportFileModel importModel)
    {
        return Task.Run(async () =>
        {
            List<CompareRemainingsPreviewModel> models = new List<CompareRemainingsPreviewModel>();
            IFormFile file = importModel.File;
            string path = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            path = Guid.NewGuid().ToString() + Path.GetExtension(path);
            string text = Path.Combine(_hostingEnvironment.WebRootPath, "temp");
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            string path2 = Path.Combine(text, path);
            FileInfo fileInfo = new FileInfo(path2);
            if (fileInfo.Extension != ".xlsx" && fileInfo.Extension != ".xls")
            {
                throw new CustomException("فرمت فایل باید xlsx یا xls اکسل باشد.");
            }
            List<Loan> loans = await _context.Loans.AsNoTracking().ToListAsync();
            List<User> members = await _context.Users.AsNoTracking().ToListAsync();
            List<UserLoan> userLoans = await _context.UserLoans.AsNoTracking().ToListAsync();
            List<UserLoanPayment> userLoanPayments = await _context.UserLoanPayments.AsNoTracking().ToListAsync();
            using (FileStream output = File.Create(path2))
            {
                await file.CopyToAsync(output);
            }
            using (FileStream fileStream = File.Open(path2, FileMode.Open, FileAccess.Read))
            {
                using IExcelDataReader excelDataReader = ExcelReaderFactory.CreateReader(fileStream);
                do
                {
                    int num = 0;
                    while (excelDataReader.Read())
                    {
                        num++;
                        if (num != 1)
                        {
                            try
                            {
                                CompareRemainingsPreviewModel compareRemainingsPreviewModel = ConvertFromExcelReader_CompareRemainings(excelDataReader);
                                string text2 = ValidateCompareRemainingModel(compareRemainingsPreviewModel, loans, members, userLoans, userLoanPayments);
                                if (!string.IsNullOrWhiteSpace(text2))
                                {
                                    compareRemainingsPreviewModel.SystemMsg = text2;
                                }
                                models.Add(compareRemainingsPreviewModel);
                            }
                            catch (Exception innerException)
                            {
                                throw new CustomException("خطا در خواندن اطلاعات از فایل اکسل", innerException);
                            }
                        }
                    }
                }
                while (excelDataReader.NextResult());
            }
            if (File.Exists(path2))
            {
                File.Delete(path2);
            }
            if (models.Any((CompareRemainingsPreviewModel x) => x.SystemMsg != ""))
            {
                models = models.OrderByDescending((CompareRemainingsPreviewModel x) => x.SystemMsg).ToList();
            }
            return models;
        });
    }

    private string ValidateInstallmentModel(InstallmentImportPreviewModel model, List<User> members, List<UserLoan> userLoans, List<UserLoanPayment> payments)
    {
        string text = "";
        string paymentDate = model.PaymentDate;
        if (paymentDate == null || paymentDate.Length != 10 || !model.PaymentDate.Contains('/'))
        {
            text += "فرمت تاریخ واریز باید به صورت yyyy/mm/dd باشد. مثال: 1404/10/10. ";
        }
        else if (!model.PaymentDate.ToGregorianDateTime().HasValue)
        {
            text += "فرمت تاریخ واریز باید به صورت yyyy/mm/dd باشد. مثال: 1404/10/10. ";
        }
        if (model.Installment < 0m)
        {
            text += "مبلغ قسط صحیح نمی باشد. ";
        }
        if (model.UserLoanCode == 0 || !userLoans.Any((UserLoan x) => x.Code == model.UserLoanCode))
        {
            text += "شماره وام صحیح نمی باشد. ";
        }
        UserLoan userLoan = null;
        if (!string.IsNullOrWhiteSpace(model.NationalCode))
        {
            User member = members.FirstOrDefault((User x) => x.NationalCode == model.NationalCode);
            if (member != null)
            {
                model.FullName = member.FullName;
                userLoan = userLoans.FirstOrDefault((UserLoan x) => x.Code == model.UserLoanCode && x.UserId == member.Id);
            }
            else
            {
                text += "مشتری با این کد ملی یافت نشد";
            }
        }
        else
        {
            userLoan = userLoans.FirstOrDefault((UserLoan x) => x.Code == model.UserLoanCode);
        }
        if (userLoan == null)
        {
            text += "وام یافت نشد. ";
        }
        else if (userLoan.IsCheckouted == true)
        {
            text += "وام تسویه شده است. ";
        }
        if (userLoan != null && !payments.Any((UserLoanPayment x) => x.UserLoanId == userLoan.Id && x.InstallmentNumber == model.InstallmentNumber))
        {
            text += "شماره قسط در بین اقساط وجود ندارد. ";
        }
        return text;
    }

    private string ValidateSavedMoneyModel(SavedMoneyImportPreviewModel model, User member, List<UserSavedMoney> userSavedMonies)
    {
        string text = "";
        if (string.IsNullOrWhiteSpace(model.NationalCode))
        {
            text += "کد ملی خالی است. ";
        }
        if (string.IsNullOrWhiteSpace(model.Month))
        {
            text += "ماه خالی است. ";
        }
        else if (!model.Month.Contains('/') || model.Month.Length != 7)
        {
            text += "فرمت ماه باید به صورت yyyy/mm باشد. مثال: 1399/05. ";
        }
        if (model.Amount < 0m)
        {
            text += "مبلغ پس انداز صحیح نمی باشد.";
        }
        if (model.TotalSavedMoney < 0m)
        {
            text += "مبلغ کل پس انداز صحیح نمی باشد.";
        }
        if (member != null)
        {
            decimal num = userSavedMonies.Where((UserSavedMoney x) => x.MemberId == member.Id && x.FileMonth != model.Month).Sum((UserSavedMoney x) => x.Amount);
            if (num != 0m && num != model.TotalSavedMoney - model.Amount)
            {
                text += "مبلغ کل پس انداز موجود در فایل با پس انداز فعلی عضو مغایرت دارد.";
            }
        }
        else
        {
            text += "عضوی با این کد پرسنلی یافت نشد";
        }
        return text;
    }

    private string ValidateMemberModel(MemberImportPreviewModel model, List<User> agents)
    {
        string text = "";
        if (string.IsNullOrWhiteSpace(model.FirstName))
        {
            text += "نام خالی است. ";
        }
        if (string.IsNullOrWhiteSpace(model.LastName))
        {
            text += "نام خانوادگی خالی است. ";
        }
        if (string.IsNullOrWhiteSpace(model.FatherName))
        {
            text += "نام پدر خالی است. ";
        }
        if (string.IsNullOrWhiteSpace(model.Mobile))
        {
            text += "شماره موبایل خالی است. ";
        }
        if (string.IsNullOrWhiteSpace(model.NationalCode))
        {
            text += "کد ملی خالی است. ";
        }
        if (string.IsNullOrWhiteSpace(model.Address))
        {
            text += "آدرس خالی است. ";
        }
        if (model.Gender.ToString() != "زن" && model.Gender.ToString() != "مرد")
        {
            text += "مقدار جنسیت باید به صورت زن یا مرد باشد.";
        }
        if (string.IsNullOrWhiteSpace(model.RegDate2))
        {
            text += "تاریخ عضویت خالی است. ";
        }
        else if (!model.RegDate2.Contains('/') || model.RegDate2.Length != 10)
        {
            text += "فرمت تاریخ عضویت باید به صورت yyyy/mm/dd باشد. مثال: 1402/08/10";
        }
        if (string.IsNullOrWhiteSpace(model.BirthDate))
        {
            text += "تاریخ تولد خالی است. ";
        }
        else if (!model.BirthDate.Contains('/') || model.BirthDate.Length != 10)
        {
            text += "فرمت تاریخ تولد باید به صورت yyyy/mm/dd باشد. مثال: 1402/08/10";
        }
        if (!string.IsNullOrWhiteSpace(model.Agent) && !agents.Any((User x) => x.UserName == model.Agent))
        {
            text += "نماینده یافت نشد. ";
        }
        return text;
    }

    private string ValidateCompareRemainingModel(CompareRemainingsPreviewModel model, List<Loan> loans, List<User> members, List<UserLoan> userLoans, List<UserLoanPayment> payments)
    {
        string text = "";
        if (string.IsNullOrWhiteSpace(model.NationalCode))
        {
            text += "کد ملی خالی است. ";
        }
        if (model.Remain < 0m)
        {
            text += "مبلغ باقیمانده صحیح نمی باشد.";
        }
        if (model.LoanCode == 0 || !loans.Any((Loan x) => x.Code == model.LoanCode))
        {
            text += "شماره وام صحیح نمی باشد.";
        }
        else
        {
            User member = members.FirstOrDefault((User x) => x.NationalCode == model.NationalCode);
            if (member != null)
            {
                Guid loanId = loans.First((Loan x) => x.Code == model.LoanCode).Id;
                List<UserLoan> source = userLoans.Where((UserLoan x) => x.LoanId == loanId && x.UserId == member.Id).ToList();
                if (!source.Any())
                {
                    text += "وام یافت نشد.";
                }
                else
                {
                    UserLoan userLoan = source.FirstOrDefault((UserLoan x) => !x.IsCheckouted.HasValue || !x.IsCheckouted.Value);
                    if (userLoan == null)
                    {
                        text += "وام های مرتبط با این کد تسویه شده اند.";
                    }
                    else if (userLoan.Remain != model.Remain)
                    {
                        text = text + "مغایرت در مبالغ: باقیمانده در فایل:" + model.Remain + " - باقیمانده در سایت: " + userLoan.Remain;
                    }
                }
            }
            else
            {
                text += "عضوی با این کد پرسنلی یافت نشد";
            }
        }
        return text;
    }

    private InstallmentImportPreviewModel ConvertFromExcelReader_Installment(IExcelDataReader reader)
    {
        InstallmentImportPreviewModel installmentImportPreviewModel = new InstallmentImportPreviewModel();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            object value = reader.GetValue(i);
            switch (i)
            {
                case 0:
                    installmentImportPreviewModel.NationalCode = value?.ToString();
                    break;
                case 1:
                    {
                        int.TryParse(value?.ToString(), out var result2);
                        installmentImportPreviewModel.UserLoanCode = result2;
                        break;
                    }
                case 2:
                    {
                        int.TryParse(value?.ToString(), out var result4);
                        installmentImportPreviewModel.InstallmentNumber = result4;
                        break;
                    }
                case 3:
                    {
                        decimal.TryParse(value?.ToString(), out var result3);
                        installmentImportPreviewModel.Installment = result3;
                        break;
                    }
                case 4:
                    installmentImportPreviewModel.PaymentDate = value?.ToString();
                    break;
                case 5:
                    {
                        int.TryParse(value?.ToString(), out var result);
                        installmentImportPreviewModel.PaymentMethod = (PaymentMethods)result;
                        break;
                    }
            }
        }
        return installmentImportPreviewModel;
    }

    private SavedMoneyImportPreviewModel ConvertFromExcelReader_SavedMoney(IExcelDataReader reader)
    {
        SavedMoneyImportPreviewModel savedMoneyImportPreviewModel = new SavedMoneyImportPreviewModel();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            object value = reader.GetValue(i);
            switch (i)
            {
                case 0:
                    savedMoneyImportPreviewModel.NationalCode = value?.ToString();
                    break;
                case 1:
                    savedMoneyImportPreviewModel.Month = value?.ToString();
                    break;
                case 2:
                    savedMoneyImportPreviewModel.Number = value?.ToString();
                    break;
                case 3:
                    {
                        decimal.TryParse(value?.ToString(), out var result2);
                        savedMoneyImportPreviewModel.Amount = result2;
                        break;
                    }
                case 4:
                    {
                        decimal.TryParse(value?.ToString(), out var result);
                        savedMoneyImportPreviewModel.TotalSavedMoney = result;
                        break;
                    }
            }
        }
        return savedMoneyImportPreviewModel;
    }

    private MemberImportPreviewModel ConvertFromExcelReader_Member(IExcelDataReader reader)
    {
        MemberImportPreviewModel memberImportPreviewModel = new MemberImportPreviewModel();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            object obj = reader.GetValue(i)?.ToString().Replace('ي', 'ی').ToEnglishDigits() ?? "";
            switch (i)
            {
                case 0:
                    memberImportPreviewModel.FirstName = obj?.ToString();
                    break;
                case 1:
                    memberImportPreviewModel.LastName = obj?.ToString();
                    break;
                case 2:
                    memberImportPreviewModel.FatherName = obj?.ToString();
                    break;
                case 3:
                    memberImportPreviewModel.Mobile = obj?.ToString();
                    break;
                case 4:
                    memberImportPreviewModel.Phone = obj?.ToString();
                    break;
                case 5:
                    memberImportPreviewModel.NationalCode = obj?.ToString();
                    break;
                case 6:
                    memberImportPreviewModel.BirthDate = obj?.ToString();
                    break;
                case 7:
                    memberImportPreviewModel.Address = obj?.ToString();
                    break;
                case 8:
                    memberImportPreviewModel.Gender = obj?.ToString();
                    break;
                case 9:
                    memberImportPreviewModel.RegDate2 = obj?.ToString();
                    break;
                case 10:
                    memberImportPreviewModel.Familiar1FirstName = obj?.ToString();
                    break;
                case 11:
                    memberImportPreviewModel.Familiar1LastName = obj?.ToString();
                    break;
                case 12:
                    memberImportPreviewModel.Familiar1Mobile = obj?.ToString();
                    break;
                case 13:
                    memberImportPreviewModel.Familiar2FirstName = obj?.ToString();
                    break;
                case 14:
                    memberImportPreviewModel.Familiar2LastName = obj?.ToString();
                    break;
                case 15:
                    memberImportPreviewModel.Familiar2Mobile = obj?.ToString();
                    break;
                case 16:
                    memberImportPreviewModel.Agent = obj?.ToString();
                    break;
            }
        }
        return memberImportPreviewModel;
    }

    private CompareRemainingsPreviewModel ConvertFromExcelReader_CompareRemainings(IExcelDataReader reader)
    {
        CompareRemainingsPreviewModel compareRemainingsPreviewModel = new CompareRemainingsPreviewModel();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            object value = reader.GetValue(i);
            switch (i)
            {
                case 0:
                    compareRemainingsPreviewModel.NationalCode = value?.ToString();
                    break;
                case 1:
                    {
                        int.TryParse(value?.ToString(), out var result2);
                        compareRemainingsPreviewModel.LoanCode = result2;
                        break;
                    }
                case 2:
                    {
                        decimal.TryParse(value?.ToString(), out var result);
                        compareRemainingsPreviewModel.Remain = result;
                        break;
                    }
            }
        }
        return compareRemainingsPreviewModel;
    }

    private UserLoan CreateNewUserLoan(InstallmentImportPreviewModel model, Guid loanId)
    {
        return new UserLoan();
    }

    private UserSavedMoney CreateNewUserSavedMoney(SavedMoneyImportPreviewModel model, bool isFirstPeriod = false)
    {
        UserSavedMoney userSavedMoney = new UserSavedMoney();
        userSavedMoney.Amount = (isFirstPeriod ? (model.TotalSavedMoney - model.Amount) : model.Amount);
        if (!isFirstPeriod)
        {
            userSavedMoney.TargetMonth = (model.Month + "/01").ToGregorianDateTime().Value;
        }
        userSavedMoney.Description = (isFirstPeriod ? ("موجودی اول دوره - فایل " + model.Month) : ("فایل - " + model.Month));
        userSavedMoney.FileMonth = model.Month;
        userSavedMoney.SavedBy = "دریافت فایل توسط [" + _appContext.User?.FullName + "]";
        userSavedMoney.SaveDate = DateTime.Now;
        return userSavedMoney;
    }

    private string GenerateUserName(int customerCode, string nationalCode)
    {
        if (_appConfig.IsCloud)
        {
            return $"{customerCode}-{nationalCode}";
        }
        return nationalCode;
    }
}
