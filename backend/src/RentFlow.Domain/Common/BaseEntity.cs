namespace RentFlow.Domain.Common;

/// <summary>
/// Represents the base entity class that provides common properties for all domain entities.
/// </summary>
/// <remarks>
/// This class serves as the foundation for all entities in the domain model,
/// providing audit and identification fields that are inherited by derived entities.
/// </remarks>
public class BaseEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
