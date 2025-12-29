using System.Text.RegularExpressions;

namespace UserService.Domain.ValueObjects;

public sealed class Email
{
    public string Value { get; }

    private Email() { } // EF Core

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email is required");

        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Invalid email format");

        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
