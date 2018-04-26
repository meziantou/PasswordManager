import { UserService } from "../models/services";
import { Router } from '../router';
import { InitializeResult } from '../ui/view-component';

export async function userMustBeAnonymous(userService: UserService, router: Router) {
    const user = await userService.me();
    if (user) {
        router.setUrl("/");
        return InitializeResult.StopProcessing;
    }

    return InitializeResult.Ok;
}

export async function userMustBeAuthenticated(userService: UserService, router: Router) {
    const user = await userService.me();
    if (!user) {
        router.setUrl("/login");
        return InitializeResult.StopProcessing;
    }

    return InitializeResult.Ok;
}

export async function userMustBeAuthenticatedAndConfigured(userService: UserService, router: Router) {
    const user = await userService.me();
    if (!user) {
        router.setUrl("/login");
        return InitializeResult.StopProcessing;
    }

    if (!user.publicKey) {
        router.setUrl("/user/generate-key");
        return InitializeResult.StopProcessing;
    }

    return InitializeResult.Ok;
}