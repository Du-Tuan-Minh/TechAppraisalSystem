namespace Application.Common
{
    public class ApiResponse<T>
    {
            public bool IsSuccess { get; set; }
            public T? Data { get; set; }
            public string? Message { get; set; }
            public List<string>? Errors { get; set; }
            public int StatusCode { get; set; }

            public static ApiResponse<T> Success(T data, string message = "Success", int statusCode = 200)
                => new() { IsSuccess = true, Data = data, Message = message, StatusCode = statusCode };

            public static ApiResponse<T> Failure(int statusCode, string message, List<string>? errors = null)
                => new() { IsSuccess = false, Message = message, Errors = errors ?? new(), StatusCode = statusCode };
    }
}