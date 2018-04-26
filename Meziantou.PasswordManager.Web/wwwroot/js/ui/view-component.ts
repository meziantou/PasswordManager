import { removeChildren } from "../dom-utilities";

export abstract class ViewComponent {
    protected parentNode: Node | null = null;

    public initialize(): InitializeResult | Promise<InitializeResult> | RedirectResult | Promise<RedirectResult> {
        return InitializeResult.Ok;
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

export class RedirectResult {
    constructor(public readonly url: string) {
    }
}