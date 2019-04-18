using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Meziantou.PasswordManager.Windows.Utilities
{
    internal class HaveIBeenPwnedHttpClientHandler : HttpClientHandler
    {
        private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(1500);
        private readonly TimeSpan _safetyInterval = TimeSpan.FromMilliseconds(100);

        private DateTime? _lastRequest;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_lastRequest != null)
            {
                var elapsed = DateTime.UtcNow - _lastRequest.Value;
                if (elapsed < _interval)
                {
                    await Task.Delay(_interval - elapsed + _safetyInterval).ConfigureAwait(false);
                }
            }

            for (var i = 0; i < 5; i++)
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode != (System.Net.HttpStatusCode)429)
                {
                    _lastRequest = DateTime.UtcNow;
                    return response;
                }

                if (response.Headers.RetryAfter != null)
                {
                    var delta = response.Headers.RetryAfter.Delta;
                    if (delta != null)
                    {
                        await Task.Delay(delta.Value + _safetyInterval).ConfigureAwait(false);
                    }
                    else
                    {
                        await Task.Delay(_interval + _safetyInterval).ConfigureAwait(false);
                    }
                }
            }

            return null;
        }
    }
}
