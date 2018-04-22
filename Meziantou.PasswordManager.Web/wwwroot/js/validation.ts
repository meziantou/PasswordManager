import "reflect-metadata";
import { DataType, getObjectAnnotationsFromInstance, getPropertyAnnotationsFromInstance } from "./data-annotations";

export interface IValidationRule {
    evaluate(target: any, value: any, key: string): string | null;
}

export interface ValidationError {
    propertyName: string | null;
    errors: string[];
}

export function validationRule(rule: IValidationRule) {
    return (target: any, propertyKey: string) => {
        addValidationRule(target, propertyKey, rule);
    };
}

export function validate(target: any): ValidationError[] {
    // Get the list of properties to validate
    const keys = getProperties(target);
    const errors: ValidationError[] = [];
    for (const key of keys) {
        const propertyErrors = validateProperty(target, key);
        if (propertyErrors.length > 0) {
            errors.push({
                propertyName: key,
                errors: propertyErrors
            });
        }
    }

    return errors;
}

export function validateProperty<T>(target: T, propertyKey: keyof T): string[] {
    let rules = Reflect.getMetadata("validation", target, propertyKey) as IValidationRule[] | undefined;
    if (!Array.isArray(rules)) {
        rules = [];
    }

    // Get the list of properties to validate
    const annotations = getPropertyAnnotationsFromInstance(target, propertyKey);
    if (annotations.required) {
        rules.push(RequiredValidationRule.instance);
    }

    if (annotations.dataType === DataType.emailAddress) {
        rules.push(EmailAddressValidationRule.instance);
    }

    const errorMessages: string[] = [];
    for (const rule of new Set(rules)) {
        const error = rule.evaluate(target, target[propertyKey], propertyKey);
        if (error) {
            errorMessages.push(error);
        }
    }

    return errorMessages;
}

export function isValid(target: any) {
    const validationResult = validate(target);
    return validationResult.length === 0;
}

function addValidationRule(target: any, propertyKey: string, rule: IValidationRule) {
    const rules: IValidationRule[] = Reflect.getMetadata("validation", target, propertyKey) || [];
    rules.push(rule);

    // list of properties to validate
    let properties: string[] = Reflect.getMetadata("validation", target) || [];
    if (properties.indexOf(propertyKey) < 0) {
        properties.push(propertyKey);
    }

    Reflect.defineMetadata("validation", properties, target);
    Reflect.defineMetadata("validation", rules, target, propertyKey);
}

function getProperties(target: any) {
    const keys = Reflect.getMetadata("validation", target) as string[] | undefined;
    const annotations = getObjectAnnotationsFromInstance(target);

    const result = new Set<string>(annotations.properties);
    if (keys) {
        for (const key of keys) {
            result.add(key);
        }
    }

    return result;
}

class RequiredValidationRule implements IValidationRule {
    static instance = new RequiredValidationRule();

    evaluate(_target: any, value: any, key: string): string | null {
        if (value) {
            return null;
        }

        return `${key} is required`;
    }
}

class EmailAddressValidationRule implements IValidationRule {
    static regex = /^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
    static instance = new EmailAddressValidationRule();

    evaluate(_target: any, value: any, key: string): string | null {
        if (typeof value === "string" && value !== "") {
            if (EmailAddressValidationRule.regex.test(value)) {
                return null;
            }

            return `'${value}' is not a valid email address`;
        }

        return null;
    }
}

export class CompareOtherPropertyValidationRule<T> implements IValidationRule {
    constructor(private otherPropertyName: keyof T) {
    }

    public evaluate(target: any, value: any, key: string): string | null {
        const otherValue = target[this.otherPropertyName];
        if (value !== otherValue) {
            return `Value does not match ${this.otherPropertyName}`;
        }

        return null;
    }
}