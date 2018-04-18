using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public class UserRepository
    {
        private readonly PasswordManagerDatabase _database;

        public UserRepository(PasswordManagerDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Task<User> LoadByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(email))
                return Task.FromResult<User>(null);

            using (var tx = _database.BeginReadTransaction())
            {
                return Task.FromResult(_database.FindUser(email));
            }
        }

        public Task SaveAsync(User user, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            using (var tx = _database.BeginWriteTransaction())
            {
                var users = _database.Users;
                user.SetId(users);
                user.UpdateTrackingProperties();
                users.AddOrReplace(_database.FindUser(user.Email), user);
                tx.Commit();

                return Task.CompletedTask;
            }
        }
    }
}
