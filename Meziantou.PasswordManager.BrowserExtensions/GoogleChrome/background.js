"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
const serviceUrlHttp = "http://localhost:47532";
chrome.contextMenus.create({
    title: "Fill form",
    contexts: ["browser_action"],
    onclick: function (obj, tab) {
        fillForm(tab);
    }
});
chrome.contextMenus.create({
    title: "Open PasswordManager",
    contexts: ["browser_action"],
    onclick: function (obj, tab) {
        openPasswordManager(tab);
    }
});
chrome.browserAction.onClicked.addListener(function (tab) {
    fillForm(tab);
});
function sendMessageToTab(tab, handler) {
    const tabId = tab.id;
    if (typeof tabId === "number") {
        const url = tab.url;
        if (!url) {
            return;
        }
        chrome.tabs.executeScript({
            file: "Declarations.js"
        }, () => {
            chrome.tabs.executeScript({
                file: "PasswordManager.js"
            }, () => {
                handler(tabId, url);
            });
        });
    }
}
function openPasswordManager(tab) {
    sendMessageToTab(tab, (tabId, url) => {
        chrome.tabs.sendMessage(tabId, {
            type: 1 /* NavigateLink */,
            url: url
        });
    });
}
function fillForm(tab) {
    sendMessageToTab(tab, (tabId, url) => __awaiter(this, void 0, void 0, function* () {
        try {
            const result = yield fetch(serviceUrlHttp + "/documents?url=" + encodeURIComponent(url), {
                method: "get",
                mode: "cors",
                cache: "no-cache"
            });
            if (result.ok) {
                const documents = yield result.json();
                if (Array.isArray(documents)) {
                    chrome.tabs.sendMessage(tabId, {
                        type: 0 /* Documents */,
                        url: url,
                        documents: documents
                    });
                }
                else {
                    console.log("No password found for url '+" + url + "'");
                }
            }
            else {
                console.log("No password found for url '+" + url + "'");
            }
        }
        catch (ex) {
            chrome.tabs.sendMessage(tabId, {
                type: 1 /* NavigateLink */,
                url: url
            });
        }
    }));
}
