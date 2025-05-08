using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserLogin.Model;
using static System.Net.WebRequestMethods;

namespace UserLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MyDBContext dbContext;
        private readonly IConfiguration configuration;
        public UsersController(MyDBContext dBContext, IConfiguration configuration)
        {
            this.dbContext = dBContext;
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration(UserDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var objUser = dbContext.Users.FirstOrDefault(x => x.Email == userDTO.Email);
            if (objUser == null)
            {
                dbContext.Users.Add(new User
                {
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Email = userDTO.Email,
                    Password = userDTO.Password
                });
                dbContext.SaveChanges();
                return Ok("User registered successfully");
            }

            else
            {
                return BadRequest("User is already exists with the same email address!");
            }


        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LogInDTO logInDTO)
        {
            var user = dbContext.Users.FirstOrDefault(x=>x.Email == logInDTO.Email && x.Password ==logInDTO.Password);
            if (user != null)
            {
                //Adding authentication logic
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub,configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("Email", user.Email.ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
                var signin = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    configuration["Jwt:Issuer"],
                    configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: signin
                    );
                string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

                //below is returning an HTTP 200 OK response with a JSON object containing two properties:
                //Token → tokenValue(likely a JWT token used for authentication).
                //User → user(the authenticated user object).

                return Ok(new { Token = tokenValue , User = user});
                //return Ok(user);
            }
           
               return  BadRequest("User is not exists!");
            
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetUsers() 
        {
            return Ok(dbContext.Users.ToList());
        }

        //[Authorize]
        [HttpGet]
        [Route("GetUserByID")]
        public IActionResult GetUserById(int id)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.UserId == id);

            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest("No user found");
            }
        }
    }
}
