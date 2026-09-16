namespace Pactly.Core.Domain;

public enum ProductType
{
    Plan,
    AddOn
}

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public int MonthlyPriceCents { get; set; }
    public int AnnualPriceCents { get; set; }
    public List<string> Features { get; set; } = new();
    public int? SeatLimit { get; set; }
    public bool IsActive { get; set; } = true;
}
