using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Persistence.Repositories
{
    public class RoleRepository : GenericRepository<Role, HospitalManagementSystemContext>,
            IRoleRepository
    {
        public RoleRepository(HospitalManagementSystemContext context, ILogger logger) : base(context, logger)
        {

        }
    }
}
