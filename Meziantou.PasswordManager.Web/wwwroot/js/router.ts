import { registerEventHandler, DisposableEventHandler } from "./utilities";

export type RouteHandler = (data: RouteData) => void;

export class Router {
    private readonly events: DisposableEventHandler[] = [];
    private readonly routes: Route[] = [];
    private defaultRouteHandler: RouteHandler | null = null;

    public addRoute(template: string, handler: RouteHandler) {
        this.routes.push(new Route(template, handler));
    }

    public setDefaultRoute(handler: RouteHandler | null) {
        this.defaultRouteHandler = handler;
    }

    public start() {
        this.events.push(registerEventHandler(window, "hashchange", e => {
            console.log("hashchange");
            const url = this.getUrl(e.newURL);
            this.handleUrl(url);
        }));

        //this.events.push(registerEventHandler(window, "popstate", e => {
        //    console.log("popstate");
        //    const url = this.getCurrentUrl();
        //    this.handleUrl(url);
        //}));

        this.handleUrl(this.getCurrentUrl());
    }

    public destroy() {
        this.events.forEach(item => item.removeHandler());
        this.events.length = 0;
    }

    public setUrl(url: string) {
        console.log("setUrl " + url);
        history.pushState(null, "", "#" + url);
        this.handleUrl(this.getCurrentUrl());
    }

    private getCurrentUrl() {
        return this.getUrl(window.location.href);
    }

    private getUrl(str: string | null) {
        if (str) {
            const url = new URL(str);
            const hash = (url.hash || "#").substring(1);
            if (!hash.startsWith("/")) {
                return "/" + hash;
            }

            return hash;
        }

        return "/";
    }

    private handleUrl(url: string) {
        for (const route of this.routes) {
            if (route.match(url)) {
                console.log(`${url} is handled`);
                return;
            }
        }

        if (this.defaultRouteHandler) {
            const parsedUrl = new URL("http://dummy/" + url);
            this.defaultRouteHandler(new RouteData(new Map<string, string>(), parsedUrl.searchParams));
        }
    }
}

export class RouteData {
    constructor(public routeParameters: Map<string, string>, public queryParameters: URLSearchParams) {
    }

    public get(name: string) {
        if (this.routeParameters.has(name)) {
            return this.routeParameters.get(name);
        }

        if (this.queryParameters.has(name)) {
            return this.queryParameters.get(name);
        }

        return undefined;
    }
}

class Route {
    private readonly templateRegex: RegExp;
    private readonly routeParams: string[] = [];

    constructor(public readonly template: string, public readonly handler: RouteHandler) {
        const regex = Route.buildRegex(template);
        this.templateRegex = regex.regex;
        this.routeParams = regex.params;
    }

    public match(url: string): boolean {
        const result = this.templateRegex.exec(url);
        if (result) {
            const parsedUrl = new URL("http://dummy/" + url);
            const routeParams = new Map<string, string>();
            for (var i = 0; i < this.routeParams.length && result.length - 1; i++) {
                routeParams.set(this.routeParams[i], result[i + 1]);
            }

            this.handler(new RouteData(routeParams, parsedUrl.searchParams));
            return true;
        }

        return false;
    }

    private static buildRegex(template: string) {
        if (template.endsWith("/")) {
            template = template.substring(0, template.length - 1);
        }

        if (template === "") {
            template = "/";
        }

        let regexPattern = template;
        let paramNames: string[] = [];

        const paramRegex = /\{([a-z]+)\}/ig;
        let match: RegExpExecArray | null;
        while (match = paramRegex.exec(template)) {
            const paramName = match[1];
            if (paramNames.includes(paramName)) {
                throw new Error(`Duplicate route parameter '${paramName}' in the template '${template}'`);
            }

            paramNames.push(paramName);
            regexPattern = regexPattern.replace(match[0], "([^/]*)");
        }

        return {
            params: paramNames,
            regex: new RegExp("^" + regexPattern + "/?$", "i")
        };
    }
}