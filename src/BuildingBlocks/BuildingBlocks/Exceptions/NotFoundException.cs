namespace BuildingBlocks.Exceptions;

[Serializable]
public class NotFoundException : Exception
{
    public NotFoundException() { }

    public NotFoundException(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.") { }

    public NotFoundException(string name, object key, Exception innerException) : base($"Entity \"{name}\" ({key}) was not found.", innerException) { }
}
