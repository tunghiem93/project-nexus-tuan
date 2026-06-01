using Microsoft.EntityFrameworkCore;

namespace Nexus.UserService.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    // TODO: DbSet<User>, DbSet<Role>, DbSet<Privilege> ...
    // Map entities theo schema: services/user-service/db/schema.sql
}
