using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly TokenService _tokenService;

        public PatientsController(AppDBContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterPatientDto registerDto)
        {
            if (await _context.Patients.AnyAsync(p => p.Login == registerDto.Login))
            {
                return BadRequest("Login already exists");
            }

            var patient = new Patient
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Login = registerDto.Login,
                Password = registerDto.Password 
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var patient = await _context.Patients.SingleOrDefaultAsync(p => p.Login == loginDto.Login && p.Password == loginDto.Password);

            if (patient == null)
            {
                return Unauthorized("Invalid login or password");
            }

            var accessToken = _tokenService.GenerateAccessToken(patient);
            var refreshToken = _tokenService.GenerateRefreshToken();

            patient.RefreshToken = refreshToken;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
    }

    public class TokenService
    {
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public TokenService(string jwtSecret, string jwtIssuer, string jwtAudience)
        {
            _jwtSecret = jwtSecret;
            _jwtIssuer = jwtIssuer;
            _jwtAudience = jwtAudience;
        }

        public string GenerateAccessToken(Patient patient)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, patient.Login),
                    new Claim(ClaimTypes.NameIdentifier, patient.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
