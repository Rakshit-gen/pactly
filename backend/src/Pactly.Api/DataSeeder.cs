using MongoDB.Bson;
using MongoDB.Driver;
using Pactly.Core.Domain;
using Pactly.Core.Infrastructure.Mongo;

namespace Pactly.Api;

public static class DataSeeder
{
    public static async Task EnsureSeededAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MongoContext>();

        var existingCount = await context.Products.CountDocumentsAsync(FilterDefinition<Product>.Empty);
        if (existingCount > 0)
        {
            return;
        }

        var products = new List<Product>
        {
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Slug = "starter",
                Name = "Starter",
                Description = "For small teams sending their first few agreements.",
                Type = ProductType.Plan,
                MonthlyPriceCents = 2900,
                AnnualPriceCents = 29000,
                SeatLimit = 5,
                Features = new List<string>
                {
                    "Up to 5 seats",
                    "20 agreements per month",
                    "Standard audit trail",
                    "Email support"
                }
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Slug = "growth",
                Name = "Growth",
                Description = "For teams that live in contracts every day.",
                Type = ProductType.Plan,
                MonthlyPriceCents = 5900,
                AnnualPriceCents = 59000,
                SeatLimit = 20,
                Features = new List<string>
                {
                    "Up to 20 seats",
                    "Unlimited agreements",
                    "Detailed audit trail with export",
                    "Priority support"
                }
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Slug = "scale",
                Name = "Scale",
                Description = "For organizations running agreements as infrastructure.",
                Type = ProductType.Plan,
                MonthlyPriceCents = 14900,
                AnnualPriceCents = 149000,
                SeatLimit = null,
                Features = new List<string>
                {
                    "Unlimited seats",
                    "Unlimited agreements",
                    "Custom retention policies",
                    "Dedicated support"
                }
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Slug = "extra-storage",
                Name = "Extra document storage",
                Description = "An additional 50 GB of signed document storage.",
                Type = ProductType.AddOn,
                MonthlyPriceCents = 900,
                AnnualPriceCents = 9000,
                Features = new List<string> { "50 GB additional storage" }
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Slug = "priority-support",
                Name = "Priority support",
                Description = "Skip the queue with a two hour response window.",
                Type = ProductType.AddOn,
                MonthlyPriceCents = 1500,
                AnnualPriceCents = 15000,
                Features = new List<string> { "Two hour response window", "Direct Slack channel" }
            }
        };

        await context.Products.InsertManyAsync(products);
    }
}
