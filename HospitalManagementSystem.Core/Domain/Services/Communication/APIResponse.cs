namespace HospitalManagementSystem.Core.Domain.Services.Communication
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
        public APIResponse()
        {

        }
    }

    public class BooleanResponse : BaseResponse<bool>
    {
        public BooleanResponse(bool category) : base(category) { }
        public BooleanResponse(string message) : base(message) { }
    }
}
