using System;
using System.Collections.Generic;

namespace LoanSystem.Core.Exceptions;

public class CustomException : Exception
{
    private ICollection<string> _relatedErrors;

    public object[] Arguments { get; set; }

    public virtual ICollection<string> RelatedErrors
    {
        get
        {
            return _relatedErrors ?? (_relatedErrors = new List<string>());
        }
        set
        {
            _relatedErrors = value;
        }
    }

    public CustomException()
    {
    }

    public CustomException(string message)
        : base(message)
    {
    }

    public CustomException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public CustomException(string message, Exception innerException, List<string> relatedErrors)
        : base(message, innerException)
    {
        RelatedErrors = relatedErrors;
    }

    public CustomException(string message, List<string> relatedErrors)
        : base(message)
    {
        RelatedErrors = relatedErrors;
    }
}
