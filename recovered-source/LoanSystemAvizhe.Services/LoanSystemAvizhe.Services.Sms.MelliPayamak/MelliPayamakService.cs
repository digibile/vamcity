using System;
using System.Linq;
using LoanSystem.Core.Exceptions;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Sms.Models.IPPanel;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.WebOne;
using mpNuget;

namespace LoanSystemAvizhe.Services.Sms.MelliPayamak;

public class MelliPayamakService : IPayamakService
{
	private MelliPayamakConfig _melliPayamakConfig;

	public MelliPayamakService(MelliPayamakConfig MelliPayamakConfig)
	{
		_melliPayamakConfig = MelliPayamakConfig;
	}

	public ResponsePayamak Send(BaseSendPayamak sendMelliPayamak)
	{
		RestResponse restResponse = new RestClient(_melliPayamakConfig.Username, _melliPayamakConfig.Password).Send(sendMelliPayamak.To, _melliPayamakConfig.From, sendMelliPayamak.Text, _melliPayamakConfig.IsFlash);
		long result;
		if (restResponse.StrRetStatus == "Ok")
		{
			return new ResponsePayamak
			{
				Success = (restResponse.StrRetStatus == "Ok" && long.TryParse(restResponse.Value, out result) && result > 100),
				Key = restResponse.Value,
				Message = restResponse.StrRetStatus,
				Response = restResponse
			};
		}
		throw new CustomException(restResponse.StrRetStatus);
	}

	public ResponsePayamak SendByBaseNumber(BaseSendPayamak sendMelliPayamak, int bodyId)
	{
		RestResponse restResponse = new RestClient(_melliPayamakConfig.Username, _melliPayamakConfig.Password).SendByBaseNumber(sendMelliPayamak.Text, sendMelliPayamak.To, bodyId);
		if (restResponse.StrRetStatus == "Ok")
		{
			return new ResponsePayamak
			{
				Success = (restResponse.StrRetStatus == "Ok"),
				Key = restResponse.Value,
				Message = restResponse.StrRetStatus,
				Response = restResponse
			};
		}
		throw new CustomException(restResponse.StrRetStatus);
	}

	public ResponsePayamak CheckDelivery(string recId)
	{
		int[] source = new int[2] { 1, 200 };
		int[] source2 = new int[8] { -1, -3, 2, 3, 5, 16, 100, 500 };
		RestResponse delivery = new RestClient(_melliPayamakConfig.Username, _melliPayamakConfig.Password).GetDelivery(long.Parse(recId));
		return new ResponsePayamak
		{
			Success = true,
			Key = (Enumerable.Contains(source, delivery.RetStatus) ? AppSmsStatus.Delivered.ToString() : (Enumerable.Contains(source2, delivery.RetStatus) ? AppSmsStatus.Failed.ToString() : AppSmsStatus.Pending.ToString())),
			Message = delivery.StrRetStatus,
			Response = delivery
		};
	}

	public ResponsePayamak GetBasePrice(BaseSendPayamak sendMelliPayamak)
	{
		RestResponse basePrice = new RestClient(_melliPayamakConfig.Username, _melliPayamakConfig.Password).GetBasePrice();
		return new ResponsePayamak
		{
			Success = (basePrice.StrRetStatus == "Ok"),
			Key = basePrice.Value,
			Message = basePrice.StrRetStatus,
			Response = basePrice
		};
	}

	public ResponsePayamak GetCredit()
	{
		RestResponse credit = new RestClient(_melliPayamakConfig.Username, _melliPayamakConfig.Password).GetCredit();
		return new ResponsePayamak
		{
			Success = (credit.StrRetStatus == "Ok"),
			Key = credit.Value,
			Message = credit.StrRetStatus,
			Response = credit
		};
	}

	public ResponsePayamak SendByLookupKaveNegar(SendKaveNegar sendPayamak, string templateName)
	{
		throw new NotImplementedException();
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
