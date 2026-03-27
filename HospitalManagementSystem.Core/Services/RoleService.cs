using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using HospitalManagementSystem.Core.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Role>> ListAllRolesAsync()
        {
            return await _unitOfWork.Role.GetAllAsync();
        }

        public async Task<string> GetRoleById(int id)
        {
            var role = await _unitOfWork.Role.GetByIdAsync(id);
            if(role != null)
            {
                return role.Name;
            }
            return null;
        }
    }
}
