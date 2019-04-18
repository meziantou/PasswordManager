using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Client
{
    public class HttpServer : IDisposable
    {
        private HttpListener _listener;
        private Thread _thread;

        public void Start()
        {
            if (_listener == null)
            {
                var listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:47532/");
                _listener = listener;
            }

            _listener.Start();

            if (_thread == null || !_thread.IsAlive)
            {
                var thread = new Thread(o =>
                {
                    while (true)
                    {
                        try
                        {
                            var ctx = _listener.GetContext();
                            HandleRequest(ctx);
                        }
                        catch (HttpListenerException ex)
                        {
                            if (ex.ErrorCode == 995)
                                return;
                        }
                    }
                });

                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                _thread = thread;
            }
        }

        public void Stop()
        {
            if (_thread != null)
            {
                if (_thread.IsAlive)
                {
                    _thread.Abort();
                }

                _thread = null;
            }

            _listener?.Stop();
        }

        public void Dispose()
        {
            Stop();
            _listener?.Close();
        }

        private void HandleRequest(HttpListenerContext context)
        {
            if (HandleCorsRequest(context))
                return;

            var passwordManagerContext = PasswordManagerContext.Current;
            var url = context.Request.Url;
            if (string.Equals(url.AbsolutePath, "/documents", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(url.AbsolutePath, "/getpassword", StringComparison.OrdinalIgnoreCase))
            {
                if (!passwordManagerContext.IsLoggedIn)
                {
                    Unauthorized(context);
                    return;
                }

                var parameters = PasswordManagerUri.ParseNullableQuery(url.Query);
                if (!parameters.TryGetValue("url", out var searchUrl))
                {
                    BadRequest(context, null);
                    return;
                }

                var options = DocumentOptions.None;
                if (parameters.TryGetValue("options", out var optionsValue) && !Enum.TryParse(optionsValue, out options))
                {
                    options = DocumentOptions.None;
                }

                try
                {
                    var documents = passwordManagerContext.User.Documents.Where(doc => doc.MatchSearchUrl(searchUrl)).Select(_ => new Document(_, options));
                    var json = JsonConvert.SerializeObject(documents);
                    Json(context, json);
                }
                catch (PasswordManagerException ex)
                {
                    BadRequest(context, JsonConvert.SerializeObject(new
                    {
                        ex.Code,
                        ex.Message
                    }));
                }
                catch (Exception ex)
                {
                    InternalServerError(context, JsonConvert.SerializeObject(new
                    {
                        ex.Message,
                        ex.StackTrace
                    }));
                }
            }
        }

        private void Json(HttpListenerContext context, string json)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            var bytes = Encoding.UTF8.GetBytes(json);
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.Close();
        }

        private void BadRequest(HttpListenerContext context, string json)
        {
            context.Response.StatusCode = 400;
            context.Response.StatusDescription = "Bad request";

            if (json != null)
            {
                context.Response.ContentType = "application/json";
                var bytes = Encoding.UTF8.GetBytes(json);
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            }

            context.Response.Close();
        }

        private void InternalServerError(HttpListenerContext context, string json)
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";

            if (json != null)
            {
                context.Response.ContentType = "application/json";
                var bytes = Encoding.UTF8.GetBytes(json);
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            }

            context.Response.Close();
        }

        private void Unauthorized(HttpListenerContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.StatusDescription = "Unauthorized";
            context.Response.Close();
        }

        private bool HandleCorsRequest(HttpListenerContext context)
        {
            var origin = context.Request.Headers.Get("origin");
            if (!string.IsNullOrEmpty(origin))
            {
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    context.Response.StatusCode = 200;
                    context.Response.AddHeader("Access-Control-Allow-Origin", origin);
                    context.Response.AddHeader("Access-Control-Allow-Methods", "GET");
                }
            }

            return false;
        }

        private class Document
        {
            public Document(Client.Document document, DocumentOptions options)
            {
                foreach (var field in document.Fields)
                {
                    Fields.Add(new Field(field, options));
                }
            }

            public IList<Field> Fields { get; set; } = new List<Field>();
        }

        private class Field
        {
            public Field(Client.Field field, DocumentOptions options)
            {
                Name = field.Name;
                Selector = field.Selector;
                Type = field.Type;
                if (!field.IsEncrypted || !options.HasFlag(DocumentOptions.DontIncludeEncryptedValue))
                {
                    Value = field.GetValueAsString();
                }
            }

            public string Name { get; set; }
            public string Selector { get; set; }
            public string Value { get; set; }
            public FieldValueType Type { get; set; }
        }

        [Flags]
        private enum DocumentOptions
        {
            None = 0x0,
            DontIncludeEncryptedValue = 0x1
        }
    }
}
