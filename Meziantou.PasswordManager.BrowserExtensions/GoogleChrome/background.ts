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

function sendMessageToTab(tab: chrome.tabs.Tab, handler: (tabId: number, url: string) => void) {
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

function openPasswordManager(tab: chrome.tabs.Tab) {
  sendMessageToTab(tab, (tabId, url) => {
    chrome.tabs.sendMessage(tabId, <Meziantou.PasswordManager.Message>{
      type: Meziantou.PasswordManager.MessageType.NavigateLink,
      url: url
    });
  });
}

function fillForm(tab: chrome.tabs.Tab) {
  sendMessageToTab(tab, async (tabId, url) => {
    try {
      const result = await fetch(serviceUrlHttp + "/documents?url=" + encodeURIComponent(url), {
        method: "get",
        mode: "cors",
        cache: "no-cache"
      });

      if (result.ok) {
        const documents = await result.json();

        if (Array.isArray(documents)) {
          chrome.tabs.sendMessage(tabId, <Meziantou.PasswordManager.DocumentMessage>{
            type: Meziantou.PasswordManager.MessageType.Documents,
            url: url,
            documents: documents
          });
        } else {
          console.log("No password found for url '+" + url + "'");
        }
      } else {
        console.log("No password found for url '+" + url + "'");
      }
    } catch (ex) {
      chrome.tabs.sendMessage(tabId, <Meziantou.PasswordManager.Message>{
        type: Meziantou.PasswordManager.MessageType.NavigateLink,
        url: url
      });
    }
  });
}