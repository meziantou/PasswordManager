using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Meziantou.PasswordManager.Api.Data
{
    public class DocumentRepository
    {
        private readonly PasswordManagerDatabase _database;

        public DocumentRepository(PasswordManagerDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            _database = database;
        }

        public Task<Document> LoadByIdAsync(int id, CancellationToken ct = default(CancellationToken))
        {
            using (var tx = _database.BeginReadTransaction())
            {
                return Task.FromResult(_database.Documents.FirstOrDefault(document => document.Id == id));
            }
        }

        public Task<Document> LoadByIdAndUserAsync(int id, User user, CancellationToken ct = default(CancellationToken))
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            using (var tx = _database.BeginReadTransaction())
            {
                return Task.FromResult(_database.FindAccessibleDocumentByUser(user).FirstOrDefault(_ => _.Id == id));
            }
        }

        public Task<IEnumerable<Document>> LoadByUserAsync(User user, CancellationToken ct = default(CancellationToken))
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            using (var tx = _database.BeginReadTransaction())
            {
                return Task.FromResult(_database.FindAccessibleDocumentByUser(user));
            }
        }

        public Task DeleteAsync(Document document, CancellationToken ct = default(CancellationToken))
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            using (var tx = _database.BeginWriteTransaction())
            {
                _database.Documents.Remove(document);
                tx.Commit();

                return Task.CompletedTask;
            }
        }

        public Task SaveAsync(Document document, CancellationToken ct = default(CancellationToken))
        {
            using (var tx = _database.BeginWriteTransaction())
            {
                foreach(var field in document.Fields)
                {
                    if (field.IsEncrypted)
                    {
                        if (field.FindKey(document.User) == null)
                            throw new ArgumentException($"Field '{field.Name}' has no key for user '{document.User.Username}'.", nameof(document));

                        foreach (var access in document.Accesses)
                        {
                            if (field.FindKey(access.User) == null)
                                throw new ArgumentException($"Field '{field.Name}' has no key for user '{access.User.Username}'.", nameof(document));
                        }
                    }
                }

                var documents = _database.Documents;
                document.SetId(documents);
                document.UpdateTrackingProperties();

                foreach(var field in document.Fields)
                {
                    field.SetId(document.Fields);
                    field.UpdateTrackingProperties();

                    foreach(var fieldKey in field.Keys)
                    {
                        fieldKey.UpdateTrackingProperties();
                    }
                }

                foreach (var access in document.Accesses)
                {
                    access.UpdateTrackingProperties();
                }

                documents.AddOrReplace(_database.FindDocument(document.Id), document);
                tx.Commit();
                return Task.CompletedTask;
            }
        }
    }
}