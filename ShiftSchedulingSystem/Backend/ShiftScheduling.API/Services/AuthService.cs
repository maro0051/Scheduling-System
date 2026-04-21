using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShiftScheduling.Core.DTOs;
using ShiftScheduling.Core.Entities;
using ShiftScheduling.Infrastructure.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShiftScheduling.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IRepository<User> userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var users = await _userRepository.FindAsync(u => u.Email == loginDto.Email);
            var user = users.FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null;

            if (!user.IsActive)
                return null;

            var token = GenerateJwtToken(user);
            
            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            var exists = await _userRepository.ExistsAsync(u => u.Email == registerDto.Email);
            if (exists)
                return null;

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = registerDto.Role,
                PhoneNumber = registerDto.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);
            var token = GenerateJwtToken(createdUser);

            return new AuthResponseDto
            {
                UserId = createdUser.Id,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                Role = createdUser.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "ShiftSchedulingSuperSecretKey2024!@#$%^&*()_+");
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}