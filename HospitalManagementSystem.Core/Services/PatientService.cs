using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using HospitalManagementSystem.Core.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PatientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<int> GetPatientsCount()
        {
            return await _unitOfWork.Patient.GetCountOfPatients();
        }
        public async Task<IEnumerable<Patient>> ListAllPatientsAsync()
        {
            return await _unitOfWork.Patient.GetAllAsync();
        }

        public async Task<Patient> GetPatientById(int id)
        {
            return await _unitOfWork.Patient.GetByIdAsync(id);
        }

        public async Task<Patient> GetPatientByUUID(string patientUUID)
        {
            return await _unitOfWork.Patient.GetPatientByUUID(patientUUID);
        }

        public async Task<bool> AddPatient(Patient patient)
        {
            try
            {
                await _unitOfWork.Patient.AddAsync(patient);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdatePatient(Patient patient)
        {
            try
            {
                _unitOfWork.Patient.Update(patient);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeletePatient(int id)
        {
            try
            {
                var patientAppointments = await _unitOfWork.Appointment.DeleteAppointmentsByPatientId(id);

                var patientPrescriptions = await _unitOfWork.Prescription.DeletePrescriptionsByPatientId(id);

                var patient = await _unitOfWork.Patient.GetByIdAsync(id);
                if (patient == null) return false;

                _unitOfWork.Patient.Remove(patient);

                var user = await _unitOfWork.User.GetUserByUUID(patient.Userid);

                _unitOfWork.User.Remove(user);

                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
