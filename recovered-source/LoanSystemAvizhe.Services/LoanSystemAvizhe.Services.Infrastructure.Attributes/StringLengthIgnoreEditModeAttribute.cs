using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LoanSystemAvizhe.Services.Infrastructure.Attributes;

public class StringLengthIgnoreEditModeAttribute : ValidationAttribute
{
	private StringLengthAttribute _innerAttribute;

	private string _errorMessage { get; set; }

	public StringLengthIgnoreEditModeAttribute(int maximumLength, int minimumLength, string errorMessage)
	{
		_innerAttribute = new StringLengthAttribute(maximumLength);
		_innerAttribute.MinimumLength = minimumLength;
		_errorMessage = errorMessage;
	}

	protected override ValidationResult IsValid(object value, ValidationContext validationContext)
	{
		PropertyInfo property = validationContext.ObjectType.GetProperty("Id");
		if (property != null)
		{
			if (!property.GetValue(validationContext.ObjectInstance, null).Equals(0) && string.IsNullOrEmpty((string)value))
			{
				return ValidationResult.Success;
			}
			if (!_innerAttribute.IsValid(value))
			{
				_ = validationContext.DisplayName;
				_innerAttribute.ErrorMessage = _errorMessage;
				try
				{
					_innerAttribute.Validate(value, validationContext.MemberName);
				}
				catch (Exception ex)
				{
					string errorMessage = (ErrorMessage = ex.Message);
					return new ValidationResult(errorMessage);
				}
			}
			return ValidationResult.Success;
		}
		return new ValidationResult(FormatErrorMessage("Id"));
	}
}
