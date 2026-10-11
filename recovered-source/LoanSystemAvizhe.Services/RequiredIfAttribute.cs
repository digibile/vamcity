using System.ComponentModel.DataAnnotations;

public class RequiredIfAttribute : RequiredAttribute
{
	private string PropertyName { get; set; }

	private object DesiredValue { get; set; }

	private string _errorMessage { get; set; }

	public RequiredIfAttribute(string propertyName, object desiredvalue, string errorMessage)
	{
		PropertyName = propertyName;
		DesiredValue = desiredvalue;
		_errorMessage = errorMessage;
	}

	protected override ValidationResult IsValid(object value, ValidationContext context)
	{
		object objectInstance = context.ObjectInstance;
		if (objectInstance.GetType().GetProperty(PropertyName).GetValue(objectInstance, null)
			.ToString() == DesiredValue.ToString())
		{
			ValidationResult validationResult = base.IsValid(value, context);
			if (validationResult != null)
			{
				validationResult.ErrorMessage = _errorMessage;
			}
			return validationResult;
		}
		return ValidationResult.Success;
	}
}
