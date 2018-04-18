using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public class DocumentRepository
    {
        private readonly PasswordManagerDatabase _database;

        public DocumentRepository(PasswordManagerDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Task<Document> LoadByIdAndUserAsync(IntId id, User user, CancellationToken ct = default)
        {
            using (var tx = _database.BeginReadTransaction())
            {
                return Task.FromResult(_database.Documents.FirstOrDefault(d => d.User == user && d.Id == id));
            }
        }

        public Task<IEnumerable<Document>> LoadAccessibleByUserAsync(User user, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            using (var tx = _database.BeginReadTransaction())
            {
                return Task.FromResult<IEnumerable<Document>>(_database.Documents.Where(d => d.User == user || d.SharedWith?.Contains(user) == true).ToList());
            }
        }

        public Task SaveAsync(Document document, CancellationToken ct = default)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            using (var tx = _database.BeginWriteTransaction())
            {
                var documents = _database.Documents;

                document.SetId(documents);
                document.UpdateTrackingProperties();
                documents.AddOrReplace(document);
                tx.Commit();

                return Task.CompletedTask;
            }
        }
    }
}
