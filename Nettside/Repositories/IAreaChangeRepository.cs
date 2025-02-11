using Nettside.Models;

namespace Nettside.Repositiories
{
    public interface IAreaChangeRepository
    {
        Task<IEnumerable<AreaChangeModel>> GetAllAsync();
        Task<AreaChangeModel?> GetAsync(Guid id);
        Task<AreaChangeModel> AddAsync(AreaChangeModel areaChangeRepository);
        Task<AreaChangeModel?> UpdateAsync(AreaChangeModel areaChangeRepository);
        Task<AreaChangeModel?> DeleteAsync(Guid id);

    }
}
