using ECommerceAPI.ECommerceUserAPI.Models;
using ECommerceAPI.ECommerceUserAPI.Services;
using ECommerceUserAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly TokenBlacklistService _tokenBlacklistService;

    public AuthController(IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, TokenService tokenService, TokenBlacklistService tokenBlacklistService)
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _tokenBlacklistService = tokenBlacklistService;
    }
 
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
        {
            return Unauthorized(new { message = "Invalid login attempt" });
        }

        var token = _tokenService.GenerateToken(user.Id);
        return Ok(new { Token = token });
    }
  
    //[AllowAnonymous]
    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (model.Password != model.ConfirmPassword)
        {
            return BadRequest(new { message = "Passwords do not match." });
        }

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(new { message = "Registration successful" });
        }
        return BadRequest(result.Errors);
    }
  
    [HttpGet("userDetails")]
    //[Authorize]
    public async Task<IActionResult> GetUserDetails()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

        // Get the authenticated user's ID from the claims
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }

        // Fetch the user from the database using UserManager
        var user = await _userManager.FindByIdAsync(userIdClaim);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            User = new {
                user.Id,
                user.UserName,
                user.Email
            },
            Roles = roles
        });
    }
    private string GenerateJwtToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.Now.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        try
        {
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            // Log the exception (ex.Message) for troubleshooting
            throw;
        }
    }
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Access the JTI claim
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        await _tokenBlacklistService.LogoutAsync(jti);
        return Ok(new { message = "Logged out successfully." });
    }
}
