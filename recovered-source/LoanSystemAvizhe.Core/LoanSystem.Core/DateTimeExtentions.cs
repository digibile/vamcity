using System;
using System.Globalization;

namespace LoanSystem.Core;

public static class DateTimeExtentions
{
    public static string ToPersianTime(this TimeSpan ts)
    {
        return ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0');
    }

    public static string ToPersianTime(this TimeSpan? ts)
    {
        if (!ts.HasValue)
        {
            return "";
        }
        return ts.Value.ToPersianTime();
    }

    public static string ToPersianDate(this DateTime dateTime)
    {
        PersianCalendar persianCalendar = new PersianCalendar();
        try
        {
            return $"{persianCalendar.GetYear(dateTime).ToString().PadLeft(4, '0')}/{persianCalendar.GetMonth(dateTime).ToString().PadLeft(2, '0')}/{persianCalendar.GetDayOfMonth(dateTime).ToString().PadLeft(2, '0')}";
        }
        catch
        {
            return "";
        }
    }

    public static PersianDate ToPersianDateObject(this DateTime dateTime)
    {
        PersianCalendar persianCalendar = new PersianCalendar();
        PersianDate persianDate = new PersianDate();
        try
        {
            persianDate.Year = persianCalendar.GetYear(dateTime);
            persianDate.Month = persianCalendar.GetMonth(dateTime);
            persianDate.Day = persianCalendar.GetDayOfMonth(dateTime);
            persianDate.DayOfWeek = persianCalendar.GetDayOfWeek(dateTime);
            persianDate.Hour = dateTime.Hour;
            persianDate.Minute = dateTime.Minute;
            persianDate.Second = dateTime.Second;
            persianDate.MonthName = GetMonthName(persianCalendar.GetMonth(dateTime));
            return persianDate;
        }
        catch
        {
            return persianDate;
        }
    }

    private static string GetMonthName(int m)
    {
        return m switch
        {
            1 => "فروردین",
            2 => "اردیبهشت",
            3 => "خرداد",
            4 => "تیر",
            5 => "مرداد",
            6 => "شهریور",
            7 => "مهر",
            8 => "آبان",
            9 => "آذر",
            10 => "دی",
            11 => "بهمن",
            12 => "اسفند",
            _ => "نامعلوم",
        };
    }

    public static string ToPersianDateTime(this DateTime dateTime)
    {
        try
        {
            return string.Format("{2} {0}:{1}", dateTime.Hour.ToString().PadLeft(2, '0'), dateTime.Minute.ToString().PadLeft(2, '0'), dateTime.ToPersianDate());
        }
        catch
        {
            return "";
        }
    }

    public static string ToPersianDate(this DateTime? dateTime)
    {
        if (dateTime.HasValue)
        {
            return dateTime.Value.ToPersianDate();
        }
        return string.Empty;
    }

    public static string ToPersianDateTime(this DateTime? dateTime)
    {
        if (dateTime.HasValue)
        {
            return dateTime.Value.ToPersianDateTime();
        }
        return string.Empty;
    }

    public static DateTime? ToGregorianDateTime(this string persianDate)
    {
        if (string.IsNullOrEmpty(persianDate))
        {
            return null;
        }
        try
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            string[] array = persianDate.Split(' ');
            string[] array2 = array[0].Split('/');
            string[] array3 = new string[3] { "0", "0", "0" };
            if (array.Length == 2)
            {
                array3 = array[1].Split(':');
            }
            int year = int.Parse(array2[0]);
            short month = short.Parse(array2[1]);
            short day = short.Parse(array2[2]);
            short hour = short.Parse(array3[0]);
            short minute = short.Parse(array3[1]);
            int second = ((array3.Length == 3) ? short.Parse(array3[2]) : 0);
            return persianCalendar.ToDateTime(year, month, day, hour, minute, second, 0);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static (string, string) GetStartEndOfMonthInPersian(DateTime dateTime)
    {
        string[] array = dateTime.ToPersianDate().Split("/");
        string text = array[0];
        string text2 = array[1];
        string item = text + "/" + text2 + "/01";
        int.TryParse(text2, out var result);
        int.TryParse(text, out var result2);
        result = ((result + 1 > 12) ? 1 : result);
        object arg = result2;
        string arg2 = result.ToString().PadLeft(2, '0');
        string arg3;
        if (result < 6)
        {
            arg3 = "31";
        }
        else
        {
            arg3 = ((result != 12) ? "30" : "29");
        }
        string item2 = $"{arg}/{arg2}/{arg3}";
        return (item, item2);
    }

    public static DateTime GetEndOfNextPersianMonth(DateTime now)
    {
        PersianCalendar persianCalendar = new PersianCalendar();
        int num = persianCalendar.GetYear(now);
        int month = persianCalendar.GetMonth(now);
        month++;
        if (month > 12)
        {
            month = 1;
            num++;
        }
        int daysInMonth = persianCalendar.GetDaysInMonth(num, month);
        return persianCalendar.ToDateTime(num, month, daysInMonth, 0, 0, 0, 0);
    }
}
