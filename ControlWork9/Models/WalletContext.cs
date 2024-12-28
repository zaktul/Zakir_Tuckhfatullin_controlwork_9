using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControlWork9.Models;

public class WalletContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<SomeProvider> SomeProviders { get; set; }
    public DbSet<ServiceUser> ServiceUsers { get; set; }
    
    public WalletContext(DbContextOptions<WalletContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<SomeProvider>().HasData(
            new SomeProvider { Id = 1, Name = "Steam" },
            new SomeProvider { Id = 2, Name = "O!" },
            new SomeProvider { Id = 3, Name = "WildBerries" },
            new SomeProvider { Id = 4, Name = "Mega" }
        );
        
        modelBuilder.Entity<Transaction>()
            .HasOne(a => a.Reciever)
            .WithMany(u => u.TransactionsU)
            .HasForeignKey(a => a.RecieverId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Transaction>()
            .HasOne(a => a.Sender)
            .WithMany(u => u.TransactionsSendU)
            .HasForeignKey(a => a.SenderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ServiceUser>()
            .HasOne(a => a.Owner)
            .WithMany(u => u.ServiceUsersU)
            .HasForeignKey(a => a.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ServiceUser>()
            .HasOne(su => su.SomeProvider)
            .WithMany(sp => sp.ServiceUsers)
            .HasForeignKey(su => su.ServiceProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}