using System.Collections.ObjectModel;
using System.Linq;
using Meziantou.PasswordManager.Client;

namespace Meziantou.PasswordManager.Windows.ViewModel
{
    public class EditableDocument : AutoObject
    {
        public EditableDocument() : this(null)
        {
        }

        public EditableDocument(Document document)
        {
            Document = document;
            Fields = new ObservableCollection<EditableField>();

            if (document != null)
            {
                DisplayName = document.IsSharedBySomeone ? document.UserDisplayName : document.DisplayName;
                Tags = document.FinalTags;

                if (CanEditFields)
                {
                    foreach (var field in document.Fields.OrderBy(f => f.SortOrder))
                    {
                        var editableField = new EditableField();
                        editableField.Name = field.Name;
                        editableField.ValueType = field.Type;
                        editableField.Options = field.Options;
                        editableField.Selector = field.Selector;
                        editableField.ValueString = field.GetValueAsString();

                        Fields.Add(editableField);
                    }
                }
            }
        }

        public bool CanEditFields => Document == null || !Document.IsSharedBySomeone;

        public Document Document { get; }

        public string DisplayName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string Tags
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public ObservableCollection<EditableField> Fields
        {
            get => GetProperty<ObservableCollection<EditableField>>();
            set => SetProperty(value);
        }

        public static EditableDocument CreateDefaultWebDocument()
        {
            var document = new EditableDocument(null);
            document.Fields.Add(CreateField(CreateFieldType.Url));
            document.Fields.Add(CreateField(CreateFieldType.EmailAddress));
            document.Fields.Add(CreateField(CreateFieldType.Username));
            document.Fields.Add(CreateField(CreateFieldType.Password));
            document.Fields.Add(CreateField(CreateFieldType.MultiLineString));
            return document;
        }

        internal static EditableField CreateField(CreateFieldType type)
        {
            var field = new EditableField();
            switch (type)
            {
                case CreateFieldType.String:
                    field.ValueType = FieldValueType.String;
                    break;
                case CreateFieldType.MultiLineString:
                    field.ValueType = FieldValueType.MultiLineString;
                    break;
                case CreateFieldType.Username:
                    field.ValueType = FieldValueType.Username;
                    break;
                case CreateFieldType.Password:
                    field.ValueType = FieldValueType.Password;
                    field.Options |= FieldOptions.Encrypted;
                    break;
                case CreateFieldType.Url:
                    field.ValueType = FieldValueType.Url;
                    break;
                case CreateFieldType.EmailAddress:
                    field.ValueType = FieldValueType.EmailAddress;
                    break;
                case CreateFieldType.EncryptedMultiLineString:
                    field.ValueType = FieldValueType.MultiLineString;
                    field.Options |= FieldOptions.Encrypted;
                    break;
            }

            field.Name = DefaultNameAttribute.GetName(type);
            return field;
        }

        //public void AddMissingDefaultWebField()
        //{
        //    var doc = CreateDefaultWebDocument();
        //    int lastIndex = -1;
        //    foreach (var field in doc.Fields)
        //    {
        //        int index = Fields.IndexOf(f => f.Name == field.Name);
        //        if (index >= 0)
        //        {
        //            lastIndex = index;
        //            continue;
        //        }

        //        lastIndex += 1;
        //        Fields.Insert(lastIndex, field);
        //    }
        //}
    }
}