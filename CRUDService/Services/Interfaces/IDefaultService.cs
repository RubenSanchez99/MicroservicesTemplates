using CRUDService.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRUDService.Services.Interfaces
{
    public interface IDefaultService
    {
        Task<List<Entity>> GetEntities();
        Task<Entity> GetEntity(int id);
        Task<int> AddEntity(Entity entity);
        Task<int> UpdateEntity(Entity entity);
        Task DeleteEntity(int id);
    }
}
