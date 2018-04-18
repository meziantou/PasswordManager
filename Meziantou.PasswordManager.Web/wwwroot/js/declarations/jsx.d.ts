declare namespace JSX {
    export interface InstrinsicHTMLElement {
        className?: string;
        id?: string;
        style?: string;

        onclick?: HTMLElement["onclick"];
    }

    export interface InstrinsicHTMLAnchorElement extends InstrinsicHTMLElement {
        href?: string;
    }

    export interface InstrinsicHTMLLabelElement extends InstrinsicHTMLElement {
        for?: string;
    }

    export interface InstrinsicHTMLOListElement extends InstrinsicHTMLElement {
        reversed?: string;
        start?: string;
    }

    export interface InstrinsicHTMLFormElement extends InstrinsicHTMLElement {
        method?: "dialog" | "GET" | "POST";
    }

    export interface InstrinsicHTMLButtonElement extends InstrinsicHTMLElement {
        type?: "button" | "reset" | "submit";
    }

    export interface InstrinsicHTMLInputElement extends InstrinsicHTMLElement {
        type?: "text" | "password";
        readonly?: true;
        value?: string;
    }

    export interface IntrinsicElements {
        a: InstrinsicHTMLAnchorElement;
        button: InstrinsicHTMLButtonElement;
        dialog: InstrinsicHTMLElement;
        div: InstrinsicHTMLElement;
        form: InstrinsicHTMLFormElement;
        h1: InstrinsicHTMLElement;
        h2: InstrinsicHTMLElement;
        h3: InstrinsicHTMLElement;
        h4: InstrinsicHTMLElement;
        h5: InstrinsicHTMLElement;
        h6: InstrinsicHTMLElement;
        input: InstrinsicHTMLInputElement;
        label: InstrinsicHTMLLabelElement;
        li: InstrinsicHTMLElement;
        ol: InstrinsicHTMLOListElement;
        section: InstrinsicHTMLElement;
        span: InstrinsicHTMLElement;
        ul: InstrinsicHTMLElement;
    }

    export interface AttributeCollection {
        [name: string]: any;
    }
}