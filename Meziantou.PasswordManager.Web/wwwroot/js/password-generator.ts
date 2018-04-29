export interface PasswordGeneratorOptions {
    length: number;
    numbers: boolean;
    lowerLetters: boolean;
    upperLetters: boolean;
    lowerAccents: boolean;
    upperAccents: boolean;
    specialCharacters: boolean;
    unicode: boolean;
}

export function generatePassword(options: PasswordGeneratorOptions) {
    const alphabet = randomizeString(generateAlphabet(options));
    if (alphabet.length === 0) {
        return "";
    }

    let result = "";
    const values = generateRandomValues(options.length);
    for (const v of values) {
        result += alphabet[v % alphabet.length];
    }

    return result;
}

function generateRandomValues(length: number) {
    const array = new Uint32Array(length);
    window.crypto.getRandomValues(array);
    return array;
}

function randomizeString(str: string) {
    return Array.from(str)
        .map(c => ({ c: c, index: generateRandomValues(1)[0] }))
        .sort(_ => _.index)
        .map(_ => _.c)
        .join("");
}

function generateAlphabet(options: PasswordGeneratorOptions) {
    let result = "";
    if (options.numbers) {
        result += "0123456789";
    }

    if (options.lowerLetters) {
        result += "abcdefghijklmnopqrstuvwxyz";
    }

    if (options.upperLetters) {
        result += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    }

    if (options.lowerAccents) {
        result += "àèìòùáéíóúýâêîôûãñõäëïöüÿç";
    }

    if (options.upperAccents) {
        result += "ÀÈÌÒÙÁÉÍÓÚÝÂÊÎÔÛÃÑÕÄËÏÖÜŸÇ";
    }

    if (options.specialCharacters) {
        result += "!#$%&'()*+,-./:;<=>?@[\]_";
    }

    if (options.unicode) {
        result += "£¤~`¿¡øßðåÐØÅ"; // TODO improve this list
    }

    return result;
}