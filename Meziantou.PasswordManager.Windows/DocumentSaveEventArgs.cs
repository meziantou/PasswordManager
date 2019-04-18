using System;
using Meziantou.PasswordManager.Client;

namespace Meziantou.PasswordManager.Windows
{
    public class DocumentSaveEventArgs : EventArgs
    {
        public DocumentSaveEventArgs(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            Document = document;
        }

        public Document Document { get; }
    }
}