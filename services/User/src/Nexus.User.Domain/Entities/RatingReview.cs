using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class RatingReview : Entity
{
    public Guid TransactionRefId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid RaterUserId { get; set; }
    public Guid RatedUserId { get; set; }
    public string FeedbackType { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string? Comment { get; set; }
    public bool IsDisputed { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }

    public UserAccount RaterUser { get; set; } = null!;
    public UserAccount RatedUser { get; set; } = null!;
}
