using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Meziantou.PasswordManager.Web.Areas.Api.Configuration;
using Meziantou.PasswordManager.Web.Areas.Api.Data;
using Meziantou.PasswordManager.Web.Areas.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Meziantou.PasswordManager.Web.Areas.Api.Controllers
{
    [Area(Constants.ApiArea)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IOptions<JwtAuthentication> _jwtAuthentication;
        private readonly CurrentUserProvider _currentUserProvider;

        public UserController(UserRepository userRepository, IPasswordHasher passwordHasher, IOptions<JwtAuthentication> jwtAuthentication, CurrentUserProvider currentUserProvider)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtAuthentication = jwtAuthentication ?? throw new ArgumentNullException(nameof(jwtAuthentication));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
        }

        [HttpGet]
        public async Task<IActionResult> Me()
        {
            var user = await _currentUserProvider.GetUserAsync(HttpContext.RequestAborted);
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> SaveKey([FromBody]SaveKeyModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            var user = await _currentUserProvider.GetUserAsync();
            if (user == null)
                return BadRequest("User not found");

            user.PublicKey = model.PublicKey;
            user.PrivateKey = model.PrivateKey;
            await _userRepository.SaveAsync(user);

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GenerateToken([FromBody]GenerateTokenModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepository.LoadByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Email or password is invalid");

            var result = _passwordHasher.VerifyHashedPassword(user.Password, model.Password);
            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Email or password is invalid");

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password = _passwordHasher.HashPassword(model.Password);
                await _userRepository.SaveAsync(user);
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, model.Email),
                new Claim(JwtRegisteredClaimNames.Sub, model.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var jwtAuthentication = _jwtAuthentication.Value;
            var token = new JwtSecurityToken(
                issuer: jwtAuthentication.ValidIssuer,
                audience: jwtAuthentication.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                notBefore: DateTime.UtcNow,
                signingCredentials: jwtAuthentication.SigningCredentials);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userRepository.LoadByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("User already exists");

            var user = new User();
            user.Email = model.Email;
            user.Password = _passwordHasher.HashPassword(model.Password);
            await _userRepository.SaveAsync(user);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicKeys([FromQuery]string[] emails)
        {
            var result = new List<PublicKeyModel>();
            foreach (var email in emails)
            {
                var user = await _userRepository.LoadByEmailAsync(email);
                if (user?.PublicKey != null)
                {
                    result.Add(new PublicKeyModel { Email = user.Email, PublicKey = user.PublicKey });
                }
            }

            return Ok(result);
        }

        public class PublicKeyModel
        {
            public string Email { get; set; }
            public JObject PublicKey { get; set; }
        }

        public class GenerateTokenModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            public string Password { get; set; }
        }

        public class RegisterModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            public string Password { get; set; }
        }

        public class SaveKeyModel
        {
            [Required]
            public JObject PublicKey { get; set; }
            [Required]
            public JObject PrivateKey { get; set; }
        }
    }
}
