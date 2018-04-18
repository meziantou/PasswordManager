import { removeChildren } from "../dom-utilities";

export abstract class ViewComponent {
    protected parentNode: Node | null = null;

    public initialize(): Promise<void> | void | InitializeResult | Promise<InitializeResult> {
    }

    public render(parentNode: Node): Promise<void> {
        this.parentNode = parentNode;
        return this.renderCore(parentNode);
    }

    protected abstract renderCore(parentNode: Node): Promise<void>;

    public destroy(): void {
        if (this.parentNode) {
            removeChildren(this.parentNode);
            this.parentNode = null;
        }
    }
}

export const enum InitializeResult {
    Ok,
    StopProcessing
}