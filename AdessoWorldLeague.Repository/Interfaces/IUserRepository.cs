using AdessoWorldLeague.Data.Documents;

namespace AdessoWorldLeague.Repository.Interfaces;

public interface IUserRepository : IGenericRepository<UserDocument>
{
    Task<UserDocument?> GetByEmailAsync(string email);
}
