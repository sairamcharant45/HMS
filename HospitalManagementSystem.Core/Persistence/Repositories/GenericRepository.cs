using HospitalManagementSystem.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Persistence.Repositories
{
    public class GenericRepository<TEntity, TContext> : IGenericRepository<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        protected readonly TContext Context;
        protected readonly ILogger Logger;
        public GenericRepository(TContext context,
            ILogger logger)
        {
            this.Context = context;
            this.Logger = logger;
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            try
            {
                return await Context.Set<TEntity>().FindAsync(id);
            }
            catch (Exception error)
            {
                Logger.LogError("GetByIdAsync:: Database Exception: {0}", error);
                return null;
            }
        }

        public TEntity GetById(int id)
        {
            try
            {
                return Context.Set<TEntity>().Find(id);
            }
            catch (Exception error)
            {
                Logger.LogError("GetById:: Database Exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            try
            {
                return await Context.Set<TEntity>().AsNoTracking().ToListAsync();
            }
            catch (Exception error)
            {
                Logger.LogError("GetAllAsync:: Database Exception: {0}", error);
                return null;
            }
        }

        public async Task AddAsync(TEntity model)
        {
            try
            {
                await Context.Set<TEntity>().AddAsync(model);
            }
            catch (Exception error)
            {
                Logger.LogError("AddAsync:: Database Exception: {0}", error);
            }
        }

        public void Add(TEntity model)
        {
            try
            {
                Context.Set<TEntity>().Add(model);
            }
            catch (Exception error)
            {
                Logger.LogError("Add:: Database Exception: {0}", error);
            }
        }

        public void Update(TEntity model)
        {
            try
            {
                Context.Set<TEntity>().Update(model);
                //Context.Attach(model).State = EntityState.Modified;
            }
            catch (Exception error)
            {
                Logger.LogError("Update:: Database Exception: {0}", error);
            }
        }

        public void Reload(TEntity model)
        {
            try
            {
                Context.Entry(model).Reload();
            }
            catch (Exception error)
            {
                Logger.LogError("Update:: Database Exception: {0}", error);
            }
        }
        public void Remove(TEntity model)
        {
            try
            {
                Context.Set<TEntity>().Remove(model);
            }
            catch (Exception error)
            {
                Logger.LogError("Remove:: Database Exception: {0}", error);
            }
        }
    }
}
