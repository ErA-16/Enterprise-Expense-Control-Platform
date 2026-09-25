using BCrypt.Net;
using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Models;
using Business_Operations_Expense_Control_Platform.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Business_Operations_Expense_Control_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private readonly ILogger<AuthController> _logger;
        private readonly IJwtService _jwtService;

        public AuthController(DatabaseContext db, ILogger<AuthController> logger, IJwtService jwtService)
        {
            _db = db;
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Signup(CreateUserDto request)
        {
            _logger.LogInformation("Creating Account for {userEmail}", request.Email);
            var userExists = await _db.Users.AnyAsync(u => u.Email == request.Email);

            if (userExists)
            {
                _logger.LogWarning("Account creation failed. {userEmail} is being used.", request.Email);
                return BadRequest("An employee with this email exists.");
            }

            var departmentExists = await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId);

            if (!departmentExists)
            {
                _logger.LogWarning("Account creation failed. Department ID {DeptId} not found.", request.DepartmentId);
                return BadRequest("Selected department does not exist");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newSignup = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                DepartmentId = request.DepartmentId,
                PasswordHash = hashedPassword,
            };

            await _db.Users.AddAsync(newSignup);
            await _db.SaveChangesAsync();

            return Ok("Registration successful.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserAuthDto request)
        {
            _logger.LogInformation("Login attempt for {Email}", request.Email);

            var selectedUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (selectedUser == null)
            {
                _logger.LogWarning("Login failed: Email {Email} not found.", request.Email);
                return Unauthorized("Invalid email or password");
            }

            var isValidPasswword = BCrypt.Net.BCrypt.Verify(request.Password, selectedUser.PasswordHash);

            if (!isValidPasswword)
            {
                _logger.LogWarning("Login failed: Incorrect password for {Email}.", request.Email);
                return Unauthorized("Invalid email or password.");
            }

            var token = _jwtService.GenerateToken(selectedUser);

            return Ok(new
            {
                token = token,
                Message = "Login successful!",
            });
        }
    }
}
