namespace Pactly.Core.Domain;

public class CartLine
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class Cart
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<CartLine> Lines { get; set; } = new();
}
