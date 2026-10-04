namespace CinemaStock.Client.Validation;

public interface IImageValidator
{
    // public кортеж
    (bool IsValid, string? ErrorMessage) ValidateHeaderAndSize(
        byte[] fileBytes,
        string fileName,
        long maxSizeBytes,
        string[] allowedExtensions);

    bool IsValidImageSignature(byte[] bytes);
}