using System.Text;

namespace LoanSystem.Core;

public static class StringExtentions
{
	public static string ToEnglishDigits(this string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return input;
		}
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		foreach (char c in input)
		{
			if (c >= '٠' && c <= '٩')
			{
				stringBuilder.Append((char)(48 + (c - 1632)));
			}
			else if (c >= '۰' && c <= '۹')
			{
				stringBuilder.Append((char)(48 + (c - 1776)));
			}
			else if (c >= '０' && c <= '９')
			{
				stringBuilder.Append((char)(48 + (c - 65296)));
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}
}
