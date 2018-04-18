using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public class PasswordManagerDatabase : Database
    {
        public PasswordManagerDatabase(string path)
            : base(path)
        {
        }

        [JsonProperty(Order = 1000)]
        public IList<User> Users { get; } = new List<User>();

        [JsonProperty(Order = 2000)]
        public IList<Document> Documents { get; } = new List<Document>();

        public User FindUser(string email)
        {
            return Users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
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
