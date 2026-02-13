using AdessoWorldLeague.Data.Documents;
using AdessoWorldLeague.Mongo.Context;
using AdessoWorldLeague.Repository.Interfaces;
using MongoDB.Driver;

namespace AdessoWorldLeague.Repository.Implementations;

public class UserRepository : GenericRepository<UserDocument>, IUserRepository
{
    public UserRepository(IMongoContext context) : base(context, "users") { }

    public async Task<UserDocument?> GetByEmailAsync(string email)
    {
        return await Collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }
}
