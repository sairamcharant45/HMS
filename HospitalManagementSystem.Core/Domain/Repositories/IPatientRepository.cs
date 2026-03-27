using HospitalManagementSystem.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Repositories
{
    public interface IPatientRepository : IGenericRepository<Patient>
    {
        public Task<int> GetCountOfPatients();
        public Task<bool> isPatientEmailExists(string email);
        public Task<Patient> GetPatientByUUID(string id);
    }
}
