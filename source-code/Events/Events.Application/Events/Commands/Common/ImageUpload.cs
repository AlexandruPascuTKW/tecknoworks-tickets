namespace Events.Application.Events.Commands.Common;

/// <summary>EVT-F6 — a picture sent with a command (as base64 in JSON).</summary>
public class ImageUpload
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

/// <summary>ADR-008: JPEG, PNG or WebP up to 2 MB, recognised from the first bytes, not from the name.</summary>
public static class ImageRules
{
    public const int MaxBytes = 2 * 1024 * 1024;

    /// <summary>The real content type of the picture, or null if it is not a format we accept.</summary>
    public static string? Detect(byte[] content)
    {
        if (content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (content.Length >= 8 && content.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return "image/png";
        }

        if (content.Length >= 12
            && content.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && content.AsSpan(8, 4).SequenceEqual("WEBP"u8))
        {
            return "image/webp";
        }

        return null;
    }
}

public static class ImageUploadValidation
{
    public static IRuleBuilderOptions<T, ImageUpload?> MustBeAcceptableImage<T>(this IRuleBuilder<T, ImageUpload?> rule)
        => rule
            .Must(i => i is null || (i.Content.Length > 0 && i.Content.Length <= ImageRules.MaxBytes))
            .WithMessage("Image must be up to 2 MB.")
            .Must(i => i is null || ImageRules.Detect(i.Content) is not null)
            .WithMessage("Image must be a JPEG, PNG or WebP picture.");
}
