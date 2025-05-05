namespace TaskApi.Application.Responses
{
    public class ApiResponse : ApiResponse<object> 
    { 
        public static new ApiResponse Ok(string message = "OK")
            => new() { StatusCode = 200, Message = message};

        public static new ApiResponse Fail(int code, string message, string? errorId = null)
            => new() { StatusCode = code, Message = message, ErrorId = errorId };
    }

    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorId { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "OK")
        => new() { StatusCode = 200, Message = message, Data = data };

        public static ApiResponse<T> Fail(int code, string message, string? errorId = null)
            => new() { StatusCode = code, Message = message, ErrorId = errorId };
    }
}
