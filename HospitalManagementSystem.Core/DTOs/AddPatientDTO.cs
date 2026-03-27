using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.DTOs
{
    public class AddPatientDTO
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneNo { get; set; }

        public string Contact { get; set; }

        public string BloodGroup { get; set; }

        public string Gender { get; set; }

        public string DateOfBirth { get; set; }

        public string Address { get; set; }
    }
}
