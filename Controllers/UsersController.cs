using MasterUserAccountAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MasterUserAccountAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MasterUserAccountDBContext _context;
        private readonly IConfiguration _configuration;


        public UsersController(MasterUserAccountDBContext context, IConfiguration configuration)
        {
            this._context = context;
            _configuration = configuration;

        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            var token = GenerateJwtToken(user);
            var applications = _context.UserApplications.Where(x => x.UserId == user.UserId).ToList();
            bool check = CheckIfUserBelongsToApplication(user.UserId, model.ApplicationId);
            return Ok(new
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = token,
                UserBelongsToApplication = check,
            });
        } 
        
        
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            var token = GenerateJwtToken(user);
            var applications = _context.UserApplications.Where(x => x.UserId == user.UserId).ToList();
            return Ok(new
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = token,
            });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        } 
        
        [HttpGet("Applications")]
        public async Task<ActionResult<IEnumerable<Application>>> GetApplication()
        {
            return await _context.Applications.ToListAsync();
        }

        [HttpPost]
        [Route("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            // TODO: Implement password reset logic
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null)
            {
                // User with this email address not found
                return NotFound();
            }

            user.Password = model.Password;
            return Ok();
        }


        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [Authorize]
        [HttpPost("CreateUser")]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        [Authorize]
        [HttpGet("GetUserApplicationAssignment/{id}")]
        public async Task<ActionResult<IEnumerable<UserApplication>>> GetUserApplicationAssignment(int id)
        {
            return await _context.UserApplications.Where(x=>x.UserId == id).ToListAsync();
        }

        [Authorize]
        [HttpPost("AssignUserToApplication")]
        public async Task<ActionResult> AssignUser([FromBody] IsAssidnedUser model)
        {
            if (model.ApplicationId.Count == 0)
            {
               var results = _context.UserApplications.Where(x=>x.UserId == model.UserId).ToList();
                foreach (var item in results)
                {
                    _context.UserApplications.Remove(item);
                }
            }
            else
            {
				foreach (var item in model.ApplicationId)
				{
					if (_context.UserApplications.Any(x => x.UserId == model.UserId && x.UserApplicationId == item))
					{

					}
					else
					{
						_context.UserApplications.Add(new UserApplication
						{
							ApplicationId = item,
							UserId = model.UserId,
							UserCredentials = "TestCredentials"
						});
					}

				}
			}

			await _context.SaveChangesAsync();

			return Ok();
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        private bool CheckIfUserBelongsToApplication(int UserId, int ApplicationId)
        {
            return _context.UserApplications.Any(x => x.UserId == UserId && x.ApplicationId == ApplicationId);
        }


        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.UserId.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
