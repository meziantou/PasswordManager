import { ViewComponent, InitializeResult } from "./view-component";
import { DataType, PropertyDataAnnotations, getObjectAnnotationsFromInstance, getPropertyAnnotationsFromInstance } from "../data-annotations";
import { isNullOrUndefined, isString, parseNumber, parseString, isUndefined, isArray, isObject, isBoolean } from "../utilities";
import { validateProperty, validate } from "../validation";
import * as jsx from "./jsx";
import { removeChildren } from "./dom-utilities";
import { appendChild } from "./jsx";

export async function openModalForm<T>(form: FormComponent<T> | T): Promise<T | null> {
    const dialog = document.createElement("dialog");

    if (!(form instanceof FormComponent)) {
        form = new DialogFormComponent(form);
    }

    await form.initialize({ formMethod: "dialog" });
    await form.render(dialog);

    appendChild(document.body, dialog);
    dialog.showModal();

    const f = form;
    return new Promise<T | null>((resolve, reject) => {
        dialog.addEventListener("close", e => {
            console.log("close: " + dialog.returnValue);
            f.destroy();
            dialog.remove();
            if (dialog.returnValue) {
                resolve(f.model);
            }

            resolve(null);
        }, {
                once: true
            });
    });
}

export abstract class FormComponent<T> extends ViewComponent {
    private _model: T | null = null;
    protected title: string | null = null;
    protected submitButtonText: string = "submit";
    protected options: InitializeOptions | null = null;

    private submitted = false;
    private editors: IEditor[] = [];

    public get model() {
        return this._model;
    }

    public set model(value: T | null) {
        this._model = value;
    }

    public async initialize(options?: InitializeOptions) {
        this.options = options || null;
        this.model = await this.loadDataCore();
        return InitializeResult.Ok;
    }

    protected abstract loadDataCore(): Promise<T> | T;

    protected async renderCore(parentNode: Node): Promise<void> {
        const form = document.createElement("form");
        form.noValidate = true;
        if (this.options && this.options.formMethod) {
            form.method = this.options.formMethod;
        }

        const annotations = getObjectAnnotationsFromInstance(this.model);
        if (isBoolean(annotations.autoComplete)) {
            form.autocomplete = annotations.autoComplete ? "on" : "off";
        }

        for (const propertyKey of annotations.properties) {
            const editor = this.createEditor(form, this.model, propertyKey);
            await editor.render();
            this.editors.push(editor);
        }

        const submit = document.createElement("button");
        submit.type = "submit";
        submit.textContent = this.submitButtonText;
        form.appendChild(submit);

        parentNode.appendChild(form);

        form.addEventListener("submit", async e => {
            e.preventDefault();
            const model = this.model;
            if (!model) {
                throw new Error("Cannot submit form: model is not set.");
            }

            submit.disabled = true;

            try {
                const isValid = this.editors.reduce((currentValue, editor) => editor.validate() && currentValue, true);
                if (!isValid) {
                    return;
                }

                const result = await this.onSubmit(model);
                if (result) {
                    if (form.method === "dialog") {
                        const dialog = form.closest("dialog");
                        if (dialog) {
                            dialog.close("true");
                        }
                    }
                }
            } catch (ex) {
                // TODO show error
                console.error(ex);
            } finally {
                submit.disabled = false;
            }
        });

        return Promise.resolve();
    }

    protected abstract onSubmit(model: T): Promise<boolean>;

    public createEditor(parentNode: Node, model: any, propertyKey: string): IEditor {
        if (isObject(model) && isArray(model[propertyKey])) {
            return new ArrayEditor(this, parentNode, model, propertyKey);
        }

        return new Editor(parentNode, model, propertyKey);
    }
}

class DialogFormComponent<T> extends FormComponent<T> {
    private value: T;
    constructor(value: T) {
        super();
        this.value = value;
    }

    protected loadDataCore(): T {
        return this.value;
    }

    protected onSubmit(model: T): Promise<boolean> {
        return Promise.resolve(true);
    }
}

export interface InitializeOptions {
    formMethod?: string;
}

/*
    Before submit => 
        Validate on blur
        If already validated => Validate on input
    On Submit => Validate
        Validate on input
 */
const enum ValidationOptions {
    ValidateExplicitly = 0,
    ValidateOnBlur = 1,
    ValidateOnInput = 2,
    ClearErrorsOnInput = 4,
    ValidateOnChange = 8,
}

type EditorElement = HTMLInputElement | HTMLSelectElement;

export interface IEditor {
    render(): Promise<void> | void;
    validate(): boolean;
}

export class Editor implements IEditor {
    private readonly propertyAnnotations: PropertyDataAnnotations;
    private readonly editorId: string;

    private elements: {
        editorElement: EditorElement,
        validationErrorsElement: Node
    } | null = null;

    private isValidated = false;
    private validationOptions: ValidationOptions = ValidationOptions.ValidateOnBlur | ValidationOptions.ClearErrorsOnInput;

    private static counter = 0;

    constructor(
        private readonly parent: Node,
        private readonly model: any,
        public readonly propertyKey: string) {
        this.editorId = `id-${this.propertyKey}-${++Editor.counter}`;
        this.propertyAnnotations = getPropertyAnnotationsFromInstance(model, propertyKey);
        if (isNullOrUndefined(model)) {
            this.model = {};
        }
    }

    public render() {
        const input = this.createInput();

        const dom =
            <div className="form-editor">
                <label for={this.editorId}>{this.getPropertyDisplayName()}</label>
                {input}
                <ul className="form-editor-validation"></ul>
            </div>;

        jsx.appendChild(this.parent, dom);
        const validation = dom.querySelector(".form-editor-validation")!;
        this.elements = {
            editorElement: input,
            validationErrorsElement: validation
        };
    }

    protected createInput() {
        if (this.propertyAnnotations.lookupValues) {
            const select = document.createElement("select");
            select.id = this.editorId;

            for (const value of this.propertyAnnotations.lookupValues) {
                const option = document.createElement("option");
                option.value = value.value;
                if (isString(value.text)) {
                    option.textContent = value.text;
                }

                select.add(option);
            }

            const modelValue = this.getModelValue();
            if (!isNullOrUndefined(modelValue)) {
                select.value = this.getModelValue();
            }

            this.addInputEvents(select, "value");
            return select;

        } else {
            const input = document.createElement("input");
            input.id = this.editorId;
            if (isBoolean(this.propertyAnnotations.autoComplete)) {
                input.autocomplete = this.propertyAnnotations.autoComplete ? "on" : "off";
            }

            let valuePropertyName: keyof HTMLInputElement;
            switch (this.propertyAnnotations.dataType) {
                case DataType.emailAddress:
                    input.type = "email";
                    valuePropertyName = "value";
                    break;

                case DataType.password:
                    input.type = "password";
                    valuePropertyName = "value";
                    break;

                case DataType.number:
                    input.type = "number";
                    valuePropertyName = "valueAsNumber";
                    break;

                case DataType.string:
                default:
                    input.type = "text";
                    valuePropertyName = "value";
                    break;
            }

            const modelValue = this.getModelValue();
            if (!isNullOrUndefined(modelValue)) {
                input.value = this.getModelValue();
            }

            this.addInputEvents(input, valuePropertyName);
            return input;
        }
    }

    private addInputEvents<T extends EditorElement>(input: T, getValueProperty: keyof T) {
        input.addEventListener("change", e => {
            this.setModelValue(input[getValueProperty]);

            if ((this.validationOptions & ValidationOptions.ValidateOnChange) === ValidationOptions.ValidateOnChange) {
                this.validate();
            }
        });

        input.addEventListener("blur", e => {
            if ((this.validationOptions & ValidationOptions.ValidateOnBlur) === ValidationOptions.ValidateOnBlur) {
                this.setModelValue(input[getValueProperty]);
                this.validate();
            }
        });

        input.addEventListener("input", e => {
            if ((this.validationOptions & ValidationOptions.ValidateOnInput) === ValidationOptions.ValidateOnInput) {
                this.setModelValue(input[getValueProperty]);
                this.validate();
                return;
            }

            if ((this.validationOptions & ValidationOptions.ClearErrorsOnInput) === ValidationOptions.ClearErrorsOnInput) {
                this.clearValidationErrors();
            }
        });
    }

    public validate() {
        if (!this.elements) {
            throw new Error("Must call create before validate");
        }

        // Change validation options
        this.isValidated = true;
        this.validationOptions |= ValidationOptions.ValidateOnInput;

        this.clearValidationErrors();

        const errors = validateProperty(this.model, this.propertyKey);
        if (errors.length > 0) {
            for (const error of errors) {
                appendChild(this.elements.validationErrorsElement, <li>{error}</li>)
            }

            return false;
        }

        return true;
    }

    private clearValidationErrors() {
        if (this.elements) {
            removeChildren(this.elements.validationErrorsElement);
        }
    }

    protected getModelValue() {
        return this.model[this.propertyKey];
    }

    protected setModelValue(value: any) {
        if (!isUndefined(this.propertyAnnotations.dataType)) {
            switch (this.propertyAnnotations.dataType) {
                case DataType.string:
                case DataType.emailAddress:
                case DataType.password:
                    value = parseString(value);
                    break;

                case DataType.number:
                    value = parseNumber(value, null);
                    break;
            }
        }
        this.model[this.propertyKey] = value;
    }

    private getPropertyDisplayName() {
        return this.propertyKey.split(/(?=[A-Z])/).map(upperCaseFirstLetter).join(" ");

        function upperCaseFirstLetter(value: string) {
            return value.length === 0 ? "" : (value[0].toUpperCase() + value.substring(1));
        }
    }
}

export class ArrayEditor implements IEditor {
    private readonly editors: IEditor[] = [];
    private elements: { itemsParent: HTMLElement } | null = null;

    constructor(
        private readonly formComponent: FormComponent<any>,
        protected readonly parent: Node,
        protected readonly model: any,
        public readonly propertyKey: string) {

        if (isNullOrUndefined(model)) {
            this.model = {};
        }
    }

    public async render() {
        const ul = document.createElement("ul");
        this.parent.appendChild(ul);
        this.elements = { itemsParent: ul };

        const items = this.getModelValue();
        if (isArray(items)) {
            for (let item of items) {
                await this.createItem(item);
            }
        }
    }

    public validate() {
        return this.editors.reduce((currentValue, editor) => editor.validate() && currentValue, true);
    }

    protected getModelValue() {
        return this.model[this.propertyKey];
    }

    protected async createItem(item: any) {
        if (!this.elements) {
            throw new Error("Parent item is not set");
        }

        const li = document.createElement("li");
        this.elements.itemsParent.appendChild(li);

        const annotations = getObjectAnnotationsFromInstance(item);
        for (const propertyKey of annotations.properties) {
            const propertyAnnotations = getPropertyAnnotationsFromInstance(item, propertyKey);

            const editor = this.formComponent.createEditor(li, item, propertyKey);
            await editor.render();
            this.editors.push(editor);
        }
    }
}