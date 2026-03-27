using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.Domain.Services
{
    public interface IUserService
    {
        public Task<IEnumerable<User>> ListAllUsersAsync();
        public Task<bool> isUserExists(string mail);
        public Task<User> GetUserDetailsByUUID(string id);
        public Task<bool> CreateUser(User newUser);
        public Task<bool> UpdateUser(User newUserData);
        public Task<User> GetUserByEmail(string email);

        public Task<ServiceResult> VerifyUserLogin(string email, string password);
    }
}
