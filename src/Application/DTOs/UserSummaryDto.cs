namespace Nexus.User.Application.Dtos;

public record UserSummaryDto(Guid Id, string Email, string FullName, string Status);
