namespace Pactly.Core.Domain;

public enum OrderStatus
{
    Pending,
    Completed,
    Cancelled
}

public class OrderLine
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int UnitPriceCents { get; set; }
    public int Quantity { get; set; }
}

public class Order
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<OrderLine> Lines { get; set; } = new();
    public int SubtotalCents { get; set; }
    public int ProrationCreditCents { get; set; }
    public int TotalCents { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; }
}
