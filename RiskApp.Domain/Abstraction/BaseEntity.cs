namespace RiskApp.Domain.Abstraction;

public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    protected BaseEntity() => Id = Guid.NewGuid(); // normal path
    protected BaseEntity(Guid id) => Id = id;// seeding/testing path

    public void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
