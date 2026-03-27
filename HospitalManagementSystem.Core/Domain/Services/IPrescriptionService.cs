using HospitalManagementSystem.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Services
{
    public interface IPrescriptionService
    {
        public Task<int> GetPrescriptionsCount();
        public Task<IEnumerable<Prescription>> ListAllPrescriptionsAsync();
        public Task<IEnumerable<Prescription>> ListAllPrescriptionsOfDoctorAsync(int docId);
        public Task<IEnumerable<Prescription>> ListAllPrescriptionsOfPatientAsync(int patientId);
        public Task<bool> CreatePrescription(Prescription prescription);
        public Task<Prescription> GetPrescriptionById(int id);
        public Task<bool> UpdatePrescription(Prescription prescription);
        public Task<bool> DeletePrescriotion(int id);
    }
}
