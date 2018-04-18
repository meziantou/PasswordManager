import { removeChildren } from "../dom-utilities";
import { ViewComponent, InitializeResult } from "./view-component";
import { isNumber } from '../utilities';

export class ViewSwitcher {
    private currentView: ViewComponent | null = null;
    private currentViewPromise: (() => Promise<ViewComponent>) | null = null;

    constructor(private rootNode: Node) {
    }

    public async setView(viewComponentPromise: () => Promise<ViewComponent>) {
        // TODO unload current component (maybe handle case where you cannot change screen)
        // TODO set loading screen
        if (this.currentView) {
            this.currentView.destroy();
            this.currentView = null;
        }

        this.currentViewPromise = viewComponentPromise;
        const viewComponent = await viewComponentPromise();
        this.currentView = viewComponent;

        // Ensure this is the last loading view
        if (this.currentViewPromise !== viewComponentPromise) {
            return;
        }

        const result = await viewComponent.initialize();
        if (isNumber(result)) {
            if (result === InitializeResult.StopProcessing) {
                viewComponent.destroy();
                return;
            }
        }

        // Ensure this is the last loading view
        if (this.currentViewPromise !== viewComponentPromise) {
            return;
        }

        removeChildren(this.rootNode);
        await viewComponent.render(this.rootNode);
        // TODO remove loading screen
    }
}