using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Repositories
{
    public interface IUnitOfWork
    {
        IDoctorRepository Doctor { get; }

        IPatientRepository Patient { get; }

        IPrescriptionRepository Prescription { get; }

        IRoleRepository Role { get; }

        IUserRepository User { get; }

        IAppointmentRepository Appointment { get; }

        Task<int> SaveAsync();

        void DisableDetectChanges();

        void EnableDetectChanges();

        int Save();
    }
}
