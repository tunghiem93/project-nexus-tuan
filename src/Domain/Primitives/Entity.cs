namespace Nexus.Abstractions.Primitives;

public abstract class Entity : IEntity
{
    public Guid Id { get; set; }
}
