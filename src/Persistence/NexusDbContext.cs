using Microsoft.EntityFrameworkCore;
using Nexus.Abstractions.Outbox;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence;

public abstract class NexusDbContext : DbContext
{
    protected NexusDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(NexusDbContext).Assembly);
    }

    private static string GetTableName(string entityName)
    {
        return entityName switch
        {
            nameof(UserAccount) => "User",
            nameof(PasswordResetToken) => "PasswordReset",
            nameof(OutboxMessage) => "outbox_events",
            _ => entityName
        };
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new System.Text.StringBuilder();
        var previousCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(value[0]);
        builder.Append(char.ToLowerInvariant(value[0]));

        for (var i = 1; i < value.Length; i++)
        {
            var c = value[i];
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == System.Globalization.UnicodeCategory.UppercaseLetter || category == System.Globalization.UnicodeCategory.TitlecaseLetter)
            {
                if (previousCategory != System.Globalization.UnicodeCategory.SpaceSeparator &&
                    previousCategory != System.Globalization.UnicodeCategory.UppercaseLetter &&
                    previousCategory != System.Globalization.UnicodeCategory.TitlecaseLetter)
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }

            previousCategory = category;
        }

        return builder.ToString();
    }
}
