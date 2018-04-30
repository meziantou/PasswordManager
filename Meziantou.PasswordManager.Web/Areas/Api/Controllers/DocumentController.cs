using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meziantou.PasswordManager.Web.Areas.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meziantou.PasswordManager.Web.Areas.Api.Controllers
{
    [Area(Constants.ApiArea)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DocumentController : Controller
    {
        private readonly CurrentUserProvider _currentUserProvider;
        private readonly UserRepository _userRepository;
        private readonly DocumentRepository _documentRepository;

        public DocumentController(CurrentUserProvider currentUserProvider, UserRepository userRepository, DocumentRepository documentRepository)
        {
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        }

        [HttpGet]
        [Route("api/document")]
        public async Task<IActionResult> Get()
        {
            var user = await _currentUserProvider.GetUserAsync(HttpContext.RequestAborted);
            var documents = await _documentRepository.LoadAccessibleByUserAsync(user, HttpContext.RequestAborted);
            return Ok(documents.Select(d => ToDocument(d, user)));
        }

        [HttpGet]
        [Route("api/document/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _currentUserProvider.GetUserAsync(HttpContext.RequestAborted);
            var document = await _documentRepository.LoadByIdAndUserAsync(id, user, HttpContext.RequestAborted);
            if (document == null)
                return NotFound();

            return Ok(ToDocument(document, user));
        }

        [HttpPost]
        [Route("api/document")]
        public async Task<IActionResult> Post([FromBody]ServiceModel.Document document)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _currentUserProvider.GetUserAsync(HttpContext.RequestAborted);
            var existingDocument = await _documentRepository.LoadByIdAndUserAsync(document.Id, user);

            var doc = new Document()
            {
                Id = existingDocument?.Id ?? default,
                User = existingDocument?.User ?? user,
                DisplayName = document.DisplayName,
                Tags = document.Tags,
                Fields = new List<Field>()
            };

            if (document.SharedWith != null)
            {
                doc.SharedWith = new List<UserRef>();
                foreach (var email in document.SharedWith)
                {
                    var u = await _userRepository.LoadByEmailAsync(email);
                    if (u != null)
                    {
                        doc.SharedWith.Add(u);
                    }
                }
            }

            foreach (var f in document.Fields?.Where(f => f.Value != null || f.EncryptedValue != null))
            {
                var field = new Field
                {
                    Name = f.Name,
                    Value = f.Value,
                    Type = f.Type
                };

                if (f.EncryptedValue != null)
                {
                    field.EncryptedValue = new EncryptedValue
                    {
                        Data = Convert.FromBase64String(f.EncryptedValue.Data),
                        Keys = { new EncryptedKey { User = user, Key = Convert.FromBase64String(f.EncryptedValue.Key) } }
                    };

                    if (f.EncryptedValue.AdditionalKeys != null)
                    {
                        foreach (var ak in f.EncryptedValue.AdditionalKeys)
                        {
                            var u = await _userRepository.LoadByEmailAsync(ak.Email);
                            if (u != null)
                            {
                                field.EncryptedValue.Keys.Add(new EncryptedKey { User = u, Key = Convert.FromBase64String(ak.Key) });
                            }
                        }
                    }
                }

                doc.Fields.Add(field);
            }

            await _documentRepository.SaveAsync(doc);
            return Ok(ToDocument(doc, user));
        }

        private ServiceModel.Document ToDocument(Document document, User user)
        {
            return new ServiceModel.Document
            {
                Id = document.Id,
                DisplayName = document.DisplayName,
                Tags = document.Tags,
                Fields = document.Fields?.Select(f => new ServiceModel.Field
                {
                    Name = f.Name,
                    Type = f.Type,
                    Value = f.Value,
                    EncryptedValue = f.EncryptedValue == null ? null : new ServiceModel.EncryptedValue
                    {
                        Data = Convert.ToBase64String(f.EncryptedValue.Data),
                        Key = Convert.ToBase64String(f.EncryptedValue.Keys.First(k => k.User == user).Key),
                    }
                }) ?? Enumerable.Empty<ServiceModel.Field>(),
                SharedBy = document.User == user ? null : document.User.Email,
                SharedWith = document.User == user ? document.SharedWith?.Select(s => s.Email) : null
            };
        }
    }
}
