using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace HospitalManagementSystem.Core.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User, HospitalManagementSystemContext>,
            IUserRepository
    {
        private readonly ILogger _logger;
        public UserRepository(HospitalManagementSystemContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<bool> isUserEmailExists(string email)
        {
            try
            {
                return await Context.Users.AsNoTracking().AnyAsync(u => u.Email == email);
            }
            catch (Exception error)
            {
                _logger.LogError("IsScopeExistsWithNameAsync::Database exception: {0}", error);
                return false;
            }
        }

        public async Task<User> GetUserByUUID(string id)
        {
            try
            {
                return await Context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Uuid == id);
            }
            catch (Exception error)
            {
                _logger.LogError("IsScopeExistsWithNameAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<User> GetUserByEmailID(string email)
        {
            try
            {
                return await Context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception error)
            {
                _logger.LogError("IsScopeExistsWithNameAsync::Database exception: {0}", error);
                return null;
            }
        }
    }
}
