using System.Text.Json.Serialization;

namespace CinemaStock.Shared.ApiEndpointControllers;

public record ResultDto<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public List<string>? Errors { get; init; }

    public ResultDto() { }

    public ResultDto(bool isSuccess, T? data, List<string>? errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Errors = errors;
    }

    public ResultDto(bool isSuccess, List<string>? errors) : this(isSuccess, default, errors)
    {
    }
}