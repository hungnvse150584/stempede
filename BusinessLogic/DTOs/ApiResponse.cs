namespace BusinessLogic.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, T? data, string? message, IEnumerable<string>? errors)
        {
            Success = success;
            Data = data;
            Message = message;
            Errors = errors;
        }

        // Convenience constructors
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful.")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> FailureResponse(string message, IEnumerable<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}