using MongoDB.Driver;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Infrastructure.Mongo;

public class MongoUserRepository : IUserRepository
{
    private readonly MongoContext _context;

    public MongoUserRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<bool> CreateAsync(User user)
    {
        try
        {
            await _context.Users.InsertOneAsync(user);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    public async Task UpdateAsync(User user)
    {
        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
    }
}
