using System;
using System.Threading.Tasks;
using Meziantou.PasswordManager.Api.Data;
using Meziantou.PasswordManager.Api.Security;
using Meziantou.PasswordManager.Api.ServiceModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meziantou.PasswordManager.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly PasswordManagerContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly UserRepository _userRepository;

        public UserController(PasswordManagerContext context, UserRepository userRepository, IPasswordHasher passwordHasher)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (userRepository == null) throw new ArgumentNullException(nameof(userRepository));
            if (passwordHasher == null) throw new ArgumentNullException(nameof(passwordHasher));

            _context = context;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("User/SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.LoadByUsernameAsync(model.Username);
                if (user != null)
                    return BadRequest(new ErrorResponse(ErrorCode.UserAlreadyExists, "This username is already taken"));

                user = new Data.User();
                user.Username = model.Username;
                user.Password = _passwordHasher.HashPassword(model.Password);
                user.Version = model.Version;

                await _userRepository.SaveAsync(user);
                return Json(new ServiceModel.User(user));
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("User/me/SetVersion")]
        public async Task<IActionResult> SetVersion([FromBody] SetVersionModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.GetCurrentUserAsync();
                user.Version = model.Version;

                await _userRepository.SaveAsync(user);
                return Json(new ServiceModel.User(user));
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("User/me/ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.GetCurrentUserAsync();
                var result = _passwordHasher.VerifyHashedPassword(user.Password, model.OldPassword);
                if (result == PasswordVerificationResult.Failed)
                    return BadRequest(new ErrorResponse(ErrorCode.InvalidPassword, "Password is invalid"));

                user.Password = _passwordHasher.HashPassword(model.NewPassword);

                await _userRepository.SaveAsync(user);
                return Json(new ServiceModel.User(user));
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("User/me")]
        public async Task<IActionResult> Me()
        {
            var user = await _context.GetCurrentUserAsync();
            return Json(new ServiceModel.User(user));
        }

        [HttpGet]
        [Route("User/{username}/PublicKey")]
        public async Task<IActionResult> GetPublicKey(string username)
        {
            var user = await _userRepository.LoadByUsernameAsync(username);
            if (user == null)
                return BadRequest(new ErrorResponse(ErrorCode.UserNotFound, "The user does not exist"));

            if (string.IsNullOrEmpty(user.PublicKey))
                return BadRequest(new ErrorResponse(ErrorCode.UserHasNotSetPublicKey, "The user has not set a public key"));

            return Json(user.PublicKey);
        }

        [HttpPost]
        [Route("User/me/SetKey")]
        public async Task<IActionResult> SetKey([FromBody] ChangeUserKeyModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.GetCurrentUserAsync();
                user.PublicKey = model.PublicKey;
                user.PrivateKey = model.PrivateKey;
                await _userRepository.SaveAsync(user);
                return Json(new ServiceModel.User(user));
            }

            return BadRequest(ModelState);
        }
    }
}