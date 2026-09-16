using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pactly.Core.Domain;

public class CartLine
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class Cart
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public List<CartLine> Lines { get; set; } = new();
}
