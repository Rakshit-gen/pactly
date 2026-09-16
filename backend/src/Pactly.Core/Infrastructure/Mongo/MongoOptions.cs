namespace Pactly.Core.Infrastructure.Mongo;

public class MongoOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "pactly";
}
