using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class ReputationProfile : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public decimal ReputationScore { get; set; }
    public string TrustLevel { get; set; } = string.Empty;
    public int SuccessfulTransactionCount { get; set; }
    public int FailedActivityCount { get; set; }
    public int AuctionWinCount { get; set; }
    public int AuctionFailCount { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public UserAccount User { get; set; } = null!;
}
