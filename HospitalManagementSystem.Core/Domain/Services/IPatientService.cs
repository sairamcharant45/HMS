using HospitalManagementSystem.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Services
{
    public interface IPatientService
    {
        public Task<int> GetPatientsCount();
        public Task<IEnumerable<Patient>> ListAllPatientsAsync();
        public Task<Patient> GetPatientById(int id);
        public Task<bool> AddPatient(Patient doctor);
        public Task<bool> UpdatePatient(Patient patient);
        public Task<bool> DeletePatient(int id);
        public Task<Patient> GetPatientByUUID(string patientUUID);
    }
}
