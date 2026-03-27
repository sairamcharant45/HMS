using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using HospitalManagementSystem.Core.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Services
{
    public class DoctorService: IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DoctorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> GetDoctorsCount()
        {
            return await _unitOfWork.Doctor.GetCountOfDoctors();
        }

        public async Task<IEnumerable<Doctor>> ListAllDoctorsAsync()
        {
            return await _unitOfWork.Doctor.GetAllAsync();
        }

        public async Task<Doctor> GetDoctorById(int id)
        {
            return await _unitOfWork.Doctor.GetByIdAsync(id);
        }

        public async Task<bool> AddDoctor(Doctor doctor)
        {
            try
            {
                await _unitOfWork.Doctor.AddAsync(doctor);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateDoctor(Doctor doctor)
        {
            try
            {
                _unitOfWork.Doctor.Update(doctor);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteDoctor(int id)
        {
            try
            {
                var doctorAppointments = await _unitOfWork.Appointment.DeleteAppointmentsByDoctorId(id);

                var doctorPrescriptions = await _unitOfWork.Prescription.DeletePrescriptionsByDoctorId(id);

                var doctor = await _unitOfWork.Doctor.GetByIdAsync(id);
                if (doctor == null) return false;

                _unitOfWork.Doctor.Remove(doctor);

                var user = await _unitOfWork.User.GetUserByUUID(doctor.Userid);

                _unitOfWork.User.Remove(user);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Doctor> GetDoctorDetailsByUUID(string uuid)
        {
            return await _unitOfWork.Doctor.GetDoctorByUUID(uuid);
        }
    }
}
