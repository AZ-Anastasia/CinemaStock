using CinemaStock.Shared.Resources.UserErrors;

namespace CinemaStock.Client.Validation;

public class ImageValidator : IImageValidator
{
    public (bool IsValid, string? ErrorMessage) ValidateHeaderAndSize(byte[] fileBytes, string fileName, long maxSizeBytes, string[] allowedExtensions)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            return (false, $"{UserErrors.BadFileFormat}: {string.Join(", ", allowedExtensions)}");
        }

        if (fileBytes.Length > maxSizeBytes)
        {
            return (false, $"{UserErrors.TooLargeFileSize}: {maxSizeBytes / 1024 / 1024} МБ.");
        }

        if (!IsValidImageSignature(fileBytes))
        {
            return (false, $"{UserErrors.ErrorFileFormat}");
        }

        return (true, null);
    }

    public bool IsValidImageSignature(byte[] bytes)
    {
        if (bytes.Length < 4) return false;

        // JPEG: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return true;

        // PNG: 89 50 4E 47
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;

        // WEBP: RIFF....WEBP (проверяем RIFF на старте и WEBP в позициях 8-11)
        if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46)
        {
            if (bytes.Length >= 12 && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                return true;
        }

        return false;
    }
}