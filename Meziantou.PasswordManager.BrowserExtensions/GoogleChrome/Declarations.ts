namespace Meziantou.PasswordManager {
    export declare type autocompleteType = "nickname" | "name" | "email";
    export declare type inputType = "button" | "checkbox" | "color" | "date" | "datetime-local" | "email"
        | "file" | "hidden" | "image" | "month" | "number" | "password" | "radio" | "range" | "reset"
        | "search" | "submit" | "tel" | "text" | "time" | "url" | "week";

    export interface Message {
        type: number;
        url: string;
    }

    export interface DocumentMessage extends Message {
        documents: Document[];
    }

    export const enum MessageType {
        Documents,
        NavigateLink
    }

    export interface Document {
        Fields: Field[];
    }

    export interface Field {
        Name: string;
        Value: string;
        Type: number;
        Selector: string | null;
    }

    export enum FieldValueType {
        String = 0,
        MultiLineString = 1,
        Username = 2,
        Password = 3,
        Url = 4,
        EmailAddress = 5
    }
}