namespace Nexus.User.Contracts.Dtos;

public record UserSummaryDto(Guid Id, string Email, string Username, string FullName, string Status);
