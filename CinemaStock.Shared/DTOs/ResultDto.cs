namespace CinemaStock.Shared.ApiEndpointControllers;

public record ResultDto(bool IsSuccess, List<string>? Errors);