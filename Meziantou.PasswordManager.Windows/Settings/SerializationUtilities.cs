using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Meziantou.PasswordManager.Windows.Settings
{
    /// <summary>
    ///     Helper to serialize object to or from an xml file.
    /// </summary>
    public static class SerializationUtilities
    {
        public static string GetConfigurationFilePath<T>()
        {
            return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), typeof(T).Namespace), typeof(T).Name + ".json");
        }

        public static void DeleteConfigurationFile<T>()
        {
            if (File.Exists(GetConfigurationFilePath<T>()))
            {
                File.Delete(GetConfigurationFilePath<T>());
            }
        }

        public static void OpenConfigurationFile<T>()
        {
            if (File.Exists(GetConfigurationFilePath<T>()))
            {
                Process.Start(GetConfigurationFilePath<T>());
            }
        }

        public static T DeserializeFromConfiguration<T>() where T : new()
        {
            var path = GetConfigurationFilePath<T>();
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    return JsonConvert.DeserializeObject<T>(json, CreateSettings());
                }
                catch
                {
                }
            }

            return new T();
        }

        public static void SerializeToConfiguration<T>(T obj)
        {
            var path = GetConfigurationFilePath<T>();
            PathCreateDirectory(path);
            var json = JsonConvert.SerializeObject(obj, CreateSettings());
            File.WriteAllText(path, json);
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return new JsonSerializerSettings()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        private static void PathCreateDirectory(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath))
                filePath = Path.GetFullPath(filePath);
            var directoryName = Path.GetDirectoryName(filePath);
            if (Directory.Exists(directoryName))
                return;
            Directory.CreateDirectory(directoryName);
        }
    }
}