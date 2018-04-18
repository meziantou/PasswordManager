using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Meziantou.PasswordManager.Web.Areas.Api.Data;
using Microsoft.AspNetCore.Http;

namespace Meziantou.PasswordManager.Web.Areas.Api
{
    public class CurrentUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserRepository _userRepository;

        public CurrentUserProvider(IHttpContextAccessor httpContextAccessor, UserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public Task<User> GetUserAsync(CancellationToken cancellationToken = default)
        {
            var claim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (claim == null)
                return Task.FromResult<User>(null);

            return _userRepository.LoadByEmailAsync(claim.Value, cancellationToken);
        }
    }
}
