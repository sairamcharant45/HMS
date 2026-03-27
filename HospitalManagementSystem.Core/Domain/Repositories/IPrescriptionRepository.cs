using HospitalManagementSystem.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Repositories
{
    public interface IPrescriptionRepository : IGenericRepository<Prescription>
    {
        public Task<int> GetCountOfPrescriptions();
        public Task<List<Prescription>> GetPrescriptionsByPatientId(int patientId);
        public Task<List<Prescription>> GetAllPrescriptionsOfDoctor(int doctorId);
        public Task<int> DeletePrescriptionsByPatientId(int patientId);
        public Task<int> DeletePrescriptionsByDoctorId(int doctorId);
    }
}
