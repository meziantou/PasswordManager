using Meziantou.Framework.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Meziantou.PasswordManager.Client
{
    public class PasswordManagerUri
    {
        public const string UriScheme = "meziantoupasswordmanager";
        public const string FriendlyName = "PasswordManager Protocol";

        private Uri _uri;

        private PasswordManagerUri(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            _uri = uri;
        }

        public bool IsSearch => _uri.Segments.Length > 0 && _uri.Segments[0].EqualsIgnoreCase("search");

        public string GetSearchUrl()
        {
            var parameters = ParseNullableQuery(_uri.Query);
            if (parameters.TryGetValue("url", out var url))
                return url;

            return null;
        }

        public bool CopyToClipboard(bool defaultValue)
        {
            var parameters = ParseNullableQuery(_uri.Query);
            if (parameters.TryGetValue("copytoclipboard", out var value))
                return ConvertUtilities.ChangeType(value, defaultValue);

            return defaultValue;
        }

        internal static Dictionary<string, string> ParseNullableQuery(string queryString)
        {
            var accumulator = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(queryString) || queryString == "?")
            {
                return null;
            }

            var scanIndex = 0;
            if (queryString[0] == '?')
            {
                scanIndex = 1;
            }

            var textLength = queryString.Length;
            var equalIndex = queryString.IndexOf('=');
            if (equalIndex == -1)
            {
                equalIndex = textLength;
            }
            while (scanIndex < textLength)
            {
                var delimiterIndex = queryString.IndexOf('&', scanIndex);
                if (delimiterIndex == -1)
                {
                    delimiterIndex = textLength;
                }
                if (equalIndex < delimiterIndex)
                {
                    while (scanIndex != equalIndex && char.IsWhiteSpace(queryString[scanIndex]))
                    {
                        ++scanIndex;
                    }
                    var name = queryString.Substring(scanIndex, equalIndex - scanIndex);
                    var value = queryString.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1);
                    accumulator[Uri.UnescapeDataString(name.Replace('+', ' '))] = Uri.UnescapeDataString(value.Replace('+', ' '));
                    equalIndex = queryString.IndexOf('=', delimiterIndex);
                    if (equalIndex == -1)
                    {
                        equalIndex = textLength;
                    }
                }
                else
                {
                    if (delimiterIndex > scanIndex)
                    {
                        accumulator[queryString.Substring(scanIndex, delimiterIndex - scanIndex)] = string.Empty;
                    }
                }
                scanIndex = delimiterIndex + 1;
            }

            return accumulator;
        }

        public static PasswordManagerUri Parse(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;

            if (!uri.Scheme.EqualsIgnoreCase(UriScheme))
                return null;

            return new PasswordManagerUri(uri);
        }

        public static void RegisterUriScheme()
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
            {
                CreateAssociation(key);
            }
        }

        public static void RegisterUriSchemeForMachine()
        {
            using (var key = Registry.ClassesRoot.CreateSubKey(UriScheme))
            {
                CreateAssociation(key);
            }
        }

        private static void CreateAssociation(RegistryKey key, string executableLocation = null)
        {
            if (executableLocation == null)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                    throw new ArgumentException("location must be provided.");

                executableLocation = entryAssembly.Location;
            }

            var applicationLocation = executableLocation;

            key.SetValue("", "URL:" + FriendlyName);
            key.SetValue("URL Protocol", "");

            using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
            {
                defaultIcon.SetValue("", applicationLocation + ",1");
            }

            using (var commandKey = key.CreateSubKey(@"shell\open\command"))
            {
                commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
            }
        }
    }
}
