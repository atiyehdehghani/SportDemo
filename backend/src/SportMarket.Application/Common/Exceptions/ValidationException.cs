namespace SportMarket.Application.Common.Exceptions;

/// <summary>خطای اعتبارسنجی با جزئیات فیلدها</summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("اطلاعات ارسالی معتبر نیست.")
    {
        Errors = errors;
    }
}
