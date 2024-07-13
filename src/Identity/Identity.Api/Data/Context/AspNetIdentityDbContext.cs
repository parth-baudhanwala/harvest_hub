using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Identity.Api.Data.Context;

public class AspNetIdentityDbContext(DbContextOptions<AspNetIdentityDbContext> options)
    : IdentityDbContext(options);
