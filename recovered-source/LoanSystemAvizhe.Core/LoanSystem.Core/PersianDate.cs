using System;

namespace LoanSystem.Core;

public class PersianDate
{
    public int Day { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public int Hour { get; set; }

    public int Minute { get; set; }

    public int Second { get; set; }

    public string MonthName { get; set; }

    public string Date => Year.ToString().PadLeft(4, '0') + "/" + Month.ToString().PadLeft(2, '0') + "/" + Day.ToString().PadLeft(2, '0');

    public string Time => Hour.ToString().PadLeft(2, '0') + ":" + Minute.ToString().PadLeft(2, '0') + ":" + Second.ToString().PadLeft(2, '0');
}
