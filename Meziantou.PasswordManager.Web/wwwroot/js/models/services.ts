import { isNullOrUndefined, isNumber, parseInteger, isString } from "../utilities";
import { StorageCache, LocalCache } from "../caching/memory-cache";
import { required, dataType, DataType } from '../data-annotations';
import { usingMasterKey } from '../views/master-key';
import * as crypto from "../crypto";
import { Document, Field, User, EncryptedMasterKey, PublicKey, GenerateTokenResponse } from './model';

export class HttpClient {
    private token: string | null = null;

    constructor() {
        const token = sessionStorage.getItem("token");
        if (token) {
            this.token = token;
        }
    }

    public async login(email: string, password: string) {
        try {
            const response: GenerateTokenResponse = await this.postJson("/api/user/generatetoken", {
                email,
                password
            });

            this.token = response.token;
            sessionStorage.setItem("token", this.token);
            return true;
        } catch{
            return false;
        }
    }

    public isAuthenticated() {
        return !!this.token;
    }

    public async getJson(url: string) {
        var headers = new Headers();
        headers.set("Accept", "application/json");
        if (this.token) {
            headers.set("Authorization", "Bearer " + this.token);
        }

        const response = await fetch(url, {
            method: "GET",
            headers: headers,
            redirect: "error",
            credentials: "include"
        });

        if (response.ok) {
            return response.json();
        }

        if (response.status === 401) {
            window.location.href = "/#/login";
            return;
        }

        throw new Error(response.statusText);
    }

    public async postJson<T>(url: string, model: T) {
        var headers = new Headers({
            "Content-Type": "application/json",
            "Accept": "application/json"
        });

        if (this.token) {
            headers.append("Authorization", "Bearer " + this.token);
        }

        const response = await fetch(url, {
            body: JSON.stringify(model),
            method: "POST",
            headers: headers,
            redirect: "error",
            credentials: "include"
        });

        if (response.ok) {
            const text = await response.text();
            if (text) {
                return JSON.parse(text);
            }

            return null;
        }

        if (response.status === 401) {
            window.location.href = "/#/login";
            return;
        }

        throw new Error(response.statusText);
    }
}

export class UserService {
    constructor(private httpClient: HttpClient) {
    }

    async me(): Promise<User> {
        return this.httpClient.getJson("/api/user/me") as Promise<User>;
    }

    save(user: User): Promise<User> {
        return this.httpClient.postJson("/api/user/save", user) as Promise<User>;
    }

    saveKey(publicKey: JsonWebKey, privateKey: EncryptedMasterKey): Promise<User> {
        return this.httpClient.postJson("/api/user/saveKey", {
            publicKey,
            privateKey
        }) as Promise<User>;
    }

    register(email: string, password: string): Promise<void> {
        return this.httpClient.postJson("/api/user/register", {
            email,
            password
        });
    }

    login(email: string, password: string) {
        return this.httpClient.login(email, password);
    }

    isAuthenticated(): boolean {
        return this.httpClient.isAuthenticated();
    }

    getPublicKeys(emails: string[]) {
        return this.httpClient.getJson("/api/user/getPublicKeys?" + emails.map(email => "emails=" + encodeURIComponent(email)).join("&")) as Promise<PublicKey[]>;
    }
}

export class DocumentService {
    constructor(private httpClient: HttpClient) {
    }

    get() {
        return this.httpClient.getJson("/api/document") as Promise<Document[]>;
    }

    getById(id: number) {
        return this.httpClient.getJson("/api/document/" + encodeURIComponent(id.toString())) as Promise<Document>;
    }

    save(document: Document) {
        return this.httpClient.postJson("/api/document", document) as Promise<Document>;
    }
}

export async function getFieldValue(field: Field, userService: UserService) {
    if (isString(field.value)) {
        return field.value;
    }

    if (field.encryptedValue) {
        const user = await userService.me();
        const privateKey = user.privateKey;
        if (!privateKey) {
            throw new Error("User has no private key");
        }

        const key = await usingMasterKey(masterKey => crypto.importPrivateKey(privateKey, masterKey));
        if (!key) {
            throw new Error("Private key cannot be decrypted");
        }

        return await crypto.decryptData(field.encryptedValue, key);
    }

    throw new Error("Field has no value");
}