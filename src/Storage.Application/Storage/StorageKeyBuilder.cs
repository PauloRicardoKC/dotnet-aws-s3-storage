namespace Storage.Application.Storage;

/// <summary>Builds canonical Amazon S3 object keys from an optional logical folder and a file name.</summary>
public sealed class StorageKeyBuilder
{
    public string Build(string? folder, string fileName)
    {
        var normalizedFileName = GetFileName(fileName);
        var normalizedFolder = NormalizeFolder(folder);

        return string.IsNullOrEmpty(normalizedFolder)
            ? normalizedFileName
            : $"{normalizedFolder}/{normalizedFileName}";
    }

    public string NormalizeFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
            return string.Empty;

        return string.Join('/', folder
            .Trim()
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries));
    }

    public string GetFileName(string fileName) => Path.GetFileName(fileName.Replace('\\', '/'));
}
