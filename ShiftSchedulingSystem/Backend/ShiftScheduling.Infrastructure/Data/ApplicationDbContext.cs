using Microsoft.EntityFrameworkCore;
using ShiftScheduling.Core.Entities;

namespace ShiftScheduling.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftSwapRequest> ShiftSwapRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Shift configuration
            modelBuilder.Entity<Shift>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(s => s.User)
                    .WithMany(u => u.Shifts)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => new { s.UserId, s.ShiftDate }).IsUnique();
                entity.Property(s => s.ShiftType).IsRequired().HasMaxLength(50);
            });

            // ShiftSwapRequest configuration
            modelBuilder.Entity<ShiftSwapRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(r => r.RequestorShift)
                    .WithMany()
                    .HasForeignKey(r => r.RequestorShiftId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.RequestedShift)
                    .WithMany()
                    .HasForeignKey(r => r.RequestedShiftId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Requestor)
                    .WithMany(u => u.SentSwapRequests)
                    .HasForeignKey(r => r.RequestorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.RequestedUser)
                    .WithMany(u => u.ReceivedSwapRequests)
                    .HasForeignKey(r => r.RequestedUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => r.Status);
                entity.Property(r => r.Status).IsRequired().HasMaxLength(20);
            });

            // Seed data is commented out - add manually if needed
            // SeedData(modelBuilder);
        }

        /*
        // Seed data method - commented out to avoid BCrypt errors
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Create default manager account
            var managerPasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123");
            
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Email = "manager@shiftsystem.com",
                PasswordHash = managerPasswordHash,
                FirstName = "John",
                LastName = "Manager",
                Role = "Manager",
                PhoneNumber = "+1234567890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // Create sample employee accounts
            var employeePasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123");
            
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 2,
                    Email = "alice@shiftsystem.com",
                    PasswordHash = employeePasswordHash,
                    FirstName = "Alice",
                    LastName = "Johnson",
                    Role = "Employee",
                    PhoneNumber = "+1234567891",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 3,
                    Email = "bob@shiftsystem.com",
                    PasswordHash = employeePasswordHash,
                    FirstName = "Bob",
                    LastName = "Smith",
                    Role = "Employee",
                    PhoneNumber = "+1234567892",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 4,
                    Email = "carol@shiftsystem.com",
                    PasswordHash = employeePasswordHash,
                    FirstName = "Carol",
                    LastName = "Davis",
                    Role = "Employee",
                    PhoneNumber = "+1234567893",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Create sample shifts for the current week
            var today = DateTime.Today;
            var daysUntilMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var startOfWeek = today.AddDays(-daysUntilMonday);
            
            modelBuilder.Entity<Shift>().HasData(
                new Shift
                {
                    Id = 1,
                    UserId = 2,
                    ShiftDate = startOfWeek,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    ShiftType = "Morning",
                    Department = "Operations",
                    Notes = "Regular shift",
                    CreatedAt = DateTime.UtcNow
                },
                new Shift
                {
                    Id = 2,
                    UserId = 3,
                    ShiftDate = startOfWeek,
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(21, 0, 0),
                    ShiftType = "Afternoon",
                    Department = "Operations",
                    Notes = "Regular shift",
                    CreatedAt = DateTime.UtcNow
                },
                new Shift
                {
                    Id = 3,
                    UserId = 4,
                    ShiftDate = startOfWeek.AddDays(1),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    ShiftType = "Morning",
                    Department = "Customer Service",
                    Notes = "Customer support shift",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
        */
    }
}