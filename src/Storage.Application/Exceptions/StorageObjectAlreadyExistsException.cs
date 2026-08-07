namespace Storage.Application.Exceptions;

public sealed class StorageObjectAlreadyExistsException(string key)
    : Exception($"A storage object with key '{key}' already exists.")
{
    public string Key { get; } = key;
}
