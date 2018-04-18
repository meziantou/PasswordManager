interface Document {
    createElement(tagName: "dialog", options?: ElementCreationOptions): HTMLDialogElement;
}

interface Element {
    closest(selector: "dialog"): HTMLDialogElement | null;
}