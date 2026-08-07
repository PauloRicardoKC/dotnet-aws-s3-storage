namespace Storage.Infrastructure.Constants;

internal static class AwsS3Constants
{
    internal const string FileNameMetadataKey = "file-name";
    internal const string DefaultContentType = "application/octet-stream";
    internal const int PresignedUrlExpirationMinutes = 15;
}
