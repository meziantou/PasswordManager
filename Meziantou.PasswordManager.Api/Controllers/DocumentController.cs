using System;
using System.Linq;
using System.Threading.Tasks;
using Meziantou.PasswordManager.Api.Data;
using Meziantou.PasswordManager.Api.ServiceModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meziantou.PasswordManager.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    public class DocumentController : Controller
    {
        private readonly PasswordManagerContext _context;
        private readonly DocumentRepository _documentRepository;
        private readonly UserRepository _userRepository;

        public DocumentController(PasswordManagerContext context, DocumentRepository documentRepository, UserRepository userRepository)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (userRepository == null) throw new ArgumentNullException(nameof(userRepository));
            if (documentRepository == null) throw new ArgumentNullException(nameof(documentRepository));

            _context = context;
            _userRepository = userRepository;
            _documentRepository = documentRepository;
        }

        [HttpGet]
        [Route("document/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _context.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            var document = await _documentRepository.LoadByIdAndUserAsync(id, user);
            if (document == null)
                return NotFound();

            return Ok(new ServiceModel.Document(document, user));
        }

        [HttpGet]
        [Route("document")]
        public async Task<IActionResult> Get()
        {
            var user = await _context.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            var documents = await _documentRepository.LoadByUserAsync(user);
            return Ok(documents.Select(doc => new ServiceModel.Document(doc, user)));
        }

        [HttpPost]
        [Route("document")]
        public async Task<IActionResult> Post([FromBody] ServiceModel.Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            Data.Document doc = null;
            if (int.TryParse(document.Id, out int id))
            {
                doc = await _documentRepository.LoadByIdAsync(id);
                if (doc != null)
                {
                    var access = doc.FindAccess(user);
                    if (access != null)
                    {
                        access.DisplayName = document.UserDisplayName;
                        access.Tags = document.UserTags;
                        await _documentRepository.SaveAsync(doc);
                        return Ok(new ServiceModel.Document(doc, user));
                    }

                    if (!doc.IsOwnedBy(user))
                        return BadRequest(new ErrorResponse(ErrorCode.Unauthorized, "Cannot edit this document"));
                }
            }

            if (doc == null)
            {
                doc = new Data.Document();
                doc.User = user;
            }

            doc.DisplayName = document.DisplayName;
            doc.Tags = document.Tags;

            doc.Fields.Clear();
            foreach (var field in document.Fields)
            {
                var f = new Data.Field();
                doc.Fields.Add(f);
                f.Name = field.Name;
                f.Options = field.Options;
                f.SortOrder = field.SortOrder;
                f.Type = field.Type;
                f.Value = field.Value;
                f.Selector = field.Selector;

                if (field.Key != null)
                {
                    var fk = f.FindKey(user);
                    if (fk == null)
                    {
                        fk = new Data.FieldKey();
                        fk.User = user;
                        f.Keys.Add(fk);
                    }

                    fk.Key = field.Key.Key;
                }

                if (field.Keys != null)
                {
                    foreach (var sharedFieldKey in field.Keys)
                    {
                        var u = await _userRepository.LoadByUsernameAsync(sharedFieldKey.Username);
                        if (u == null)
                            continue;

                        var fk = f.FindKey(u);
                        if (fk == null)
                        {
                            fk = new Data.FieldKey();
                            fk.User = u;
                            f.Keys.Add(fk);
                        }

                        fk.Key = sharedFieldKey.Key;
                    }
                }
            }

            await _documentRepository.SaveAsync(doc);
            return Ok(new ServiceModel.Document(doc, user));
        }

        [HttpDelete]
        [Route("document/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.GetCurrentUserAsync();
            var document = await _documentRepository.LoadByIdAsync(id);

            if (!document.IsOwnedBy(user))
                return Forbid();

            await _documentRepository.DeleteAsync(document);
            return Ok();
        }

        [HttpPost]
        [Route("document/{id}/share")]
        public async Task<IActionResult> Share(int id, [FromBody] ShareDocumentModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUser = await _context.GetCurrentUserAsync();
            var document = await _documentRepository.LoadByIdAsync(id);
            if (document == null)
                return NotFound(new ErrorResponse(ErrorCode.DocumentNotFound, "Document not found"));

            if (!document.IsOwnedBy(currentUser))
                return Forbid(); // Cannot share a document that we don't own

            var user = await _userRepository.LoadByUsernameAsync(model.Username);
            if (user == null)
                return NotFound(new ErrorResponse(ErrorCode.UserNotFound, "User not found"));

            // Add SharedDocument
            var access = document.FindAccess(user);
            if (access == null)
            {
                access = new DocumentAccess();
                access.User = user;
                document.Accesses.Add(access);
            }

            // Add FieldKeys
            if (model.Keys != null)
            {
                foreach (var fieldKey in model.Keys)
                {
                    if (!int.TryParse(fieldKey.FieldId, out int fieldId))
                        continue;

                    var field = document.FindField(fieldId);
                    if (field == null)
                        continue;

                    var fk = field.FindKey(user);
                    if (fk == null)
                    {
                        fk = new Data.FieldKey();
                        fk.User = user;
                        field.Keys.Add(fk);
                    }

                    fk.Key = fieldKey.Key;
                }
            }

            await _documentRepository.SaveAsync(document);
            return Ok(new ServiceModel.Document(document, currentUser));
        }
    }
}