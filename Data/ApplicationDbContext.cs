using Microsoft.EntityFrameworkCore;
using SignupApp.Models;

namespace SignupApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<SignupModel> Users { get; set; }
}