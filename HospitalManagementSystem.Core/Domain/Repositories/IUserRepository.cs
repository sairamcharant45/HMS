using HospitalManagementSystem.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public Task<bool> isUserEmailExists(string email);
        public Task<User> GetUserByUUID(string id);
        public Task<User> GetUserByEmailID(string email);
    }
}
