import { UserService, DocumentService, getFieldValue } from "../models/services";
import { Router } from '../router';
import { ViewComponent, InitializeResult } from '../ui/view-component';
import { appendChild } from '../ui/jsx';
import * as jsx from '../ui/jsx';
import { parseInteger, isNumber, isString } from '../utilities';
import { usingMasterKey } from './master-key';
import * as crypto from '../crypto';
import { Document, Field, FieldType } from '../models/model';

export class DocumentList extends ViewComponent {
    private documents: Document[] = [];

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

        this.documents = await this.documentService.get();
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
                <h1>{getDisplayName(doc)}</h1>
                {doc.fields && doc.fields.map(field =>
                    <div>
                        <span>{field.name}</span>
                        <input type="text" readonly value={getDisplayValue(field)} />
                        <button type="button" onclick={this.copyToClipboard(field)}>Copy</button>
                    </div>)}
            </div>);

        this.parentNode.appendChild(dialog);
        dialog.showModal();
    }

    private copyToClipboard(field: Field) {
        const handler = async (e: MouseEvent) => {
            e.preventDefault();

            const value = await getFieldValue(field, this.userService);

            // https://developers.google.com/web/updates/2018/03/clipboardapi
            (navigator as any).clipboard.writeText(value)
                .catch(err => {
                    console.error('Could not copy text', err);
                });
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