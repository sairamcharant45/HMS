namespace HospitalManagementSystem.Web.Models
{
    //public class GetUserProfileResult
    //{
    //    public string suid { get; set; }
    //    public string birthdate { get; set; }
    //    public string name { get; set; }
    //    public string phone_number { get; set; }
    //    public bool phone_number_verified { get; set; }
    //    public string email { get; set; }
    //    public bool email_verified { get; set; }
    //    public string gender { get; set; }
    //    public string id_document_type { get; set; }
    //    public string id_document_number { get; set; }
    //    public string loa { get; set; }
    //    public string country { get; set; }
    //    public string photo { get; set; }
    //}

    //public class GetUserProfileResponse
    //{
    //    public bool success { get; set; }
    //    public string message { get; set; }
    //    public GetUserProfileResult result { get; set; }
    //}




    public class GetUserProfileResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public GetUserProfileResult result { get; set; }
    }
    public class GetUserProfileResult
    {
        public string suid { get; set; }
        public string birthdate { get; set; }
        public string name { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string id_document_type { get; set; }
        public string id_document_number { get; set; }
        public string loa { get; set; }
        public string country { get; set; }
        public string photo { get; set; }
    }

    public class GetUserProfileResponse1
    {
        public bool success { get; set; }
        public string message { get; set; }
        public GetUserProfileResult1 result { get; set; }
    }

    public class GetUserProfileResult1
    {
        public string gender { get; set; }
        public string email { get; set; }
        public string photo { get; set; }
        public string country { get; set; }
        public string subscriberUid { get; set; }
        public string displayName { get; set; }
        public string dateOfBirth { get; set; }
        public string mobileNumber { get; set; }
    }
}
