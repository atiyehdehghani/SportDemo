namespace SportMarket.Application.Common.Models;

public class OperationResult
{
    public bool Succeeded { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static OperationResult Success() => new() { Succeeded = true };

    public static OperationResult Failed(params string[] errors) => new() { Succeeded = false, Errors = errors };
}
