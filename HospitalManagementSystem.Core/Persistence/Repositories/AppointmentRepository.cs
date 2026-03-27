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
    public class AppointmentRepository : GenericRepository<Appointment, HospitalManagementSystemContext>,
            IAppointmentRepository
    {
        private readonly ILogger _logger;

        public AppointmentRepository(HospitalManagementSystemContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<Appointment> GetAppointmentById(int Id)
        {
            return await Context.Appointments.FirstOrDefaultAsync(p => p.Id == Id);
        }

        public async Task<int> GetActiveAppointmentsCount()
        {
            return await Context.Appointments.Where(p => p.Status == "APPROVED").CountAsync();
        }

        public async Task<int> GetPendingAppointmentsCount()
        {
            return await Context.Appointments.Where(p => p.Status == "PENDING").CountAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientId(int patientId)
        {
            return await Context.Appointments.Where(p => p.Patientid == patientId).ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorId(int doctorId)
        {
            return await Context.Appointments.Where(p => p.Doctorid == doctorId).ToListAsync();
        }

        public async Task<int> DeleteAppointmentsByDoctorId(int doctorId)
        {
            bool exists = await Context.Appointments.AnyAsync(p => p.Doctorid == doctorId);
            if (!exists)
            {
                return -1;
            }
            return await Context.Appointments.Where(p => p.Doctorid == doctorId).ExecuteDeleteAsync();
        }
        public async Task<int> DeleteAppointmentsByPatientId(int patientId)
        {
            bool exists = await Context.Appointments.AnyAsync(p => p.Patientid == patientId);
            if (!exists)
            {
                return -1;
            }
            return await Context.Appointments.Where(p => p.Patientid == patientId).ExecuteDeleteAsync();
        }
    }
}
