import { isNullOrUndefined, isArray } from './utilities';
import { AdditionalKey, EncryptedValue, PublicKey, EncryptedMasterKey, Base64String } from './models/model';

// https://github.com/diafygi/webcrypto-examples#rsa-oaep
// https://gist.github.com/chrisveness/43bcda93af9f646d083fad678071b90a

interface AesKey {
    iv: Base64String;
    key: JsonWebKey;
}

type BinaryDataType = ArrayBuffer | Int8Array | Int16Array | Int32Array | Uint8Array | Uint16Array | Uint32Array | Uint8ClampedArray | Float32Array | Float64Array | DataView;
type SaltType = BinaryDataType | null;

export function generateRsaKey() {
    return window.crypto.subtle.generateKey(
        {
            name: "RSA-OAEP",
            modulusLength: 4096,
            publicExponent: new Uint8Array([0x01, 0x00, 0x01]), // 65537 (https://www.chromium.org/blink/webcrypto When generating RSA keys the public exponent must be 3 or 65537)
            hash: { name: "SHA-256" }
        },
        true,
        ["encrypt", "decrypt"]
    );
}

export function exportPublicKey(key: CryptoKeyPair) {
    return window.crypto.subtle.exportKey("jwk", key.publicKey);
}

export async function exportPrivateKey(key: CryptoKeyPair, masterPassword: string): Promise<EncryptedMasterKey> {
    // https://crypto.stackexchange.com/a/26787/16630
    // IV should be 12 bytes long
    const ivLength = 12;

    // 1. Export key
    const keyData = await window.crypto.subtle.exportKey("jwk", key.privateKey);

    // 2. Derive the masterPassord using PBKDF2
    var derivedKey = await derivePassword(masterPassword);

    // 3. Export jwk using AES-GCM
    const iv = window.crypto.getRandomValues(new Uint8Array(ivLength));
    if (iv === null) {
        throw new Error("Cannot generate random values");
    }

    const encryptedData = await window.crypto.subtle.encrypt(
        {
            name: "AES-GCM",
            iv: iv,
            tagLength: 128,
        },
        derivedKey.derivedKey,
        getBytes(JSON.stringify(keyData))
    );

    const privateKey: EncryptedMasterKey = {
        version: 1,
        iterationCount: derivedKey.iterationCount,
        salt: toBase64(derivedKey.salt),
        iv: toBase64(iv),
        jwk: toBase64(new Uint8Array(encryptedData))
    };

    return privateKey;
}

export function importPublicKey(key: JsonWebKey) {
    return window.crypto.subtle.importKey(
        "jwk",
        key,
        {
            name: "RSA-OAEP",
            hash: { name: "SHA-256" },
        },
        false,
        ["encrypt"]
    );
}

export async function importPrivateKey(keyData: EncryptedMasterKey, masterPassword: string) {
    if (keyData.version !== 1) {
        throw new Error("Expect data in version 1");
    }

    const derivedKey = await derivePassword(masterPassword, fromBase64(keyData.salt), keyData.iterationCount);
    const decryptedValue = await window.crypto.subtle.decrypt(
        {
            name: "AES-GCM",
            iv: fromBase64(keyData.iv),
            tagLength: 128,
        },
        derivedKey.derivedKey,
        fromBase64(keyData.jwk)
    );

    const json = getString(decryptedValue);
    const jwk = JSON.parse(json);

    return await window.crypto.subtle.importKey(
        "jwk",
        jwk,
        {
            name: "RSA-OAEP",
            hash: { name: "SHA-256" },
        },
        false,
        ["decrypt"]
    );
}

export async function derivePassword(password: string, salt?: SaltType, iterations?: number) {
    iterations = iterations || 50000;
    salt = salt || window.crypto.getRandomValues(new Uint8Array(16));
    if (isNullOrUndefined(salt)) {
        throw new Error("Cannot generate a ranom value");
    }

    const key = await window.crypto.subtle.importKey(
        "raw",
        getBytes(password),
        "PBKDF2",
        false,
        ["deriveKey"]
    );

    const derivedKey = await window.crypto.subtle.deriveKey(
        {
            name: "PBKDF2",
            salt: salt,
            iterations: iterations,
            hash: "SHA-256"
        },
        key,
        { name: "AES-GCM", length: 256 },
        false,
        ["encrypt", "decrypt"]
    );

    return {
        derivedKey: derivedKey,
        salt: salt,
        iterationCount: 50000
    }
}

export async function encryptData(data: string, masterPublicKey: CryptoKey, publicKeys?: PublicKey[]): Promise<EncryptedValue> {
    // 1. Generate a AES key
    const aesKey = await window.crypto.subtle.generateKey(
        {
            name: "AES-GCM",
            length: 256,
        },
        true,
        ["encrypt", "decrypt"]
    );

    // 2. Encrypt the data    
    const iv = window.crypto.getRandomValues(new Uint8Array(12));
    if (isNullOrUndefined(iv)) {
        throw new Error("Cannot generate a random value");
    }

    const encryptedData = await window.crypto.subtle.encrypt(
        {
            name: "AES-GCM",
            iv: iv,
            tagLength: 128,
        },
        aesKey,
        getBytes(data)
    );

    // 3. Encrypt the generated AES key using the RSA public key
    const aesKeyData = await window.crypto.subtle.exportKey("jwk", aesKey);
    const dataToEncrypt: AesKey = {
        iv: toBase64(iv),
        key: aesKeyData
    };

    const encryptedAesKey = await window.crypto.subtle.encrypt(
        {
            name: "RSA-OAEP"
        },
        masterPublicKey,
        getBytes(JSON.stringify(dataToEncrypt))
    );

    const additionalKeys: AdditionalKey[] = [];
    if (isArray(publicKeys)) {
        for (const publicKey of publicKeys) {
            const key = await importPublicKey(publicKey.publicKey);

            const encryptedAesKey = await window.crypto.subtle.encrypt(
                {
                    name: "RSA-OAEP"
                },
                key,
                getBytes(JSON.stringify(dataToEncrypt))
            );

            additionalKeys.push({
                email: publicKey.email,
                key: toBase64(encryptedAesKey)
            });
        }
    }

    // 4. return encrypted data and encrypted key
    return {
        data: toBase64(encryptedData),
        key: toBase64(encryptedAesKey),
        additionalKeys: additionalKeys
    };
}

export async function decryptData(data: EncryptedValue, masterPrivateKey: CryptoKey) {
    const decryptedAesKey = await window.crypto.subtle.decrypt(
        {
            name: "RSA-OAEP"
        },
        masterPrivateKey,
        fromBase64(data.key)
    );

    const json = getString(decryptedAesKey);
    const obj: AesKey = JSON.parse(json);

    const aesKey = await window.crypto.subtle.importKey(
        "jwk",
        obj.key,
        "AES-GCM",
        false,
        ["encrypt", "decrypt"]
    );

    const decryptedValue = await window.crypto.subtle.decrypt(
        {
            name: "AES-GCM",
            iv: fromBase64(obj.iv),
            tagLength: 128,
        },
        aesKey,
        fromBase64(data.data)
    );

    return getString(decryptedValue);
}

function getBytes(value: string) {
    const encoder = new TextEncoder();
    return encoder.encode(value);
}

function getString(value: ArrayBuffer) {
    const decoder = new TextDecoder("utf-8");
    return decoder.decode(value);
}

function toBase64(binaryData: ArrayBuffer | Int8Array | Int16Array | Int32Array | Uint8Array | Uint16Array | Uint32Array | Uint8ClampedArray | Float32Array | Float64Array | DataView) {
    if (binaryData instanceof Float32Array ||
        binaryData instanceof Float64Array ||
        binaryData instanceof Int8Array ||
        binaryData instanceof Int16Array ||
        binaryData instanceof Int32Array) {
        throw new Error("Insupported type: " + binaryData.constructor.name);
    }

    let str = "";
    if (binaryData instanceof DataView) {
        binaryData = binaryData.buffer;
    }

    if (binaryData instanceof ArrayBuffer) {
        binaryData = new Uint8Array(binaryData);
    }

    for (let i = binaryData.byteOffset / binaryData.BYTES_PER_ELEMENT; i < binaryData.byteLength / binaryData.BYTES_PER_ELEMENT; i++) {
        str += String.fromCharCode(binaryData[i]);
    }

    return btoa(str);
}

function fromBase64(str: string) {
    return new Uint8Array(atob(str).split('').map(c => c.charCodeAt(0)));
}