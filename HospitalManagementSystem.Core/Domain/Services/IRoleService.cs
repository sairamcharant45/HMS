using HospitalManagementSystem.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Services
{
    public interface IRoleService
    {
        public Task<IEnumerable<Role>> ListAllRolesAsync();
        public Task<string> GetRoleById(int id);
    }
}
