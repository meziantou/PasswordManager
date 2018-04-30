using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public abstract class Database : IDisposable
    {
        [JsonIgnore]
        private readonly string _path;
        [JsonIgnore]
        private string _clonedData;
        [JsonIgnore]
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        protected Database(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        protected virtual JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings();
            //settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            settings.DefaultValueHandling = DefaultValueHandling.Include;
#if DEBUG
            settings.Formatting = Formatting.Indented;
#endif

            return settings;
        }

        private void CloneData()
        {
            _clonedData = JsonConvert.SerializeObject(this, CreateSettings());
        }

        protected virtual void LoadJson(string json)
        {
            JsonConvert.PopulateObject(json, this, CreateSettings());
        }

        private void RestoreCloneData()
        {
            LoadJson(_clonedData);
            _clonedData = null;
        }

        public virtual void Save()
        {
            var json = JsonConvert.SerializeObject(this, CreateSettings());
            var directory = Path.GetDirectoryName(_path);
            var tempPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(_path) + DateTime.UtcNow.ToString("yyyyMMddhhmmssfff") + Path.GetExtension(_path));
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, _path, true);
            File.Delete(tempPath);
        }

        public virtual void Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                LoadJson(json);
            }
        }

        public ITransaction BeginReadTransaction()
        {
            _lock.EnterReadLock();
            try
            {
                return new ReadTransaction(this);
            }
            catch
            {
                _lock.ExitReadLock();
                throw;
            }
        }

        public ITransaction BeginWriteTransaction()
        {
            _lock.EnterWriteLock();
            try
            {
                CloneData();
                return new WriteTransaction(this);
            }
            catch
            {
                _lock.ExitWriteLock();
                throw;
            }
        }

        public void Dispose()
        {
            _lock?.Dispose();
            _lock = null;
        }

        private class WriteTransaction : ITransaction
        {
            public Database Database { get; }
            public bool Validated { get; private set; }

            public WriteTransaction(Database database)
            {
                Database = database ?? throw new ArgumentNullException(nameof(database));
            }

            public void Commit()
            {
                if (Validated)
                    return;

                try
                {
                    Database.Save();
                }
                finally
                {
                    Database._lock.ExitWriteLock();
                    Validated = true;
                }
            }

            private void Rollback()
            {
                if (Validated)
                    return;

                try
                {
                    Database.RestoreCloneData();
                }
                finally
                {
                    Database._lock.ExitWriteLock();
                    Validated = true;
                }
            }

            public void Dispose()
            {
                Rollback();
            }
        }

        private class ReadTransaction : ITransaction
        {
            public Database Database { get; }
            public bool Validated { get; private set; }

            public ReadTransaction(Database database)
            {
                Database = database ?? throw new ArgumentNullException(nameof(database));
            }

            public void Commit()
            {
                if (Validated)
                    return;

                try
                {
                }
                finally
                {
                    Database._lock.ExitReadLock();
                    Validated = true;
                }
            }

            public void Dispose()
            {
                Commit();
            }
        }
    }
}
