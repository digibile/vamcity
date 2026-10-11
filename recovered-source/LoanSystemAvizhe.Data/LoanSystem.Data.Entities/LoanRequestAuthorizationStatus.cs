namespace LoanSystem.Data.Entities;

public enum LoanRequestAuthorizationStatus
{
	در_انتظار_ثبت_توسط_کاربر = 0,
	ثبت_شده_توسط_کاربر = 5,
	در_انتطار_بررسی_کارشناس = 10,
	ناقص = 15,
	تایید_شده_توسط_مدیر = 20
}
