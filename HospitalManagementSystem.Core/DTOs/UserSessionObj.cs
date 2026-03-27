using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.DTOs
{
    public class UserSessionObj
    {
        public string Uuid { get; set; }

        public string fullname { get; set; }
        public string dob { get; set; }
        public string mailId { get; set; }
        public int sub { get; set; }
        public string suid { get; set; }
        public string mobileNo { get; set; }
        public string gender { get; set; }
        public string country { get; set; }

        public string? emiratesId { get; set; }
        public string? passportNo { get; set; }

        //public List<string> AccessibleModule { get; set; }
    }
}
