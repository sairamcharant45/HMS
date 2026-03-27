namespace HospitalManagementSystem.Core.Domain.Services.Communication
{
    public abstract class BaseResponse<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public T Result { get; private set; }

        protected BaseResponse(T resource)
        {
            Success = true;
            Message = string.Empty;
            Result = resource;
        }

        protected BaseResponse(T resource, string message)
        {
            Success = true;
            Message = message;
            Result = resource;
        }

        protected BaseResponse(string message)
        {
            Success = false;
            Message = message;
            Result = default;
        }
    }
}
