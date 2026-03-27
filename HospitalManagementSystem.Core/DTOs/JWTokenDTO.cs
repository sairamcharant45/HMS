using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.DTOs
{
    public class JWTokenDTO
    {
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public string Audience { get; set; }
        public long Expiry { get; set; }
        public long IssuedAt { get; set; }

        public string ClienId { get; set; }
        public string RedirecUri { get; set; }
        public string ResponseType { get; set; }
        public string Scope { get; set; }
        public string State { get; set; }
        public string Nonce { get; set; }
    }
}
