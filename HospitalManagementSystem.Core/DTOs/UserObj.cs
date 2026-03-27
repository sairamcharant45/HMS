using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.DTOs
{
    public class UserObj
    {
        public string suid { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public string birthdate { get; set; }
        public string email { get; set; }
        public string sub { get; set; }
        public string phone { get; set; }
        public string id_document_type { get; set; }
        public string id_document_number { get; set; }
        public string loa { get; set; }
    }
}
