using System;
using System.Linq;
using Kavenegar;
using Kavenegar.Models;
using Kavenegar.Models.Enums;
using LoanSystem.Core.Exceptions;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Sms.Models.IPPanel;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.WebOne;

namespace LoanSystemAvizhe.Services.Sms.KaveNegar;

public class KaveNegarService : IPayamakService
{
	private KaveNegarConfig _kaveNegarConfig;

	public KaveNegarService(KaveNegarConfig KaveNegarConfig)
	{
		_kaveNegarConfig = KaveNegarConfig;
	}

	public ResponsePayamak Send(BaseSendPayamak sendPayamak)
	{
		KavenegarApi kavenegarApi = new KavenegarApi(_kaveNegarConfig.ApiKey);
		try
		{
			SendResult sendResult = kavenegarApi.Send(_kaveNegarConfig.Sender, sendPayamak.To, sendPayamak.Text);
			return new ResponsePayamak
			{
				Success = true,
				Key = sendResult.Messageid.ToString(),
				Message = sendResult.StatusText,
				Response = sendResult
			};
		}
		catch (Exception ex)
		{
			throw new CustomException(ex.Message);
		}
	}

	public ResponsePayamak SendByBaseNumber(BaseSendPayamak sendMelliPayamak, int bodyId)
	{
		throw new NotImplementedException();
	}

	public ResponsePayamak CheckDelivery(string messageId)
	{
		MessageStatus[] source = new MessageStatus[1] { MessageStatus.Delivered };
		MessageStatus[] source2 = new MessageStatus[4]
		{
			MessageStatus.Undelivered,
			MessageStatus.Incorrect,
			MessageStatus.Canceled,
			MessageStatus.Filtered
		};
		StatusResult statusResult = new KavenegarApi(_kaveNegarConfig.ApiKey).Status(messageId);
		return new ResponsePayamak
		{
			Success = true,
			Key = (Enumerable.Contains(source, statusResult.Status) ? AppSmsStatus.Delivered.ToString() : (Enumerable.Contains(source2, statusResult.Status) ? AppSmsStatus.Failed.ToString() : AppSmsStatus.Pending.ToString())),
			Message = statusResult.Statustext,
			Response = statusResult
		};
	}

	public ResponsePayamak GetBasePrice(BaseSendPayamak sendMelliPayamak)
	{
		throw new NotImplementedException();
	}

	public ResponsePayamak GetCredit()
	{
		KavenegarApi kavenegarApi = new KavenegarApi(_kaveNegarConfig.ApiKey);
		try
		{
			AccountInfoResult accountInfoResult = kavenegarApi.AccountInfo();
			return new ResponsePayamak
			{
				Success = true,
				Key = (accountInfoResult.RemainCredit / 10).ToString(),
				Message = "",
				Response = accountInfoResult
			};
		}
		catch (Exception)
		{
			return new ResponsePayamak
			{
				Success = false,
				Key = "",
				Message = "",
				Response = null
			};
		}
	}

	public ResponsePayamak SendByLookupKaveNegar(SendKaveNegar sendPayamak, string templateName)
	{
		KavenegarApi kavenegarApi = new KavenegarApi(_kaveNegarConfig.ApiKey);
		try
		{
			SendResult sendResult = kavenegarApi.VerifyLookup(sendPayamak.To, sendPayamak.Token1, sendPayamak.Token2, sendPayamak.Token3, sendPayamak.Token10, sendPayamak.Token20, templateName, VerifyLookupType.Sms);
			return new ResponsePayamak
			{
				Success = true,
				Key = sendResult.Messageid.ToString(),
				Message = sendResult.StatusText,
				Response = sendResult
			};
		}
		catch (Exception)
		{
			throw;
		}
	}

	public ResponsePayamak SendByPatternIPPanel(SendIPPanelPattern sendPayamak, string patternName)
	{
		throw new NotImplementedException();
	}

	public ResponsePayamak SendByPatternWebOne(SendWebOnePattern sendPayamak, string patternName)
	{
		throw new NotImplementedException();
	}
}
