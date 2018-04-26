import { UserService, DocumentService, getFieldValue } from "../models/services";
import { Router, RouteData } from '../router';
import { ViewComponent, InitializeResult } from '../ui/view-component';
import { appendChild } from '../ui/jsx';
import * as jsx from '../ui/jsx';
import * as crypto from '../crypto';
import { editable, getPropertyMetadata, PropertyDataAnnotations, DataType } from '../data-annotations';
import { FormComponent, ArrayEditor, openModalForm, IEditor, InitializeOptions } from '../ui/form-component';
import { parseInteger, nameof, isString } from '../utilities';
import { usingMasterKey } from './master-key';
import { FieldType, Field, PublicKey } from '../models/model';
import { userMustBeAuthenticatedAndConfigured } from './utilities';

export class DocumentCreate extends FormComponent<EditableDocument> {
    constructor(
        private documentService: DocumentService,
        private userService: UserService,
        private router: Router,
        private routeData: RouteData) {
        super();
    }

    public async initialize(options?: InitializeOptions) {
        const result = await userMustBeAuthenticatedAndConfigured(this.userService, this.router);
        if (result !== InitializeResult.Ok) {
            return result;
        }

        return super.initialize(options);
    }

    protected async loadDataCore(): Promise<EditableDocument> {
        const id = parseInteger(this.routeData.get("id"), -1);
        if (id !== -1) {
            const document = await this.documentService.getById(id);
            if (!document) {
                this.router.setUrl("/documents");
                return new EditableDocument(); // TODO remove mandatory return value
            } else {
                const result = new EditableDocument();
                result.id = document.id;
                result.displayName = document.displayName || "";
                result.fields = [];
                result.sharedWith = (document.sharedWith || []).join(", ")
                for (const f of document.fields) {
                    var editableField = new EditableField();
                    editableField.name = f.name;
                    editableField.type = f.type;
                    if (isString(f.value)) {
                        editableField.value = f.value;
                    } else {
                        try {
                            editableField.value = await getFieldValue(f, this.userService);
                        } catch (ex) {
                            console.warn(ex);
                            this.router.setUrl("/documents");
                            return new EditableDocument(); // TODO remove mandatory return value
                        }
                    }

                    result.fields.push(editableField);
                }

                return result;
            }
        }

        return new EditableDocument();
    }

    public createEditor(parentNode: Node, model: any, propertyKey: string) {
        if (propertyKey === nameof<EditableDocument>("fields")) {
            return new FieldCollectionEditor(this, parentNode, model, propertyKey);
        }

        if (model instanceof EditableField && propertyKey === nameof<EditableField>("value")) {
            return new FieldValueEditor(parentNode, model);
        }

        return super.createEditor(parentNode, model, propertyKey);
    }

    protected async onSubmit(model: EditableDocument): Promise<boolean> {
        let publicKeys: PublicKey[] = [];
        let publicKey: CryptoKey | null = null;
        if (model.fields.some(f => this.mustEncrypt(f.type))) {
            const user = await this.userService.me();
            if (!user.publicKey) {
                throw new Error("User has no public key");
            }

            publicKey = await crypto.importPublicKey(user.publicKey);

            if (model.sharedWith) {
                publicKeys = await this.userService.getPublicKeys(model.sharedWith.split(/[\s,;]+/));
            }
        }

        const fields: Field[] = [];
        for (const f of model.fields) {
            if (this.mustEncrypt(f.type)) {
                const value = await crypto.encryptData(f.value, publicKey!, publicKeys);

                fields.push({
                    name: f.name,
                    encryptedValue: value,
                    type: f.type,
                    lastUpdatedOn: new Date(),
                });
            } else {
                fields.push({
                    name: f.name,
                    value: f.value,
                    type: f.type,
                    lastUpdatedOn: new Date()
                });
            }
        }

        await this.documentService.save({
            id: model.id,
            displayName: model.displayName,
            fields: fields,
            sharedWith: model.sharedWith.split(/[\s,;]+/)
        });

        this.router.setUrl("/documents");
        return Promise.resolve(true);
    }

    private mustEncrypt(fieldType: FieldType) {
        return [FieldType.Password, FieldType.EncryptedNote].includes(fieldType);
    }
}

class EditableDocument {
    public id?: string;

    @editable
    public displayName: string = "";

    @editable
    public fields: EditableField[] = [];

    @editable
    public sharedWith: string = "";
}

class EditableField {
    public name: string = "";

    @editable
    public value: string = "";

    public type = FieldType.Text;
}

class NewField {
    public Type = FieldType.Text;

    @editable
    public name: string = "";
}

class FieldValueEditor implements IEditor {
    constructor(private parentNode: Node,
        private field: EditableField) {
    }

    render(): void | Promise<void> {
        appendChild(this.parentNode,
            <div>
                <label>{this.field.name}</label>
                {this.createEditor()}
            </div>);
    }

    private createEditor(): Node {
        let input: HTMLTextAreaElement | HTMLInputElement;
        if ([FieldType.Note, FieldType.EncryptedNote].includes(this.field.type)) {
            input = document.createElement("textarea");
            input.value = this.field.value;
        } else {
            input = document.createElement("input");
            switch (this.field.type) {
                case FieldType.Text:
                    input.type = "text";
                    break;

                case FieldType.Password:
                    input.type = "password";
                    break;
            }

            input.value = this.field.value;
        }

        input.addEventListener("change", e => {
            this.field.value = input.value;
        });

        input.addEventListener("blur", e => {
            this.field.value = input.value;
        });

        input.addEventListener("input", e => {
            this.field.value = input.value;
        });

        return input;
    }

    validate(): boolean {
        return true;
    }
}

class FieldCollectionEditor extends ArrayEditor {
    public async render() {
        await super.render();

        appendChild(this.parent, <div><a href="#" onclick={this.appendField(FieldType.Text)}>Add a new field</a></div>)
        appendChild(this.parent, <div><a href="#" onclick={this.appendField(FieldType.Password)}>Add a new Password field</a></div>)
        appendChild(this.parent, <div><a href="#" onclick={this.appendField(FieldType.Note)}>Add a new Note field</a></div>)
    }

    private appendField(type: FieldType) {
        return (async (e: MouseEvent) => {
            e.preventDefault();

            const result = await openModalForm(new NewField());
            if (result) {
                const field = new EditableField();
                field.name = result.name;
                field.type = type;
                (this.model as EditableDocument).fields.push(field);
                await this.createItem(field);
            }
        }).bind(this);
    }
}