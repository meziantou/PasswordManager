import { UserService, DocumentService, getFieldValue } from "../models/services";
import { Router } from '../router';
import { ViewComponent, InitializeResult } from '../ui/view-component';
import { appendChild } from '../ui/jsx';
import * as jsx from '../ui/jsx';
import { parseInteger, isNumber, isString } from '../utilities';
import { usingMasterKey } from './master-key';
import * as crypto from '../crypto';
import { Document, Field, FieldType } from '../models/model';

class DocumentView {
    public fields: FieldView[];

    constructor(private document: Document) {
        this.fields = document.fields.map(f => new FieldView(f));
    }

    public get id() {
        return this.document.id;
    }

    public get displayName() {
        return this.document.displayName;
    }
}

class FieldView {
    public showEncryptedValue: boolean = false;

    constructor(public field: Field) {
    }

    public get displayName() {
        return this.field.name;
    }

    public get displayValue() {
        return getDisplayValue(this.field);
    }

    public get isEncrypted() {
        return isEncrypted(this.field);
    }
}

export class DocumentList extends ViewComponent {
    private documents: DocumentView[] = [];

    constructor(private userService: UserService, private documentService: DocumentService, private router: Router) {
        super();
    }

    public async initialize() {
        const user = await this.userService.me();
        if (!user) {
            this.router.setUrl("/login");
            return InitializeResult.StopProcessing;
        }

        if (!user.publicKey) {
            this.router.setUrl("/user/generate-key");
            return InitializeResult.StopProcessing;
        }

        const documents = await this.documentService.get();
        this.documents = documents.map(d => new DocumentView(d));
        return InitializeResult.Ok;
    }

    protected renderCore(parentNode: Node): Promise<void> {
        appendChild(parentNode,
            <ul>
                {this.documents.map(doc =>
                    <li>
                        <span data-id={doc.id} onclick={this.showDetails.bind(this)}>{doc.displayName}</span>
                        <a href={`#/documents/edit/${doc.id}`}>(edit)</a>
                    </li>)}
            </ul>);
        return Promise.resolve();
    }

    private showDetails(e: MouseEvent) {
        if (!this.parentNode) {
            // TODO log
            return;
        }

        if (!(e.currentTarget instanceof HTMLElement)) {
            // TODO log
            return;
        }

        const id = e.currentTarget.dataset["id"];
        if (!id) {
            // TODO log
            return;
        }

        const doc = this.documents.find(d => d.id === id);
        if (!doc) {
            // TODO log
            return;
        }

        const dialog = document.createElement("dialog");
        dialog.addEventListener("close", e => {
            dialog.remove();
        }, { once: true });

        // close dialog on click on backdrop
        dialog.addEventListener("click", e => {
            const rect = dialog.getBoundingClientRect();
            const isInDialog = (rect.top <= e.clientY && e.clientY <= rect.top + rect.height
                && rect.left <= e.clientX && e.clientX <= rect.left + rect.width);
            if (!isInDialog) {
                dialog.close();
            }
        });

        appendChild(dialog,
            <div>
                <h1>{doc.displayName}</h1>
                {doc.fields && doc.fields.map(field =>
                    <div>
                        <span>{field.displayName}</span>
                        <input type="text" readonly value={field.displayValue} />
                        {field.isEncrypted && <button type="button" onclick={this.showValue(field)}>Show</button>}
                        <button type="button" onclick={this.copyToClipboard(field)}>Copy</button>
                    </div>)}
            </div>);

        this.parentNode.appendChild(dialog);
        dialog.showModal();
    }

    private copyToClipboard(field: FieldView) {
        const handler = async (e: MouseEvent) => {
            e.preventDefault();

            const value = await getFieldValue(field.field, this.userService);

            // https://developers.google.com/web/updates/2018/03/clipboardapi
            (navigator as any).clipboard.writeText(value)
                .catch(err => {
                    console.error('Could not copy text', err);
                });
        };

        return handler.bind(this);
    }

    private showValue(field: FieldView) {
        const handler = async (e: MouseEvent) => {
            e.preventDefault();

            const button = e.currentTarget;
            const input = getValueInput(e);
            if (!input) {
                console.error('Could not find input element for field ' + field.displayName);
                return;
            }

            if (field.showEncryptedValue) {
                input.value = field.displayValue;
                field.showEncryptedValue = false;
                setText(button, "show");
            } else {
                const value = await getFieldValue(field.field, this.userService);
                input.value = value;
                field.showEncryptedValue = true;
                setText(button, "hide");
            }
        };

        return handler.bind(this);
    }
}

function getDisplayValue(field: Field) {
    if (isString(field.value)) {
        return field.value;
    }

    return "********";
}

function getDisplayName(document: Document) {
    if (document.displayName) {
        return document.displayName;
    }

    const fields = document.fields.filter(f => !isEncrypted(f));
    const url = fields.find(f => f.type === FieldType.Url);
    if (url && isString(url.value)) {
        return url.value;
    }

    return "";
}

function isEncrypted(field: Field) {
    return !isString(field.value);
}

function getValueInput(e: MouseEvent) {
    var target = e.currentTarget;
    if (target instanceof HTMLElement) {
        if (target.parentElement) {
            return target.parentElement.querySelector("input");
        }
    }

    return null;
}

function setText(obj: any, text: string) {
    if (obj instanceof HTMLElement) {
        obj.textContent = text;
    }
}