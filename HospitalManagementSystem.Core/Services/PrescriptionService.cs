using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using HospitalManagementSystem.Core.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PrescriptionService> _logger;
        public PrescriptionService(IUnitOfWork unitOfWork, ILogger<PrescriptionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<int> GetPrescriptionsCount()
        {
            return await _unitOfWork.Prescription.GetCountOfPrescriptions();
        }
        public async Task<IEnumerable<Prescription>> ListAllPrescriptionsAsync()
        {
            return await _unitOfWork.Prescription.GetAllAsync();
        }

        public async Task<IEnumerable<Prescription>> ListAllPrescriptionsOfDoctorAsync(int docId)
        {
            return await _unitOfWork.Prescription.GetAllPrescriptionsOfDoctor(docId);
        }

        public async Task<IEnumerable<Prescription>> ListAllPrescriptionsOfPatientAsync(int patientId)
        {
            return await _unitOfWork.Prescription.GetPrescriptionsByPatientId(patientId);
        }

        public async Task<bool> CreatePrescription(Prescription prescription)
        {
            try
            {
                await _unitOfWork.Prescription.AddAsync(prescription);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create Prescription : "+ex.ToString());
                return false;
            }
        }

        public async Task<Prescription> GetPrescriptionById(int id)
        {
            return await _unitOfWork.Prescription.GetByIdAsync(id);
        }

        public async Task<bool> UpdatePrescription(Prescription prescription)
        {
            try
            {
                _unitOfWork.Prescription.Update(prescription);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeletePrescriotion(int id)
        {
            try
            {
                var prescription = await _unitOfWork.Prescription.GetByIdAsync(id);
                if (prescription == null) return false;

                _unitOfWork.Prescription.Remove(prescription);
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
