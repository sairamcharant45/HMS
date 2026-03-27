using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Core.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Services
{
    public class AppointmentService :IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AppointmentService> _logger;
       
        public AppointmentService(IUnitOfWork unitOfWork,ILogger<AppointmentService> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> GetCountOfActiveAppointments()
        {
            return await _unitOfWork.Appointment.GetActiveAppointmentsCount();
        }
        
        public async Task<int> GetCountOfPendingAppointments()
        {
            return await _unitOfWork.Appointment.GetPendingAppointmentsCount();
        }

       
        public async Task<Appointment> GetAppointmentById(int Id)
        {
            return  await _unitOfWork.Appointment.GetAppointmentById(Id);
        }


        public async Task<IEnumerable<Appointment>> GetAllAppointmentsOfDoctor(int docId)
        {
            return await _unitOfWork.Appointment.GetAppointmentsByPatientId(docId);
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsList()
        {
            return await _unitOfWork.Appointment.GetAllAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientId(int docId)
        {
            return await _unitOfWork.Appointment.GetAppointmentsByPatientId(docId);

        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorId(int docId)
        {
            return await _unitOfWork.Appointment.GetAppointmentsByDoctorId(docId);

        } 

        public async Task<bool> UpdateAppointment(Appointment appointment)
        {
            try
            {
                _unitOfWork.Appointment.Update(appointment);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Update Appointment : " + ex.ToString());
                return false;
            }
        }
        public async Task<bool> CreateAppointment(Appointment appointment)
        {
            try
            {
                await _unitOfWork.Appointment.AddAsync(appointment);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create Appointment : "+ex.ToString());
                return false;
            }
        }

        public async Task<bool> DeleteAppointment(int id)
        {
            try
            {
                //var patientPrescriptions = await _unitOfWork.Prescription.DeletePrescriptionsByPatientId(id);
                //if (patientPrescriptions == 0) return false;

                var patient = await _unitOfWork.Appointment.GetByIdAsync(id);
                if (patient == null) return false;

                _unitOfWork.Appointment.Remove(patient);


                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete Appointment : " + ex.ToString());
                return false;
            }
        }
    }
}
