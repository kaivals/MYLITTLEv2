// Updated AuthController with JWT, roles, and role-based approval logic
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using mylittle_project.Application.DTOs;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace mylittle_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
             AppDbContext context,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _configuration = configuration;
            _config = config;
            _context = context;
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (model == null || model.Password != model.ConfirmPassword)
                return BadRequest("Invalid data or password mismatch");

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(model.Role))
                return BadRequest($"Role '{model.Role}' does not exist.");

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("A user with this email already exists.");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Role = model.Role,
                IsApproved = model.Role == "Guest" || model.Role == "Buyer"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign role
            await _userManager.AddToRoleAsync(user, model.Role);

            // Send confirmation email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, token }, Request.Scheme);
            var message = $"<h2>Welcome!</h2><p>Click to confirm: <a href='{HtmlEncoder.Default.Encode(link!)}'>Confirm Email</a></p>";

            await _emailSender.SendEmailAsync(model.Email, "Confirm your email", message);

            return Ok(new { message = "Registration successful. Please confirm your email.", userId = user.Id });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (model == null) return BadRequest("Invalid request body");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized("Invalid credentials or email not confirmed");

            // Admin approval check for elevated roles
            if (!user.IsApproved && user.Role is "Dealer" or "StoreManager" or "TenantOwner")
                return Unauthorized("Your account is not yet approved by the admin.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return Unauthorized("Invalid login attempt");

            // Generate JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expiresIn = int.TryParse(_configuration["Jwt:ExpiresInHours"], out var hours) ? hours : 2;
            var expires = DateTime.UtcNow.AddHours(expiresIn);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Role, user.Role!)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            // Save login info to UserCredentials table
            var userCredential = new UserCredential
            {
                UserId = user.Id,
                Role = user.Role!,
                Token = jwtToken,
                LoginTimestamp = DateTime.UtcNow,
                ExpiresAt = expires,
                IsActive = true
            };

            _context.UserCredentials.Add(userCredential);
            await _context.SaveChangesAsync();

            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                token = jwtToken,
                expires,
                role = user.Role,
                user = new
                {
                    id = user.Id,
                    name = user.FirstName + " " + user.LastName,
                    email = user.Email,
                    isApproved = user.IsApproved
                },
                message = "Login successful"
            });
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("approve-user/{userId}")]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);
            return Ok("User approved.");
        }


        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return Content("<h2 style='color:red;'>❌ Invalid confirmation link.</h2>", "text/html");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Content("<h2 style='color:red;'>❌ User not found.</h2>", "text/html");

            // Decode the token
            var decodedToken = Uri.UnescapeDataString(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                return Content(@"
            <html>
                <head><title>Email Confirmed</title></head>
                <body style='font-family:Arial; text-align:center; margin-top:80px;'>
                    <h1 style='color:green;'>🎉 Email Confirmed!</h1>
                    <p>You can now <a href='/login'>log in</a> to your account.</p>
                </body>
            </html>
        ", "text/html");
            }

            return Content("<h2 style='color:red;'>❌ Email confirmation failed. The link may have expired or already been used.</h2>", "text/html");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending-users")]
        public async Task<IActionResult> GetPendingUsers()
        {
            var pendingUsers = await _userManager.Users
                .Where(u => !u.IsApproved && (u.Role == "StoreManager" || u.Role == "TenantOwner" || u.Role == "Dealer"))
                .Select(u => new {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.Role
                })
                .ToListAsync();

            return Ok(pendingUsers);
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid user or token");

            var userToken = await _context.UserCredentials
                .FirstOrDefaultAsync(u => u.UserId == userId && u.Token == token && u.IsActive);

            if (userToken == null)
                return NotFound("Token not found or already inactive.");

            userToken.IsActive = false;
            await _context.SaveChangesAsync();

            await _signInManager.SignOutAsync();

            return Ok(new { message = "Logged out successfully." });
        }

    }
}
