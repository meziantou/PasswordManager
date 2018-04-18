export function isUndefined(obj: any): obj is undefined {
    return typeof (obj) === "undefined";
}

export function isNullOrUndefined(obj: any): obj is undefined | null {
    return isUndefined(obj) || obj === null;
}

export function isNullOrEmpty(str: string | null | undefined): str is undefined | null | "" {
    return isNullOrUndefined(str) || str === "";
}

export function isFunction(obj: any): obj is Function {
    return typeof (obj) === "function";
}

export function isObject(obj: any): obj is Object {
    return obj !== null && typeof (obj) === "object";
}

export function isNumber(obj: any): obj is number {
    return typeof (obj) === "number" && !isNaN(obj);
}

export function isString(obj: any): obj is string {
    return typeof (obj) === "string";
}

export function isBoolean(obj: any): obj is boolean {
    return typeof (obj) === "boolean";
}

export function isDate(obj: any): obj is Date {
    return obj instanceof Date;
}

export function isArray(obj: any): obj is Array<any> {
    return Array.isArray(obj);
}

export function isPromiseLike(obj: any): obj is PromiseLike<any> {
    return isObject(obj) && isFunction(obj.then);
}

export function isArrayLike<T>(obj: any): obj is ArrayLike<T> {
    return isObject(obj) && ("length" in obj);
}

export function nameof<T>(propertyName: keyof T) {
    return propertyName;
}

export function parseString(obj: any): string | null {
    if (isNullOrUndefined(obj)) {
        return null;
    }

    if (isString(obj)) {
        return obj;
    }

    return String(obj);
}

export function parseNumber<T>(obj: any, defaultValue: T): number | T {
    if (isNullOrUndefined(obj)) {
        return defaultValue;
    }

    if (isNumber(obj)) {
        return obj;
    }

    if (isString(obj)) {
        const f = parseFloat(obj);
        if (!isNaN(f)) {
            return f;
        }

        const i = parseInt(obj, 10);
        if (!isNaN(i)) {
            return i;
        }
    }

    return defaultValue;
}

export function parseBoolean<T>(obj: any, defaultValue: T): boolean | T {
    if (isBoolean(obj)) {
        return obj;
    }

    if (isNumber(obj)) {
        return obj !== 0;
    }

    if (isString(obj)) {
        let upper = obj.toUpperCase();
        switch (upper) {
            case "TRUE":
            case "1":
            case "YES":
            case "Y":
                return true;

            case "FALSE":
            case "0":
            case "NO":
            case "N":
                return false;
        }
    }

    return defaultValue;
}

export function parseInteger<T>(str: any, defaultValue: T): number | T {
    let parsedValue = parseNumber(str, NaN);
    if (isNaN(parsedValue)) {
        return defaultValue;
    }

    if (isNumber(parsedValue)) {
        return Math.trunc(parsedValue);
    }

    return defaultValue;
}

export function parseDateTime<T>(value: any, defaultValue: T): Date | T {
    if (isDate(value)) {
        return value;
    }

    if (isString(value)) {
        // Format /Date(<ticks>+<offset>)/
        let a = /\/Date\(((?:-|\+)?\d*)((?:-|\+)\d*)?\)\//.exec(value);
        if (a) {
            var date = new Date(+a[1]); // new Date(ticks) = new Date(Ticks + getTimeZone) => We need to remove the timezone
            date = new Date(date.getTime() + (date.getTimezoneOffset() * 60 * 1000));
            if (a[2]) {
                // add timezone
                var tz = +a[2]; // +0200, -0130
                var offset = (tz / 100 * 60) + (tz % 100); // minutes
                return new Date(date.getTime() + offset * 60000);
            }
            return date;
        }

        // Format 0000-00-00T00:00:00
        // Format 0000-00-00 00:00:00
        // Format 0000/00/00 00:00:00
        let b = /(\d{4})(\/|-)(\d{1,2})(\/|-)(\d{1,2})((T| )(\d{1,2}):(\d{1,2})(:(\d{1,2})))?/.exec(value);
        if (b) {
            // ["2012-01-01T01:02:03", "2012", "-", "01", "-", "01", "T01:02:03", "T", "01", "02", ":03", "03"]
            let year = parseInteger(b[1], 0);
            let month = parseInteger(b[3], 1) - 1; // 0 based
            let day = parseInteger(b[5], 0);
            let hours = parseInteger(b[8], 0);
            let minutes = parseInteger(b[9], 0);
            let seconds = parseInteger(b[11], 0);

            return new Date(year, month, day, hours, minutes, seconds, 0);
        }
    }

    return defaultValue;
}

export function registerEventHandler<K extends keyof HTMLElementEventMap>(target: HTMLElement, eventName: K, listener: (this: HTMLElement, ev: HTMLElementEventMap[K]) => any, useCapture?: boolean): DisposableEventHandler;
export function registerEventHandler<K extends keyof ElementEventMap>(target: Element, eventName: K, listener: (this: Element, ev: ElementEventMap[K]) => any, useCapture?: boolean): DisposableEventHandler;
export function registerEventHandler<K extends keyof DocumentEventMap>(target: Document, eventName: K, listener: (this: Document, ev: DocumentEventMap[K]) => any, useCapture?: boolean): DisposableEventHandler;
export function registerEventHandler<K extends keyof DocumentEventMap>(target: Document, eventName: K, listener: (this: Window, ev: DocumentEventMap[K]) => any, useCapture?: boolean): DisposableEventHandler;
export function registerEventHandler<K extends keyof WindowEventMap>(target: Window, eventName: K, listener: (this: Window, ev: WindowEventMap[K]) => any, useCapture?: boolean): DisposableEventHandler;
export function registerEventHandler(target: EventTarget, eventName: string, handler: (e: Event) => void, useCapture?: boolean): DisposableEventHandler {
    target.addEventListener(eventName, handler, useCapture);

    return <DisposableEventHandler>{
        target: target,
        eventName: eventName,
        useCapture: useCapture,
        removeHandler: function () {
            target.removeEventListener(eventName, handler, useCapture);
        }
    };
}

export interface DisposableEventHandler {
    target: EventTarget;
    eventName: string;
    useCapture: boolean | undefined;
    removeHandler(): void;
}

export function debounce(wait: number, func: (...args: any[]) => any, immediate: boolean = false): (...args: any[]) => any {
    let timeout: number | null = null;
    let result: any;
    return function (this: any, ...args: any[]) {
        var context = this;
        var later = () => {
            timeout = null;
            if (!immediate) {
                result = func.apply(this, args);
            }
        };
        var callNow = immediate && !timeout;
        if (timeout !== null) {
            clearTimeout(timeout);
        }
        timeout = setTimeout(later, wait);
        if (callNow) {
            result = func.apply(context, args);
        }
        return result;
    };
};