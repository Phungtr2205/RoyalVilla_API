using AutoMapper;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models.DTO;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Models;
using Microsoft.Identity.Client;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace RoyalVilla_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(ApplicationDbContext db, IConfiguration configuration, IMapper mapper)
        {
            _db = db;
            _configuration = configuration;
            _mapper = mapper;
        }



        public async Task<bool> IsEmailExistingAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDTO.Email.ToLower());
                if (user == null || user.Password != loginRequestDTO.Password)
                {
                    return null;
                }
                //generate Token
                var token = GenerateJwtToken(user);
                return new LoginResponseDTO
                {
                    userDTO = _mapper.Map<UserDTO>(user),
                    Token = token
                };
            }
            catch (Exception ex)
            {
                //Handle any other unexpected errors
                throw new InvalidOperationException("An unexpected error occurred during user login.", ex);

            }
        }

        public async Task<UserDTO?> RegisterAsync(RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {
                if (await IsEmailExistingAsync(registerationRequestDTO.Email))
                {
                    throw new InvalidOperationException($"Email {registerationRequestDTO.Email} is already registered.");
                }

                User user = new()
                {
                    Email = registerationRequestDTO.Email,
                    Name = registerationRequestDTO.Name,
                    Password = registerationRequestDTO.Password,
                    Role = string.IsNullOrEmpty(registerationRequestDTO.Role) ? "Customer" : registerationRequestDTO.Role,
                    CreatedDate = DateTime.Now,
                };

                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex)
            {
                //Handle any other unexpected errors
                throw new InvalidOperationException("An unexpected error occurred during user registration.", ex);

            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.Name, user.Name),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }
            
    }
}
