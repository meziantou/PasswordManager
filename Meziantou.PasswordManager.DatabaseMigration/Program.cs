using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meziantou.PasswordManager.Api.Data;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.DatabaseMigration
{
    class Program
    {
        static void Main()
        {
            var options = new JsonSerializerSettings();
            options.MissingMemberHandling = MissingMemberHandling.Error;
            var path = @"C:\Users\meziantou\Desktop\New folder\";
            var dbUsers = JsonConvert.DeserializeObject<List<User>>(GetContent(path + "users.json"), options);
            var dbDocuments = JsonConvert.DeserializeObject<List<DbDocument>>(GetContent(path + "documents.json"), options);
            var dbDocumentAccesses = JsonConvert.DeserializeObject<List<DbDocumentAccess>>(GetContent(path + "documentaccesses.json"), options);
            var dbFieldKeys = JsonConvert.DeserializeObject<List<DbFieldKey>>(GetContent(path + "fieldkeys.json"), options);
            var dbFields = JsonConvert.DeserializeObject<List<DbField>>(GetContent(path + "fields.json"), options);

            var db = new PasswordManagerDatabase();
            foreach (var user in dbUsers)
            {
                db.Users.Add(user);
            }

            foreach (var dbDocument in dbDocuments)
            {
                var doc = new Document();
                doc.Id = dbDocument.Id;
                doc.DisplayName = dbDocument.DisplayName;
                doc.CreatedOn = dbDocument.CreatedOn;
                doc.LastUpdatedOn = dbDocument.LastUpdatedOn;
                doc.Tags = dbDocument.Tags;
                doc.User = db.FindUser(dbDocument.UserId);
                db.Documents.Add(doc);

                foreach (var dbAccess in dbDocumentAccesses.Where(_ => _.DocumentId == dbDocument.Id))
                {
                    var access = new DocumentAccess();
                    access.CreatedOn = dbAccess.CreatedOn;
                    access.DisplayName = dbAccess.DisplayName;
                    access.LastUpdatedOn = dbAccess.LastUpdatedOn;
                    access.Tags = dbAccess.Tags;
                    access.User = db.FindUser(dbAccess.UserId);
                    doc.Accesses.Add(access);
                }

                foreach (var dbField in dbFields.Where(_ => _.DocumentId == dbDocument.Id))
                {
                    var field = new Field();
                    field.CreatedOn = dbField.CreatedOn;
                    field.Id = dbField.Id;
                    field.LastUpdatedOn = dbField.LastUpdatedOn;
                    field.Name = dbField.Name;
                    field.Options = dbField.Options;
                    field.SortOrder = dbField.SortOrder;
                    field.Type = dbField.Type;
                    field.Value = dbField.Value;
                    doc.Fields.Add(field);


                    foreach (var dbFieldKey in dbFieldKeys.Where(_ => _.FieldId == dbField.Id))
                    {
                        var fieldKey = new FieldKey();
                        fieldKey.CreatedOn = dbFieldKey.CreatedOn;
                        fieldKey.Key = dbFieldKey.Key;
                        fieldKey.LastUpdatedOn = dbFieldKey.LastUpdatedOn;
                        fieldKey.User = db.FindUser(dbFieldKey.UserId);
                        field.Keys.Add(fieldKey);
                    }
                }
            }

            foreach (var user in db.Users.ToList())
            {
                if (user.Username.StartsWith("TestUser"))
                    db.Users.Remove(user);
            }

            foreach (var doc in db.Documents.ToList())
            {
                if (doc.User.Username.StartsWith("TestUser"))
                    db.Documents.Remove(doc);
            }

            var settings = new JsonSerializerSettings();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
            settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            var json = JsonConvert.SerializeObject(db, settings);
            File.WriteAllText(path + "Meziantou.PasswordManager.json", json, Encoding.UTF8);
        }

        static string GetContent(string path)
        {
            return string.Join("", File.ReadAllLines(path));
        }
    }
}
