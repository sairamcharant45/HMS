namespace HospitalManagementSystem.Web.Models
{
    public class GetUserProfileRequest
    {
        public string UserId { get; set; }
        public string UserIdType { get; set; }
        public string ProfileType { get; set; }
        public string Purpose { get; set; }
        public string ClientId { get; set; }
        public string Scopes { get; set; }
        public string Token { get; set; }
        public string Suid { get; set; }
    }
}
