namespace OrderStream.Domain.ValueObjects;

public record OrderName
{
    public string Value { get; }
    private OrderName(string value) => Value = value;
    public static OrderName Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentOutOfRangeException.ThrowIfNotEqual(value.Length, 5);

        return new OrderName(value);
    }
}
