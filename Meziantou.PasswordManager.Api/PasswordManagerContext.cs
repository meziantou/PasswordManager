using System;
using System.Threading;
using System.Threading.Tasks;
using Meziantou.PasswordManager.Api.Data;
using Microsoft.AspNetCore.Http;

namespace Meziantou.PasswordManager.Api
{
    public class PasswordManagerContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserRepository _userRepository;

        public PasswordManagerContext(IHttpContextAccessor httpContextAccessor, UserRepository userRepository)
        {
            if (httpContextAccessor == null) throw new ArgumentNullException(nameof(httpContextAccessor));
            if (userRepository == null) throw new ArgumentNullException(nameof(userRepository));

            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public Task<User> GetCurrentUserAsync(CancellationToken ct = default(CancellationToken))
        {
            var user = _httpContextAccessor.HttpContext.User.Identity;
            if (user.IsAuthenticated)
            {
                return _userRepository.LoadByUsernameAsync(user.Name, ct);
            }

            return null;
        }
    }
}