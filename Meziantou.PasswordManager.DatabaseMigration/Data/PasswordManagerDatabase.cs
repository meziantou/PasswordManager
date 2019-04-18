using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Api.Data
{

    public class PasswordManagerDatabase
    {
        [JsonProperty(Order = 1000)]
        public IList<User> Users { get; } = new List<User>();

        [JsonProperty(Order = 2000)]
        public IList<Document> Documents { get; } = new List<Document>();

        public User FindUser(int id)
        {
            return Users.First(u => u.Id == id);
        }
    }
}