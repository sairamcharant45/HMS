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
    public class PrescriptionRepository : GenericRepository<Prescription, HospitalManagementSystemContext>,
            IPrescriptionRepository
    {
        private readonly ILogger _logger;
        public PrescriptionRepository(HospitalManagementSystemContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }
        public async Task<int> GetCountOfPrescriptions()
        {
            return await Context.Prescriptions.CountAsync();
        }
        public async Task<List<Prescription>> GetPrescriptionsByPatientId(int patientId)
        {
            return await Context.Prescriptions.Where(p => p.Patientid == patientId).ToListAsync();
        }

        public async Task<List<Prescription>> GetAllPrescriptionsOfDoctor(int docId)
        {
            return await Context.Prescriptions.Where(p => p.Doctorid == docId).ToListAsync();
        }

        public async Task<int> DeletePrescriptionsByPatientId(int patientId)
        {
            bool exists = await Context.Prescriptions.AnyAsync(p => p.Patientid == patientId);
            if (!exists)
            {
                return -1;
            }
            return await Context.Prescriptions.Where(p => p.Patientid == patientId).ExecuteDeleteAsync();
        }

        public async Task<int> DeletePrescriptionsByDoctorId(int doctorId)
        {
            bool exists = await Context.Prescriptions.AnyAsync(p => p.Doctorid == doctorId);
            if (!exists)
            {
                return -1;
            }
            return await Context.Prescriptions.Where(p => p.Doctorid == doctorId).ExecuteDeleteAsync();
        }
    }
}
