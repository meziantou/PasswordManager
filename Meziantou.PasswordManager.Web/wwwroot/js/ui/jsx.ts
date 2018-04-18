import { isFunction, isBoolean } from '../utilities';

export var Fragment = "<></>";

export function createElement(tagName: string, attributes: JSX.AttributeCollection | null, ...children: any[]): Element | DocumentFragment {
    if (tagName === Fragment) {
        return document.createDocumentFragment();
    }

    const element = document.createElement(tagName);
    if (attributes) {
        for (const key of Object.keys(attributes)) {
            const attributeValue = attributes[key];

            if (key === "className") { // JSX does not allow class as a valid name
                element.setAttribute("class", attributeValue);
            } else if (key.startsWith("on") && isFunction(attributes[key])) {
                element.addEventListener(key.substring(2), attributeValue);
            } else {
                if (isBoolean(attributeValue) && attributeValue) {
                    element.setAttribute(key, "");
                } else {
                    element.setAttribute(key, attributeValue);
                }
            }
        }
    }

    for (const child of children) {
        appendChild(element, child);
    }

    return element;
}

export function appendChild(parent: Node, child: any) {
    if (typeof child === "undefined" || child === null) {
        return;
    }

    if (Array.isArray(child)) {
        for (const value of child) {
            appendChild(parent, value);
        }
    } else if (typeof child === "string") {
        parent.appendChild(document.createTextNode(child));
    } else if (child instanceof Node) {
        parent.appendChild(child);
    } else {
        parent.appendChild(document.createTextNode(String(child)));
    }
}