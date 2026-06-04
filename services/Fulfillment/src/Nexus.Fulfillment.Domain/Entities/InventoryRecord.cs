using Nexus.Abstractions.Primitives;

namespace Nexus.Fulfillment.Domain.Entities;

public class InventoryRecord : Entity
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
}
