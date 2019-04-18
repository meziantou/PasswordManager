using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meziantou.PasswordManager.Api.Data
{
    public class UserRepository
    {
        private readonly PasswordManagerDatabase _database;

        public UserRepository(PasswordManagerDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            _database = database;
        }

        public Task<User> LoadByUsernameAsync(string username, CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
                return null;

            using (var tx = _database.BeginReadTransaction())
            {
                return Task.FromResult(_database.FindUser(username));
            }
        }

        public Task SaveAsync(User user, CancellationToken ct = default(CancellationToken))
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            using (var tx = _database.BeginWriteTransaction())
            {
                var users = _database.Users;
                user.SetId(users);
                user.UpdateTrackingProperties();
                users.AddOrReplace(_database.FindUser(user.Username), user);
                tx.Commit();

                return Task.CompletedTask;
            }
        }
    }
}