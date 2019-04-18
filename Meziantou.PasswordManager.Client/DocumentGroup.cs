using System;
using System.Collections.Generic;
using System.Linq;

namespace Meziantou.PasswordManager.Client
{
    public class DocumentGroup
    {
        public bool IsRoot { get; private set; }
        public string Name { get; set; }
        public IList<DocumentGroup> Groups { get; set; } = new List<DocumentGroup>();
        public IList<Document> Documents { get; set; } = new List<Document>();

        public static DocumentGroup Group(ICollection<Document> documents)
        {
            if (documents == null) throw new ArgumentNullException(nameof(documents));

            var root = new DocumentGroup();
            root.IsRoot = true;
            root.Groups.Add(CreateAllSecretsGroup(documents));
            root.Groups.Add(CreateMySecretGroup(documents));
            root.Groups.Add(CreateSecretsSharedWithMeGroup(documents));
            return root;
        }

        private static DocumentGroup CreateAllSecretsGroup(ICollection<Document> documents)
        {
            var group = new DocumentGroup();
            group.Name = "All secrets";
            CreateTreeviewItemsGroupByTags(group, documents.ToList());
            return group;
        }
        
        private static DocumentGroup CreateMySecretGroup(ICollection<Document> documents)
        {
            var group = new DocumentGroup();
            group.Name = "My secrets";
            CreateTreeviewItemsGroupByTags(group, documents.Where(doc => !doc.IsSharedBySomeone).ToList());
            return group;
        }

        private static DocumentGroup CreateSecretsSharedWithMeGroup(ICollection<Document> documents)
        {
            var group = new DocumentGroup();
            group.Name = "Shared with me";
            CreateTreeviewItemsGroupByUsers(group, documents.Where(doc => doc.IsSharedBySomeone).ToList());
            return group;
        }

        private static void CreateTreeviewItemsGroupByTags(DocumentGroup parent, ICollection<Document> documents)
        {
            var tags = documents.SelectMany(doc => doc.FinalTagList).Where(tag => tag != string.Empty).Distinct().OrderBy(_ => _).ToList();
            foreach (var tag in tags)
            {
                var groupItem = new DocumentGroup();
                groupItem.Name = tag;
                parent.Groups.Add(groupItem);

                var groupDocuments = documents.Where(doc => doc.FinalTagList.Contains(tag)).OrderBy(_ => _.FinalDisplayName);
                foreach (var groupDocument in groupDocuments)
                {
                    groupItem.Documents.Add(groupDocument);
                }
            }

            var noTagDocuments = documents.Where(doc => !doc.FinalTagList.Any() || doc.FinalTagList.Contains(string.Empty)).OrderBy(_ => _.FinalDisplayName);
            foreach (var groupDocument in noTagDocuments)
            {
                parent.Documents.Add(groupDocument);
            }
        }

        private static void CreateTreeviewItemsGroupByUsers(DocumentGroup parent, ICollection<Document> documents)
        {
            var users = documents.Select(doc => doc.SharedBy).Distinct().OrderBy(_ => _).ToList();
            foreach (var user in users)
            {
                var groupItem = new DocumentGroup();
                groupItem.Name = user;
                parent.Groups.Add(groupItem);

                var groupDocuments = documents.Where(doc => doc.SharedBy == user).OrderBy(_ => _.FinalDisplayName);
                foreach (var groupDocument in groupDocuments)
                {
                    groupItem.Documents.Add(groupDocument);
                }
            }
        }
    }
}
