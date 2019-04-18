using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Api.Data
{
    public abstract class Database : IDisposable
    {
        [JsonIgnore]
        private readonly string _path;
        private readonly ILogger _logger;
        [JsonIgnore]
        private string _clonedData;
        [JsonIgnore]
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        protected Database(string path, ILogger logger)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            _path = path;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected virtual JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
            settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
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

            try
            {
                File.WriteAllText(tempPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, $"Cannot write file '${tempPath}'");
                throw;
            }

            try
            {
                File.Copy(tempPath, _path, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(2, ex, $"Cannot copy file '${tempPath}' -> '${_path}'");
                throw;
            }

            try
            {
                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(3, ex, $"Cannot delete file '${tempPath}'");
                throw;
            }
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
                if (database == null)
                    throw new ArgumentNullException(nameof(database));

                Database = database;
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
                if (database == null)
                    throw new ArgumentNullException(nameof(database));

                Database = database;
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