using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Api.Data
{

    public class PasswordManagerDatabase : Database
    {
        public PasswordManagerDatabase(string path, ILogger<PasswordManagerDatabase> logger)
            : base(path, logger)
        {
        }

        [JsonProperty(Order = 1000)]
        public IList<User> Users { get; } = new List<User>();

        [JsonProperty(Order = 2000)]
        public IList<Document> Documents { get; } = new List<Document>();

        public IEnumerable<Document> FindAccessibleDocumentByUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var owned = Documents.Where(_ => _.IsOwnedBy(user));
            var shared = Documents.Where(_ => _.IsSharedWith(user));
            return owned.Concat(shared);
        }

        public User FindUser(string username)
        {
            return Users.FirstOrDefault(user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));
        }

        public Document FindDocument(int? id)
        {
            return Documents.FirstOrDefault(document => document.Id == id);
        }

        protected override void LoadJson(string json)
        {
            Users.Clear();
            Documents.Clear();

            base.LoadJson(json);
        }
    }
}