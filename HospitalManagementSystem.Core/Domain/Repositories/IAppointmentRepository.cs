using HospitalManagementSystem.Core.Domain.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Repositories
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        public Task<Appointment> GetAppointmentById(int Id);
        public Task<int> GetActiveAppointmentsCount();
        public Task<int> GetPendingAppointmentsCount();
        public Task<List<Appointment>> GetAppointmentsByPatientId(int patientId);
        public Task<List<Appointment>> GetAppointmentsByDoctorId(int doctorId);
        public Task<int> DeleteAppointmentsByDoctorId(int doctorId);
        public Task<int> DeleteAppointmentsByPatientId(int patientId);
    }
}
