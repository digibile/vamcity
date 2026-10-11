namespace LoanSystem.Services.Models.IranValidity;

public enum MessageStatus
{
	InformationNotFound = 2,
	RecivedByTCI = 3,
	NotRecivedByTCIYet = 4,
	DeliveredToPhone = 5,
	NotDeliveredToPhoneYet = 6,
	TCIBlackList = 9
}
