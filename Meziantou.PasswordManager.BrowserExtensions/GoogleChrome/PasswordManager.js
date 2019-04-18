"use strict";
var Meziantou;
(function (Meziantou) {
    var PasswordManager;
    (function (PasswordManager) {
        function register() {
            chrome.runtime.onMessage.addListener(messageEventHandler);
        }
        PasswordManager.register = register;
        function messageEventHandler(message) {
            if (location.href !== message.url) {
                console.log("Invalid url");
                return;
            }
            switch (message.type) {
                case 0 /* Documents */:
                    const documentMessage = message;
                    fillFields(documentMessage.documents);
                    chrome.runtime.onMessage.removeListener(messageEventHandler);
                    break;
                case 1 /* NavigateLink */:
                    console.log("NavigateLink: " + message.url);
                    document.location.href = "meziantoupasswordmanager:search?url=" + encodeURIComponent(message.url);
                    chrome.runtime.onMessage.removeListener(messageEventHandler);
                    break;
            }
        }
        function fillFields(documents) {
            console.log("fillFields: " + documents.length + " documents");
            if (documents.length === 0) {
                alert("No entry found for the current page.");
                return;
            }
            for (const doc of documents) {
                const elements = getElements();
                const remainingElements = elements.slice(0);
                const fields = doc.Fields.sort(sortField);
                for (const field of fields) {
                    console.log(`Field: ${field.Name} (type: ${PasswordManager.FieldValueType[field.Type]})`);
                    const element = findBestElement(elements, remainingElements, field);
                    if (element) {
                        setValue(element, field);
                        // Remove element
                        const index = remainingElements.indexOf(element);
                        if (index >= 0) {
                            remainingElements.splice(index, 1);
                        }
                    }
                }
                return;
            }
        }
        function sortField(a, b) {
            const aType = getSortOrder(a);
            const bType = getSortOrder(b);
            if (aType !== bType) {
                return aType - bType;
            }
            return 0;
            function getSortOrder(field) {
                if (!isNullOrWhitespace(field.Selector)) {
                    return 1;
                }
                switch (field.Type) {
                    case PasswordManager.FieldValueType.Password:
                        return 2;
                    case PasswordManager.FieldValueType.Username:
                        return 3;
                    case PasswordManager.FieldValueType.EmailAddress:
                        return 4;
                    default:
                        return 5;
                }
            }
        }
        function isNullOrWhitespace(input) {
            if (typeof input === 'undefined' || input == null) {
                return true;
            }
            return /\s/g.test(input);
        }
        function setValue(element, field) {
            console.log(`Setting value for field: ${field.Name}`, element);
            if (element instanceof HTMLInputElement) {
                element.value = field.Value;
                const event = new Event("change");
                element.dispatchEvent(event);
                return;
            }
            console.warn("Cannot set value of element", element, field);
        }
        function findBestElement(elements, remainingElements, field) {
            const passwordElements = elements.filter(e => canSetValue(e, PasswordManager.FieldValueType.Password, field.Value, true));
            let filteredElements = remainingElements;
            // Find by selector
            if (typeof field.Selector === "string") {
                const selector = field.Selector;
                filteredElements = remainingElements.filter(e => {
                    try {
                        return e.matches(selector);
                    }
                    catch (ex) {
                        console.warn("Invalid selector: " + selector);
                        return false;
                    }
                });
                if (filteredElements.length === 1) {
                    console.log(`Element found by selector`);
                    return filteredElements[0];
                }
                console.log(`Element not found by selector`);
                return null;
            }
            // Find by type
            filteredElements = remainingElements.filter(e => canSetValue(e, field.Type, field.Value, true));
            if (filteredElements.length === 1) {
                console.log(`Element found by exact type`);
                return filteredElements[0];
            }
            filteredElements = remainingElements.filter(e => canSetValue(e, field.Type, field.Value, false));
            if (filteredElements.length === 1) {
                console.log(`Element found by relaxed type`);
                return filteredElements[0];
            }
            // Find by autocomplete
            let byAutoComplete;
            switch (field.Type) {
                case PasswordManager.FieldValueType.Username:
                    byAutoComplete = filteredElements.filter(e => isAutocompleteType(e, "nickname", "name"));
                    break;
                case PasswordManager.FieldValueType.EmailAddress:
                    byAutoComplete = filteredElements.filter(e => isAutocompleteType(e, "email"));
                    break;
            }
            if (byAutoComplete && byAutoComplete.length === 1) {
                console.log(`Element found by autocomplete`);
                return byAutoComplete[0];
            }
            // Find by name
            let byName = filteredElements.filter(e => e instanceof HTMLInputElement && e.name === field.Name);
            if (byName.length === 1) {
                console.log(`Element found by name`);
                return byName[0];
            }
            // Find nearest field of password
            if (field.Type === PasswordManager.FieldValueType.Username || field.Type === PasswordManager.FieldValueType.EmailAddress) {
                if (passwordElements.length === 1) {
                    let passwordElement = passwordElements[0];
                    while (passwordElement) {
                        const pe = passwordElement;
                        const byCommonAncestor = filteredElements.filter(e => pe.contains(e));
                        if (byCommonAncestor.length === 1) {
                            console.log(`Element found by nearest`);
                            return byCommonAncestor[0];
                        }
                        if (byCommonAncestor.length > 1) {
                            console.log(`Element not found by nearest (elements > 1)`);
                            break;
                        }
                        passwordElement = passwordElement.parentElement;
                    }
                }
            }
            console.log(`Element not found`);
            return null;
        }
        function canSetValue(element, fieldValueType, value, exactMatch) {
            if (isInputType(element, "hidden", "submit")) {
                return false;
            }
            if (fieldValueType === PasswordManager.FieldValueType.Password) {
                return isInputType(element, "password");
            }
            if (fieldValueType === PasswordManager.FieldValueType.Username) {
                if (!exactMatch && isEmailValue(value)) {
                    return isInputType(element, "text", "email");
                }
                else {
                    return isInputType(element, "text");
                }
            }
            if (fieldValueType === PasswordManager.FieldValueType.EmailAddress) {
                if (exactMatch) {
                    return isInputType(element, "email");
                }
                return isInputType(element, "text", "email");
            }
            if (fieldValueType === PasswordManager.FieldValueType.Url) {
                return isInputType(element, "url");
            }
            return false;
        }
        function isEmailValue(value) {
            return /[a-z0-9!#$%&'*+/=?^_`{|}~.-]+@[a-z0-9-]+(\.[a-z0-9-]+)*/.test(value);
        }
        function isInputType(element, ...types) {
            if (element instanceof HTMLInputElement) {
                return types.findIndex(type => type === element.type) >= 0;
            }
            return false;
        }
        function isAutocompleteType(element, ...types) {
            if (element instanceof HTMLInputElement) {
                return types.findIndex(type => type === element.autocomplete) >= 0;
            }
            return false;
        }
        function getElements() {
            const form = findFormWithPasswordField();
            if (form) {
                return Array.from(form.elements).filter(filterFields);
            }
            return Array.from(document.querySelectorAll("input")).filter(filterFields);
        }
        function findFormWithPasswordField() {
            for (let i = 0; i < document.forms.length; i++) {
                const form = document.forms[i];
                for (let j = 0; j < form.elements.length; j++) {
                    const element = form.elements[j];
                    if (isInputType(element, "password")) {
                        return form;
                    }
                }
            }
            return null;
        }
        function filterFields(element) {
            if (isInputType(element, "button", "image", "hidden", "reset", "submit")) {
                return false;
            }
            if (element instanceof HTMLElement) {
                if (element.tabIndex < 0) {
                    return false;
                }
            }
            const style = window.getComputedStyle(element);
            if (style.display === "none") {
                return false;
            }
            if (style.opacity === "0") {
                return false;
            }
            return true;
        }
    })(PasswordManager = Meziantou.PasswordManager || (Meziantou.PasswordManager = {}));
})(Meziantou || (Meziantou = {}));
Meziantou.PasswordManager.register();
