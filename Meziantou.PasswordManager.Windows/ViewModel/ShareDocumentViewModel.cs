using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Utilities;

namespace Meziantou.PasswordManager.Windows.ViewModel
{
    internal class ShareDocumentViewModel : ViewModelBase
    {
        public ShareDocumentViewModel()
        {
            CanShare = true;
            KnownUsernames = ExtractUsernames();
        }

        [Required]
        public string Username
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [Required]
        public Document Document
        {
            get => GetProperty<Document>();
            set => SetProperty(value);
        }

        public IList<string> KnownUsernames { get; }

        public string ErrorText
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public bool CanShare
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        private static IList<string> ExtractUsernames()
        {
            var usernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var documents = PasswordManagerContext.Current?.User?.Documents;
            if (documents != null)
            {
                foreach (var document in documents)
                {
                    if (document.SharedBy != null)
                    {
                        usernames.Add(document.SharedBy);
                    }

                    if (document.SharedWith != null)
                    {
                        usernames.AddRange(document.SharedWith);
                    }
                }
            }

            return usernames.OrderBy(username => username).ToList();
        }
    }
}