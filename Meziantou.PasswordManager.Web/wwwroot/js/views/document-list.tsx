import { UserService, DocumentService, getFieldValue } from "../models/services";
import { Router } from '../router';
import { ViewComponent, InitializeResult } from '../ui/view-component';
import { appendChild } from '../ui/jsx';
import * as jsx from '../ui/jsx';
import { parseInteger, isNumber, isString, isArray, isNullOrEmpty } from '../utilities';
import { usingMasterKey } from './master-key';
import * as crypto from '../crypto';
import { Document, Field, FieldType } from '../models/model';
import { userMustBeAuthenticatedAndConfigured } from './utilities';
import * as clipboard from '../clipboard';
import { removeChildren } from '../ui/dom-utilities';

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

    public get tags() {
        if (isString(this.document.tags)) {
            return this.document.tags.split(",").map(s => s.trim()).filter(s => !isNullOrEmpty(s));
        }

        return [];
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

const untaggedGroupName = "";
export class DocumentList extends ViewComponent {
    private documents: DocumentView[] = [];
    private list: Node | null = null;

    constructor(private userService: UserService, private documentService: DocumentService, private router: Router) {
        super();
    }

    public async initialize() {
        const result = await userMustBeAuthenticatedAndConfigured(this.userService, this.router);
        if (result !== InitializeResult.Ok) {
            return result;
        }

        const documents = await this.documentService.get();
        this.documents = documents.map(d => new DocumentView(d));
        return InitializeResult.Ok;
    }

    protected renderCore(parentNode: Node): Promise<void> {
        appendChild(parentNode,
            <form onsubmit={this.onSearch.bind(this)}>
                <input type="search" name="query" />
                <button type="submit">Search</button>
            </form>);

        this.list = document.createElement("ul");
        parentNode.appendChild(this.list);

        const tags = this.getDocuments("");
        this.renderDocuments(this.list, tags);
        return Promise.resolve();
    }

    private onSearch(e: Event) {
        e.preventDefault();
        if (this.list && e.target instanceof HTMLFormElement) {
            const data = new FormData(e.target);
            const query = data.get("query");
            if (isString(query) && query.trim() !== "") {
                const documents = this.getDocuments(query);
                this.renderDocuments(this.list, documents);
            }
        }
    }

    private getDocuments(query: string) {
        const tags = new Map<string, DocumentView[]>();
        this.documents.forEach(d => {
            if (query && !filter(d)) {
                return;
            }

            if (d.tags.length > 0) {
                for (const tag of d.tags) {
                    const t = tags.get(tag);
                    if (t) {
                        t.push(d);
                    } else {
                        tags.set(tag, [d]);
                    }
                }
            } else {
                const t = tags.get(untaggedGroupName);
                if (t) {
                    t.push(d);
                } else {
                    tags.set(untaggedGroupName, [d]);
                }
            }
        });

        return tags;

        function filter(document: DocumentView) {
            const pattern = new RegExp(escapeRegex(query), "i");
            if (document.displayName && pattern.test(document.displayName)) {
                return true;
            }

            return document.fields.filter(f => !f.isEncrypted).some(f => pattern.test(f.displayValue));
        }

        function escapeRegex(pattern: string) {
            const specials = [
                // order matters for these
                "-", "[", "]"
                // order doesn't matter for any of these
                , "/", "{", "}", "(", ")", "*", "+", "?", ".", "\\", "^", "$", "|"
            ];

            // I choose to escape every character with '\'
            // even though only some strictly require it when inside of []
            const regex = RegExp('[' + specials.join('\\') + ']', 'g');

            return pattern.replace(regex, "\\$&");
        }
    }

    private renderDocuments(parentNode: Node, tags: Map<string, DocumentView[]>) {
        removeChildren(parentNode);

        for (const tag of Array.from(tags.keys()).sort()) {
            if (tag === untaggedGroupName) {
                continue;
            }

            appendChild(parentNode,
                <details>
                    <summary>{tag}</summary>
                    <ul>
                        {tags.get(tag)!.map(doc =>
                            <li>
                                {this.renderDocument(doc)}
                            </li>)}
                    </ul>
                </details>);
        }

        const untagged = tags.get(untaggedGroupName);
        if (untagged) {
            appendChild(parentNode,
                <ul>
                    {untagged.map(doc =>
                        <li>
                            {this.renderDocument(doc)}
                        </li>)}
                </ul>);
        }
    }

    private renderDocument(doc: DocumentView) {
        return (
            <div>
                <span data-id={doc.id} onclick={this.showDetails.bind(this)}>{doc.displayName}</span>
            </div>
        );
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

                <small>
                    <a href={`#/documents/edit/${doc.id}`}>(edit)</a>
                </small>
            </div>);

        this.parentNode.appendChild(dialog);
        dialog.showModal();
    }

    private copyToClipboard(field: FieldView) {
        const handler = async (e: MouseEvent) => {
            e.preventDefault();

            const value = await getFieldValue(field.field, this.userService);
            clipboard.set(value)
                .catch(() => console.error("Cannot set value to clipboard"));
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