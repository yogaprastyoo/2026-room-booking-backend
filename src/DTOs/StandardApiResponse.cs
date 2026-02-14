namespace RoomBooking.Api.DTOs;

public class StandardApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }

    public static StandardApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully.")
    {
        return new StandardApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static StandardApiResponse<T> ErrorResponse(string message, Dictionary<string, List<string>>? errors = null)
    {
        return new StandardApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
