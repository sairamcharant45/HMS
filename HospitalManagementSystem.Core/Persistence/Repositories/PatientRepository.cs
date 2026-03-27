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
    public class PatientRepository : GenericRepository<Patient, HospitalManagementSystemContext>,
            IPatientRepository
    {
        private readonly ILogger _logger;
        public PatientRepository(HospitalManagementSystemContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }
        public async Task<int> GetCountOfPatients()
        {
            return await Context.Patients.CountAsync();
        }
        public async Task<bool> isPatientEmailExists(string email)
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

        public async Task<Patient> GetPatientByUUID(string id)
        {
            try
            {
                return await Context.Patients.AsNoTracking().SingleOrDefaultAsync(u => u.Userid == id);
            }
            catch (Exception error)
            {
                _logger.LogError("IsScopeExistsWithNameAsync::Database exception: {0}", error);
                return null;
            }
        }
    }
}
