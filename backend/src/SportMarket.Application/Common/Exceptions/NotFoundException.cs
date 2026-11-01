namespace SportMarket.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"{name} یافت نشد.")
    {
    }
}
