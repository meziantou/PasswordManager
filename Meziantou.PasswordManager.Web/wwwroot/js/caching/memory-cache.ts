import { isNullOrUndefined, isArray } from "../utilities";

type Constructor<T> = { new(): T; };

export interface LocalCache {
    get<T>(key: string, constructor?: Constructor<T>): T | undefined;
    set<T>(key: string, value: T): void;
    remove(key: string): boolean;
    clear(): void;
}

export class StorageCache implements LocalCache {
    public static readonly instance = new StorageCache();

    private readonly storage = window.sessionStorage;
    private readonly storageKey = "cache";
    private cache = new Map<string, any>();

    constructor() {
        this.loadStorage();
    }

    get<T>(key: string, constructor?: Constructor<T>): T | undefined {
        if (!this.cache.has(key)) {
            return undefined;
        }

        const item = this.cache.get(key);
        return this.deserialize(constructor, item);
    }

    set<T>(key: string, value: T): void {
        this.cache.set(key, this.serialize(value));
        this.saveStorage();
    }

    remove(key: string): boolean {
        if (this.cache.delete(key)) {
            this.saveStorage();
            return true;
        }

        return false;
    }

    clear(): void {
        this.cache.clear();
        this.saveStorage();
    }

    private loadStorage() {
        this.cache.clear();

        const item = this.storage.getItem(this.storageKey);
        if (item) {
            const obj = JSON.parse(item);
            if (isArray(obj)) {
                this.cache = new Map<string, any>(obj);
            }
        }
    }

    private saveStorage() {
        const array = Array.from(this.cache.entries());
        const json = JSON.stringify(array);
        this.storage.setItem(this.storageKey, json);
    }

    private serialize<T>(value: T) {
        return JSON.stringify(value);
    }

    private deserialize<T>(constructor: Constructor<T> | undefined, value: string) {
        const data = JSON.parse(value);
        if (constructor) {
            return this.createInstance(constructor, data);
        }

        return data;
    }

    private createInstance<T>(constructor: Constructor<T>, data: Partial<T>) {
        const obj = new constructor();
        for (const key in Object.keys(data)) {
            obj[key] = data[key];
        }

        return obj;
    }
}