namespace HospitalManagementSystem.Web.Communication
{
    public class APIResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
        public APIResponse(string Message)
        {
            this.Success = false;
            this.Message = Message;
            this.Result = null;
        }
        public APIResponse(bool success, string Message)
        {
            this.Success = success;
            this.Message = Message;
            this.Result = null;
        }
        public APIResponse()
        {

        }
    }
}
