import { Router, RouteData } from "./router";
import { ViewSwitcher } from "./ui/view-switcher";
import { HttpClient, UserService, DocumentService } from './models/services';

bootstrap();

async function bootstrap() {
    const viewRootElement = document.getElementById("view-switcher");
    if (!viewRootElement) {
        throw new Error("Root element not found");
    }

    const viewSwitcher = new ViewSwitcher(viewRootElement);
    const router = new Router();
    const httpClient = new HttpClient();
    const userService = new UserService(httpClient);
    const documentService = new DocumentService(httpClient);

    router.addRoute("/login", (data) => {
        viewSwitcher.setView(async () => {
            const module = await import("./views/login");
            return new module.LoginView(
                userService,
                router);
        });
    });

    router.addRoute("/register", (data) => {
        viewSwitcher.setView(async () => {
            const module = await import("./views/register");
            return new module.UserRegisterForm(
                userService,
                router);
        });
    });

    router.addRoute("/user/generate-key", (data) => {
        viewSwitcher.setView(async () => {
            const module = await import("./views/user-generate-key");

            return new module.UserGenerateKeyForm(
                router,
                userService);
        });
    });

    router.addRoute("/documents", (data) => {
        viewSwitcher.setView(async () => {
            const module = await import("./views/document-list");

            return new module.DocumentList(
                userService,
                documentService,
                router);
        });
    });

    router.addRoute("/documents/create", (data) => {
        viewSwitcher.setView(async () => {
            const module = await import("./views/document-create");

            return new module.DocumentCreate(
                documentService,
                userService,
                router,
                data);
        });
    });

    router.addRoute("/documents/edit/{id}", (data) => {
        viewSwitcher.setView(async () => {
            const module = await import("./views/document-create");

            return new module.DocumentCreate(
                documentService,
                userService,
                router,
                data);
        });
    });

    router.setDefaultRoute((data) => document.body.appendChild(document.createTextNode("default")));
    router.start();
}