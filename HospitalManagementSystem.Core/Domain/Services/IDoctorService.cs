using HospitalManagementSystem.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Services
{
    public interface IDoctorService
    {
        public Task<int> GetDoctorsCount();
        public Task<IEnumerable<Doctor>> ListAllDoctorsAsync();
        public Task<Doctor> GetDoctorById(int id);
        public Task<bool> AddDoctor(Doctor doctor);
        public Task<bool> UpdateDoctor(Doctor doctor);
        public Task<bool> DeleteDoctor(int id);
        public Task<Doctor> GetDoctorDetailsByUUID(string uuid);
    }
}
