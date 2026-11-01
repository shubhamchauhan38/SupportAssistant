
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SupportAssistant.API.DTOs;

namespace SupportAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        const string employeeRole = "Employee";

        // Ensure the Employee role exists.
        if (!await _roleManager.RoleExistsAsync(employeeRole))
        {
            var roleResult = await _roleManager.CreateAsync(
                new IdentityRole(employeeRole));

            if (!roleResult.Succeeded)
            {
                return Problem("Could not initialize the Employee role.");
            }
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return BadRequest("An account with this email already exists.");
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(
            user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        var roleResultForUser = await _userManager.AddToRoleAsync(
            user, employeeRole);

        if (!roleResultForUser.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return Problem("Could not assign the Employee role.");
        }

        return Ok(new
        {
            message = "Registration successful.",
            email = user.Email,
            role = employeeRole
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null ||
            !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtKey = _configuration["Jwt:Key"]!;
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiryMinutes = _configuration.GetValue<int>(
            "Jwt:ExpiryMinutes", 60);

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return Ok(new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email!,
            Role = roles.FirstOrDefault() ?? "None",
            ExpiresAt = expiresAt
        });
    }
}
