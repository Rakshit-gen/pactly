using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pactly.Core.Domain;

public enum AgreementStatus
{
    Draft,
    AwaitingSignature,
    Signed,
    Executed
}

public class AuditEntry
{
    public DateTimeOffset Timestamp { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;
}

public class Agreement
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string OrderId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public AgreementStatus Status { get; set; } = AgreementStatus.Draft;
    public string ContentSnapshot { get; set; } = string.Empty;
    public string? SignatureDataUrl { get; set; }
    public DateTimeOffset? SignedAt { get; set; }
    public DateTimeOffset? ExecutedAt { get; set; }
    public List<AuditEntry> AuditTrail { get; set; } = new();
}
