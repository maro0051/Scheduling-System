using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShiftScheduling.API.Services;
using ShiftScheduling.Infrastructure.Data;
using ShiftScheduling.Infrastructure.Repositories;
using ShiftScheduling.Core.Entities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "ShiftSchedulingSuperSecretKey2024!@#$%^&*()_+");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// Register Email Service
builder.Services.AddScoped<IEmailService, EmailService>();
// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IShiftSwapService, ShiftSwapService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Create database and seed admin user
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
    
    // Check if any admin exists
    if (!dbContext.Users.Any(u => u.Role == "Admin"))
    {
        var admin = new User
        {
            Email = "admin@shiftsystem.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),  // Fixed: Use full namespace
            FirstName = "System",
            LastName = "Administrator",
            Role = "Admin",
            PhoneNumber = "1234567890",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(admin);
        dbContext.SaveChanges();
        
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("Default Admin Created!");
        Console.WriteLine($"Email: admin@shiftsystem.com");
        Console.WriteLine($"Password: Admin123");
        Console.WriteLine("=".PadRight(60, '='));
    }
    
    // Also create a default manager if none exists (optional)
    if (!dbContext.Users.Any(u => u.Role == "Manager"))
    {
        var manager = new User
        {
            Email = "manager@shiftsystem.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123"),  // Fixed: Use full namespace
            FirstName = "John",
            LastName = "Manager",
            Role = "Manager",
            PhoneNumber = "1234567891",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(manager);
        dbContext.SaveChanges();
        
        Console.WriteLine("Default Manager Created!");
        Console.WriteLine($"Email: manager@shiftsystem.com");
        Console.WriteLine($"Password: Manager123");
        Console.WriteLine("=".PadRight(60, '='));
    }
}

app.Run();