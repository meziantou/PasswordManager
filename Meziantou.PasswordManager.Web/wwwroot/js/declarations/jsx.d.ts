declare namespace JSX {
    export interface InstrinsicHTMLElement {
        className?: string;
        id?: string;
        style?: string;

        onclick?: HTMLElement["onclick"];
        onchange?: HTMLElement["onchange"];
        onblur?: HTMLElement["onblur"];
        oninput?: HTMLElement["oninput"];
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

    export interface InstrinsicHTMLDetailsElement extends InstrinsicHTMLElement {
        open?: boolean;
    }

    export interface InstrinsicHTMLFormElement extends InstrinsicHTMLElement {
        method?: "dialog" | "GET" | "POST";
        onsubmit?: HTMLElement["onsubmit"];
    }

    export interface InstrinsicHTMLButtonElement extends InstrinsicHTMLElement {
        type?: "button" | "reset" | "submit";
    }

    export interface InstrinsicHTMLInputElement extends InstrinsicHTMLElement {
        min?: string;
        max?: string;
        name?: string;
        readonly?: true;
        step?: string;
        type?: "checkbox" | "number" | "text" | "password" | "search";
        value?: string;
    }

    export interface IntrinsicElements {
        a: InstrinsicHTMLAnchorElement;
        button: InstrinsicHTMLButtonElement;
        details: InstrinsicHTMLDetailsElement;
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
        small: InstrinsicHTMLElement;
        summary: InstrinsicHTMLElement;
        ul: InstrinsicHTMLElement;
    }

    export interface AttributeCollection {
        [name: string]: any;
    }
}