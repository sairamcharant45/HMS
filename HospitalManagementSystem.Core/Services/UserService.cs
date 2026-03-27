using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<User>> ListAllUsersAsync()
        {
            return await _unitOfWork.User.GetAllAsync();
        }

        public async Task<bool> isUserExists(string mail)
        {
            return await _unitOfWork.User.isUserEmailExists(mail);
        }

        public async Task<User> GetUserDetailsByUUID(string UUID)
        {
            return await _unitOfWork.User.GetUserByUUID(UUID);
        }

        public async Task<bool> CreateUser(User newUser)
        {
            try
            {
                await _unitOfWork.User.AddAsync(newUser);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUser(User newUserData)
        {
            try
            {
                _unitOfWork.User.Update(newUserData);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _unitOfWork.User.GetUserByEmailID(email);
        }

        public async Task<ServiceResult> VerifyUserLogin(string email, string password)
        {
            var userInDB = await GetUserByEmail(email);
            if (userInDB == null)
            {
                return new ServiceResult(false, "Email doesn't exist");
            }

            if (password == userInDB.Password)
            {
                return new ServiceResult(true, "Verification Successful");
            }
            return new ServiceResult(false, "Incorrect Password");
        }
    }
}
