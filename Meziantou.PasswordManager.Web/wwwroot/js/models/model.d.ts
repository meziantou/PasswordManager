export type Base64String = string;

export interface EncryptedMasterKey {
    version: number;
    salt: Base64String;
    iterationCount: number;
    iv: Base64String;
    jwk: Base64String;
}

export interface EncryptedValue {
    data: string;
    key: string;
    additionalKeys?: AdditionalKey[]
}

export interface AdditionalKey {
    email: string;
    key: string;
}

export interface User {
    email: string;
    publicKey?: JsonWebKey;
    privateKey?: EncryptedMasterKey;
}

export interface Document {
    id?: string;
    displayName?: string;
    fields: Field[];
    tags?: string;

    userDisplayName?: string;
    userTags?: string;

    sharedWith?: string[];
}

export interface Field {
    name: string;
    value?: string;
    encryptedValue?: EncryptedValue;
    type: FieldType;
    lastUpdatedOn: Date;
}

export const enum FieldType {
    Text,
    Note,
    Username,
    Password,
    Url,
    EmailAddress,
    EncryptedNote
}

export interface PublicKey {
    email: string;
    publicKey: JsonWebKey;
}

export interface GenerateTokenResponse {
    token: string;
}