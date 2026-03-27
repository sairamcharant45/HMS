using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        
        private ILogger<UnitOfWork> _logger;
        private readonly HospitalManagementSystemContext _hmscontext;

        private IDoctorRepository _doctor;
        private IPatientRepository _patient;
        private IPrescriptionRepository _prescription;
        private IRoleRepository _role;
        private IUserRepository _user;
        private IAppointmentRepository _appointment;

        public UnitOfWork(HospitalManagementSystemContext hmsContext, ILogger<UnitOfWork> Logger)
        {
            _hmscontext = hmsContext;
            _logger = Logger;
        }

        public IDoctorRepository Doctor
        {
            get { return _doctor = _doctor ?? new DoctorRepository(_hmscontext, _logger); }
        }

        public IPatientRepository Patient
        {
            get { return _patient = _patient ?? new PatientRepository(_hmscontext, _logger); }
        }

        public IPrescriptionRepository Prescription
        {
            get { return _prescription = _prescription ?? new PrescriptionRepository(_hmscontext, _logger); }
        }

        public IRoleRepository Role
        {
            get { return _role = _role ?? new RoleRepository(_hmscontext, _logger); }
        }

        public IUserRepository User
        {
            get { return _user = _user ?? new UserRepository(_hmscontext, _logger); }
        }

        public IAppointmentRepository Appointment
        {
            get { return _appointment = _appointment ?? new AppointmentRepository(_hmscontext, _logger); }
        }


        public async Task<int> SaveAsync()
        {
            return await _hmscontext.SaveChangesAsync();
        }

        public void DisableDetectChanges()
        {
            _hmscontext.ChangeTracker.AutoDetectChangesEnabled = false;
            return;
        }

        public void EnableDetectChanges()
        {
            _hmscontext.ChangeTracker.AutoDetectChangesEnabled = true;
            return;
        }
        public int Save()
        {
            return _hmscontext.SaveChanges();
        }

    }
}