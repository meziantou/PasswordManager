using System;
using System.Threading.Tasks;
using Meziantou.PasswordManager.Client;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Meziantou.PasswordManager.Api;

namespace Meziantou.PasswordManager.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private class MasterKeyProvider : IMasterKeyProvider
        {
            private readonly string _key;
            public MasterKeyProvider(string key)
            {
                _key = key;
            }

            public string GetMasterKey(int attempt)
            {
                return _key;
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task Scenario1()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseEnvironment("Test")
                .UseStartup<Startup>();
            using (var server = new TestServer(webHostBuilder))
            {
                using (var httpClient = server.CreateClient())
                {
                    var guid = Guid.NewGuid();
                    const int keySize = 1024;
                    var masterKey = guid.ToString();
                    var username1 = "TestUser1 - " + guid;
                    const string password1 = "Pa$$w0rd1";

                    var username2 = "TestUser2 - " + guid;
                    const string password2 = "Pa$$w0rd2";

                    var context = Client.PasswordManagerContext.Current;
                    context.MasterKeyProvider = new MasterKeyProvider(masterKey);
                    context.MasterKeyStore = new TemporaryKeyStore();

                    context.Client = new PasswordManagerClient(httpClient);
                    var client = context.Client;
                    //client.ApiUrl = "https://api.passwordmanager.meziantou.net/";
                    //client.ApiUrl = "http://localhost:5000/";

                    // Generate 2 users and their keys
                    Console.WriteLine("Signup user1");
                    await client.SignUpAsync(username1, password1);
                    Console.WriteLine("Signup user2");
                    await client.SignUpAsync(username2, password2);

                    Console.WriteLine("Generate key user1");
                    client.SetCredential(username1, password1);
                    var user1 = await client.MeAsync();
                    user1.GenerateKey(masterKey, keySize);
                    await client.SetUserKeyAsync(user1);

                    Console.WriteLine("Generate key user2");
                    client.SetCredential(username2, password2);
                    var user2 = await client.MeAsync();
                    user2.GenerateKey(masterKey, keySize);
                    await client.SetUserKeyAsync(user2);

                    // Create document
                    Console.WriteLine("Create document");
                    client.SetCredential(username1, password1);
                    var document = user1.CreateNewDocument();
                    document.DisplayName = "Document1";
                    document.AddField("field1", "value1", FieldValueType.String, null);
                    document.AddEncryptedField("field2", "value2", FieldValueType.String, null, null);
                    document = await client.SaveDocumentAsync(document);

                    // User2 must not have access to the document
                    Console.WriteLine("Get document from user2");
                    client.SetCredential(username2, password2);
                    try
                    {
                        await client.LoadDocumentAsync(document.Id);
                        Assert.Fail();
                    }
                    catch
                    {
                    }

                    // Share document
                    Console.WriteLine("Share document");
                    client.SetCredential(username1, password1);
                    document = await client.ShareDocumentAsync(document, username2);

                    // Test user2 has access to the document
                    Console.WriteLine("Get document from user2");
                    client.SetCredential(username2, password2);
                    var user2Document = await client.LoadDocumentAsync(document.Id);
                    Assert.AreEqual("Document1", document.DisplayName);
                    Assert.AreEqual(null, document.Tags);
                    Assert.AreEqual("value1", user2Document.Fields.First(f => f.Name == "field1").GetValueAsString());
                    Assert.AreEqual("value2", user2Document.Fields.First(f => f.Name == "field2").GetValueAsString());

                    // Update document
                    Console.WriteLine("Update document from user1");
                    client.SetCredential(username1, password1);
                    var users = await client.GetUserPublicKeysAsync(document);
                    document.DisplayName = "DisplayNameUser1";
                    document.Tags = "TagsUser1";
                    document.Fields.Clear();
                    document.AddField("field1", "value1", FieldValueType.String, "[name=email]");
                    document.AddEncryptedField("field2", "value2", FieldValueType.String, null, users);
                    document.AddEncryptedField("field3", "value3", FieldValueType.String, null, users);
                    document = await client.SaveDocumentAsync(document);
                    Assert.AreEqual("DisplayNameUser1", document.DisplayName);
                    Assert.AreEqual("TagsUser1", document.Tags);
                    Assert.AreEqual("value1", document.Fields.First(f => f.Name == "field1").GetValueAsString());
                    Assert.AreEqual("[name=email]", document.Fields.First(f => f.Name == "field1").Selector);

                    // Test user2 has access to the document
                    Console.WriteLine("Get document from user2");
                    client.SetCredential(username2, password2);
                    user2Document = await client.LoadDocumentAsync(document.Id);
                    Assert.AreEqual("DisplayNameUser1", user2Document.DisplayName);
                    Assert.AreEqual("TagsUser1", user2Document.Tags);
                    Assert.AreEqual("value1", user2Document.Fields.First(f => f.Name == "field1").GetValueAsString());
                    Assert.AreEqual("value2", user2Document.Fields.First(f => f.Name == "field2").GetValueAsString());

                    // Change master key
                    Console.WriteLine("Change master key of user1");
                    client.SetCredential(username1, password1);
                    user1.GenerateKey(masterKey, keySize);
                    await client.SetUserKeyAsync(user1);

                    // Test user2 has access to the document
                    Console.WriteLine("Get document from user2");
                    client.SetCredential(username2, password2);
                    user2Document = await client.LoadDocumentAsync(document.Id);
                    Assert.AreEqual("value1", user2Document.Fields.First(f => f.Name == "field1").GetValueAsString());
                    Assert.AreEqual("value2", user2Document.Fields.First(f => f.Name == "field2").GetValueAsString());
                    Assert.AreEqual("value3", user2Document.Fields.First(f => f.Name == "field3").GetValueAsString());

                    // User 2 update the display name and tags
                    Console.WriteLine("Update document from user2");
                    user2Document.UserDisplayName = "DisplayNameUser2";
                    user2Document.UserTags = "TagsUser2";
                    user2Document = await client.SaveDocumentAsync(user2Document);
                    Assert.AreEqual("DisplayNameUser1", user2Document.DisplayName);
                    Assert.AreEqual("TagsUser1", user2Document.Tags);
                    Assert.AreEqual("DisplayNameUser2", user2Document.UserDisplayName);
                    Assert.AreEqual("TagsUser2", user2Document.UserTags);

                    Console.WriteLine("Get document from user1");
                    client.SetCredential(username1, password1);
                    document = await client.LoadDocumentAsync(document.Id);
                    Assert.AreEqual("DisplayNameUser1", document.DisplayName);
                    Assert.AreEqual("TagsUser1", document.Tags);
                    Assert.AreEqual(null, document.UserDisplayName);
                    Assert.AreEqual(null, document.UserTags);
                }
            }
        }
    }
}
