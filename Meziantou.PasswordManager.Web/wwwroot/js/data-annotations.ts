import "reflect-metadata";
import { isUndefined, isFunction, isObject, isNullOrUndefined } from './utilities';

export const getPropertyMetadata = Symbol("Metadata");
const metadataName = "dataAnnotations";

export function required(target: any, propertyKey: string) {
    setPropertyAnnotation(target, propertyKey, "required", true);
}

export function readonly(target: any, propertyKey: string) {
    setPropertyAnnotation(target, propertyKey, "readonly", true);
}

export function editable(target: any, propertyKey?: string) {
    if (propertyKey) {
        setPropertyAnnotation(target, propertyKey, "editable", true);
    } else {
        setObjectAnnotation(target, "editable", true);
    }
}

export function dataType(dataType: DataType) {
    return (target: any, propertyKey: string) => {
        setPropertyAnnotation(target, propertyKey, "dataType", dataType);
    };
}

export function lookupValues(values: LookupValue[]) {
    return (target: any, propertyKey: string) => {
        setPropertyAnnotation(target, propertyKey, "lookupValues", values);
    };
}

export function autoComplete(value: boolean) {
    return (target: any, propertyKey?: string) => {
        if (propertyKey) {
            setPropertyAnnotation(target, propertyKey, "autoComplete", value);
        } else {
            setObjectAnnotation(target, "autoComplete", value);
        }
    };
}

function setPropertyAnnotation<T extends keyof PropertyDataAnnotations>(target: any, propertyKey: string, name: T, value: PropertyDataAnnotations[T]) {
    const objectAnnotations = getObjectAnnotations(target.constructor);
    Reflect.defineMetadata(metadataName, objectAnnotations, target.constructor);

    const propertyAnnotations: PropertyDataAnnotations = Reflect.getMetadata(metadataName, target, propertyKey) || {};
    Reflect.defineMetadata(metadataName, propertyAnnotations, target, propertyKey);

    propertyAnnotations[name] = value;

    if (!objectAnnotations.properties.includes(propertyKey)) {
        objectAnnotations.properties.push(propertyKey);
    }
}

function setObjectAnnotation<T extends keyof ObjectDataAnnotations>(target: any, name: T, value: ObjectDataAnnotations[T]) {
    const objectAnnotations = getObjectAnnotations(target);
    objectAnnotations[name] = value;
    Reflect.defineMetadata(metadataName, objectAnnotations, target);
}

function getObjectAnnotations(target: any): ObjectDataAnnotations {
    const metadata = Reflect.getMetadata(metadataName, target);
    return metadata || { properties: [] };
}

export function getObjectAnnotationsFromInstance(target: any): ObjectDataAnnotations {
    if (isNullOrUndefined(target)) {
        return { properties: [] };
    }

    const metadata = Reflect.getMetadata(metadataName, target.constructor);
    return metadata || { properties: [] };
}

function getPropertyAnnotations(target: any, propertyKey: string): PropertyDataAnnotations {
    const objAnnotations = getObjectAnnotations(target);
    const propertyAnnotations: PropertyDataAnnotations = Reflect.getMetadata(metadataName, target, propertyKey) || {};
    const result = { ...objAnnotations, ...propertyAnnotations, properties: undefined };

    if (isUndefined(result.dataType)) {
        const designType = Reflect.getMetadata("design:type", target, propertyKey);
        if (designType) {
            if (designType === String) {
                result.dataType = DataType.string;
            } else if (designType === Number) {
                result.dataType = DataType.number;
            }
        }
    }

    const f = target[getPropertyMetadata];
    if (isFunction(f)) {
        return f(propertyKey, result);
    }

    return result;
}

export function getPropertyAnnotationsFromInstance(target: any, propertyKey: string): PropertyDataAnnotations {
    const objAnnotations = getObjectAnnotationsFromInstance(target);
    const propertyAnnotations: PropertyDataAnnotations = Reflect.getMetadata(metadataName, target, propertyKey) || {};
    const result = { ...objAnnotations, ...propertyAnnotations, properties: undefined };
    delete result.properties;

    if (isUndefined(result.dataType)) {
        const designType = Reflect.getMetadata("design:type", target, propertyKey);
        if (designType) {
            if (designType === String) {
                result.dataType = DataType.string;
            } else if (designType === Number) {
                result.dataType = DataType.number;
            }
        }
    }

    const f = target[getPropertyMetadata];
    if (isFunction(f)) {
        return f(propertyKey, result);
    }

    return result;
}

export interface ObjectDataAnnotations {
    properties: string[];
    required?: boolean;
    editable?: boolean;
    readonly?: boolean;
    dataType?: DataType;
    autoComplete?: boolean;
}

export interface PropertyDataAnnotations {
    required?: boolean;
    editable?: boolean;
    readonly?: boolean;
    dataType?: DataType;
    lookupValues?: LookupValue[];
    autoComplete?: boolean;
}

export interface LookupValue {
    text?: string;
    value: string;
}

export const enum DataType {
    string,
    emailAddress,
    password,
    number
}