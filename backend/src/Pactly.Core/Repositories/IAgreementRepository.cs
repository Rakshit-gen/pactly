using Pactly.Core.Domain;

namespace Pactly.Core.Repositories;

public interface IAgreementRepository
{
    Task<Agreement?> GetByIdAsync(string id);
    Task<List<Agreement>> GetByUserIdAsync(string userId);
    Task CreateAsync(Agreement agreement);
    Task UpdateAsync(Agreement agreement);
}
