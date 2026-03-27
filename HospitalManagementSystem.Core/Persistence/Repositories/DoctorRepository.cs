using HospitalManagementSystem.Core.Domain.Models;
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
    public class DoctorRepository : GenericRepository<Doctor, HospitalManagementSystemContext>,
            IDoctorRepository
    {
        private readonly ILogger _logger;
        public DoctorRepository(HospitalManagementSystemContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<int> GetCountOfDoctors()
        {
            return await Context.Doctors.CountAsync();
        }

        public async Task<bool> isDoctorEmailExists(string email)
        {
            try
            {
                return await Context.Doctors.AsNoTracking().AnyAsync(u => u.Email == email);
            }
            catch (Exception error)
            {
                _logger.LogError("IsScopeExistsWithNameAsync::Database exception: {0}", error);
                return false;
            }
        }

        public async Task<Doctor> GetDoctorByUUID(string id)
        {
            try
            {
                return await Context.Doctors.AsNoTracking().SingleOrDefaultAsync(u => u.Userid == id);
            }
            catch (Exception error)
            {
                _logger.LogError("IsScopeExistsWithNameAsync::Database exception: {0}", error);
                return null;
            }
        }
    }
}
