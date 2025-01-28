using Microsoft.EntityFrameworkCore;
using Nettside.Data;
using Nettside.Models;

namespace Nettside.Repositiories
{
    public class AreaChangeRepository : IAreaChangeRepository
    {
        private readonly AppDbContext appDbContext;
        public AreaChangeRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        public async Task<AreaChangeModel> AddAsync(AreaChangeModel areaChange)
        {
            await appDbContext.AreaChanges.AddAsync(areaChange);
            await appDbContext.SaveChangesAsync();
            return areaChange;
        }
        public async Task<AreaChangeModel?> DeleteAsync(int id)
        {
            var areaChangeRepository = await appDbContext.AreaChanges.FindAsync(id);
            if (areaChangeRepository == null)
            {
                return null;
            }
            appDbContext.AreaChanges.Remove(areaChangeRepository);
            await appDbContext.SaveChangesAsync();
            return areaChangeRepository;
        }
        public async Task<IEnumerable<AreaChangeModel>> GetAllAsync()
        {
            return await appDbContext.AreaChanges.ToListAsync();
        }
        public async Task<AreaChangeModel?> GetAsync(int id)
        {
            return await appDbContext.AreaChanges.FindAsync(id);
        }
        public async Task<AreaChangeModel?> UpdateAsync(AreaChangeModel areaChange)
        {
            var existingAreaChange = await appDbContext.AreaChanges.FindAsync(areaChange.Id);
            if (existingAreaChange == null)
            {
                return null;
            }
            appDbContext.Entry(existingAreaChange).CurrentValues.SetValues(areaChange);
            await appDbContext.SaveChangesAsync();
            return existingAreaChange;
        }
    }
}