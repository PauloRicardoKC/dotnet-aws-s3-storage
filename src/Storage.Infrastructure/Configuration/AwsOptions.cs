namespace Storage.Infrastructure.Configuration;

public sealed class AwsOptions
{
    public const string SectionName = "Aws";
    public string Region { get; init; } = "us-east-1";
    public string BucketName { get; init; } = string.Empty;
    public string? AccessKey { get; init; }
    public string? SecretKey { get; init; }
}
