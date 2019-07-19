using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CRUDService.Infrastructure;
using CRUDService.Model;
using CRUDService.Services.Interfaces;

namespace CRUDService.Services.Implementations
{
    internal class DefaultService : IDefaultService
    {
        private readonly RepositoryContext context;

        public DefaultService(RepositoryContext context)
        {
            this.context = context;
        }

        public async Task<int> AddEntity(Entity entity)
        {
            context.Entities.Add(entity);
            await context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task DeleteEntity(int id)
        {
            var entityToDelete = context.Entities.Single(i => i.Id == id);

            context.Entities.Update(entityToDelete);
            await context.SaveChangesAsync();
        }

        public async Task<Entity> GetEntity(int id)
        {
            return await context.Entities.AsNoTracking().SingleAsync(x => x.Id == id);
        }

        public async Task<List<Entity>> GetEntities()
        {
            return await context.Entities.AsNoTracking().ToListAsync();
        }

        public async Task<int> UpdateEntity(Entity entity)
        {
            var entityToUpdate = await context.Entities.SingleAsync(i => i.Id == entity.Id);

            entityToUpdate = entity;
            context.Entities.Update(entityToUpdate);
            await context.SaveChangesAsync();
            return entityToUpdate.Id;
        }
    }
}
