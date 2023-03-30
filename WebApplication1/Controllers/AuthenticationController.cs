
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using WebApplication1.Authorication;



namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Register")]

        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExist = await userManager.FindByNameAsync(model.UserName);
            if (userExist != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Statues = "Error", Message = "User Already Exist" });
            ApplicationUser applicationUser = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            var result = await userManager.CreateAsync(applicationUser, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Statues = "Error", Message = "User Creation Failed" });
            }
            return Ok(new Response { Statues = "Success", Message = "User Created SucessFully" });

        }

        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var applicationUser = await userManager.FindByNameAsync(model.UserName);
            if (applicationUser != null && await userManager.CheckPasswordAsync(applicationUser, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(applicationUser);

                var authClaims = new List<Claim>
                {
                    new Claim (ClaimTypes.Name, model.UserName),
                    new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),

                };
                foreach (var claim in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, claim));
                }

                var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secert"]));
                var token = new JwtSecurityToken
                   (
                   issuer: _configuration["JWT:ValidIssuer"],
                   audience: _configuration["JWT:ValidAudience"],
                   expires: DateTime.Now.AddHours(3),
                   claims: authClaims,
                   signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256)
                   );


                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    //expiration = token.ValidTo,
                    //User = applicationUser.UserName

                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("RegisterAdmin")]

        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExist = await userManager.FindByNameAsync(model.UserName);
            if (userExist != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Statues = "Error", Message = "User Already Exist" });
            ApplicationUser applicationUser = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            var result = await userManager.CreateAsync(applicationUser, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Statues = "Error", Message = "User Creation Failed" });
            }
            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            if (await roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await userManager.AddToRoleAsync(applicationUser, UserRoles.Admin);
            }

            return Ok(new Response { Statues = "Success", Message = "User Created SucessFully" });

        }
    }
}

