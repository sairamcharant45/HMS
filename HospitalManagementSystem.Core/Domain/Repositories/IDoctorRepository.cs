using HospitalManagementSystem.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Repositories
{
    public interface IDoctorRepository : IGenericRepository<Doctor>
    {
        public Task<int> GetCountOfDoctors();
        public Task<bool> isDoctorEmailExists(string email);
        public Task<Doctor> GetDoctorByUUID(string id);
    }
}
