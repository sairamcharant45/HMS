using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalManagementSystem.Core.Domain.Models;

namespace HospitalManagementSystem.Core.Domain.Services
{
    public interface IAppointmentService
    {
        public Task<int> GetCountOfActiveAppointments();
        public Task<int> GetCountOfPendingAppointments();
        public Task<IEnumerable<Appointment>> GetAllAppointmentsOfDoctor(int docId);
        public Task<IEnumerable<Appointment>> GetAllAppointmentsList();

        public Task<Appointment> GetAppointmentById(int Id);

        public Task<bool> UpdateAppointment(Appointment appointment);

        public Task<IEnumerable<Appointment>> GetAppointmentsByPatientId(int docId);

        public Task<IEnumerable<Appointment>> GetAppointmentsByDoctorId(int docId);

        public  Task<bool> CreateAppointment(Appointment appointment);

        public Task<bool> DeleteAppointment(int id);
    }
}
