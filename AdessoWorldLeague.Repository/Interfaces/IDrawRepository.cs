using AdessoWorldLeague.Data.Documents;

namespace AdessoWorldLeague.Repository.Interfaces;

public interface IDrawRepository : IGenericRepository<DrawDocument>
{
    Task<List<DrawDocument>> GetAllOrderedByDateAsync();
}
