import "reflect-metadata";
import { isUndefined, isFunction } from './utilities';

export const getPropertyMetadata = Symbol("Metadata");

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

function setPropertyAnnotation<T extends keyof PropertyDataAnnotations>(target: any, propertyKey: string, name: T, value: PropertyDataAnnotations[T]) {
    const metadataKey = "dataAnnotations";
    const objectAnnotations = getObjectAnnotations(target);
    Reflect.defineMetadata("dataAnnotations", objectAnnotations, target);

    const propertyAnnotations: PropertyDataAnnotations = Reflect.getMetadata("dataAnnotations", target, propertyKey) || {};
    Reflect.defineMetadata("dataAnnotations", propertyAnnotations, target, propertyKey);

    propertyAnnotations[name] = value;

    if (!objectAnnotations.properties.includes(propertyKey)) {
        objectAnnotations.properties.push(propertyKey);
    }
}

function setObjectAnnotation<T extends keyof ObjectDataAnnotations>(target: any, name: T, value: ObjectDataAnnotations[T]) {
    const metadataKey = "dataAnnotations";
    const objectAnnotations = getObjectAnnotations(target);
    Reflect.defineMetadata("dataAnnotations", objectAnnotations, target);
    objectAnnotations[name] = value;
}

export function getObjectAnnotations(target: any): ObjectDataAnnotations {
    const d = getDefaultObjectAnnotations();
    return { ...d, ...Reflect.getMetadata("dataAnnotations", target) };
}

export function getPropertyAnnotations(target: any, propertyKey: string): PropertyDataAnnotations {
    const objAnnotations = getObjectAnnotations(target);
    const propertyAnnotations: PropertyDataAnnotations = Reflect.getMetadata("dataAnnotations", target, propertyKey) || {};
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

function getDefaultObjectAnnotations(): ObjectDataAnnotations {
    return { properties: [] };
}

export interface ObjectDataAnnotations {
    properties: string[];
    required?: boolean;
    editable?: boolean;
    readonly?: boolean;
    dataType?: DataType;
}

export interface PropertyDataAnnotations {
    required?: boolean;
    editable?: boolean;
    readonly?: boolean;
    dataType?: DataType;
    lookupValues?: LookupValue[];
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