import { appendChild } from '../ui/jsx';
import * as jsx from '../ui/jsx';
import { PasswordGeneratorOptions, generatePassword } from '../password-generator';
import { nameof, isNumber } from '../utilities';
import * as clipboard from '../clipboard';

const options: PasswordGeneratorOptions = {
    length: 16,
    numbers: true,
    lowerLetters: true,
    upperLetters: true,
    lowerAccents: true,
    upperAccents: true,
    specialCharacters: true,
    unicode: true,
}

export function openPasswordGenerator(): Promise<string | null> {
    const dialog = document.createElement("dialog");
    appendChild(dialog,
        <div>
            <form method="dialog" onsubmit={e => dialog.returnValue = getPassword(dialog)}>
                <input type="text" name="password" />
                <button type="button" onclick={e => generate(dialog)}>Generate</button>
                <div>
                    <button type="submit">Save and copy to clipboard</button>
                </div>
            </form>
            <details open>
                <summary>Options</summary>
                <div>
                    <div>
                        <label for="pgLength">Length</label>
                        <input id="pgLength" type="number" name="length" min="1" step="1" />
                    </div>
                    <div>
                        <input id="pgNumbers" type="checkbox" name="numbers" />
                        <label for="pgNumbers">Numbers</label>
                    </div>
                    <div>
                        <input id="pgLowerLetters" type="checkbox" name="lowerLetters" />
                        <label for="pgLowerLetters">Lowercase letters</label>
                    </div>
                    <div>
                        <input id="pgUpperLetters" type="checkbox" name="upperLetters" />
                        <label for="pgUpperLetters">Uppercase letters</label>
                    </div>
                    <div>
                        <input id="pgLowerAccents" type="checkbox" name="lowerAccents" />
                        <label for="pgLowerAccents">Lowercase accents</label>
                    </div>
                    <div>
                        <input id="pgUpperAccents" type="checkbox" name="upperAccents" />
                        <label for="pgUpperAccents">Uppercase accents</label>
                    </div>
                    <div>
                        <input id="pgSpecial" type="checkbox" name="specialCharacters" />
                        <label for="pgSpecial">Special characters</label>
                    </div>
                    <div>
                        <input id="pgUnicode" type="checkbox" name="unicode" />
                        <label for="pgUnicode">Unicode characters</label>
                    </div>
                </div>
            </details>
        </div>);

    addEvents(dialog);
    generate(dialog);

    document.body.appendChild(dialog);
    dialog.showModal();

    return new Promise<string | null>((resolve, reject) => {
        dialog.addEventListener("close", e => {
            console.log("close: " + dialog.returnValue);
            dialog.remove();
            if (dialog.returnValue) {
                resolve(dialog.returnValue);
                clipboard.set(dialog.returnValue).catch(() => console.error("Cannot set value to clipboard"));
            }

            resolve(null);
        }, {
                once: true
            });
    });
}

function addEvents(dialog: HTMLElement) {
    const names: (keyof PasswordGeneratorOptions)[] = [
        "length",
        "lowerAccents",
        "lowerLetters",
        "numbers",
        "specialCharacters",
        "unicode",
        "upperAccents",
        "upperLetters"
    ];

    names.forEach(name => {
        const value = options[name];
        const input = dialog.querySelector(`[name='${name}']`) as HTMLInputElement;
        if (isNumber(value)) {
            input.valueAsNumber = value;
        } else {
            input.checked = value;
        }

        ["change", "input"].forEach(eventName => {
            input.addEventListener(eventName, e => {
                if (input.type === "number") {
                    options[name] = input.valueAsNumber;
                } else {
                    options[name] = input.checked;
                }

                generate(dialog);
            });
        });
    });
}

function generate(dialog: HTMLElement) {
    const result = generatePassword(options);
    const input = dialog.querySelector(`[name='password']`) as HTMLInputElement;
    input.value = result;
}

function getPassword(dialog: HTMLElement) {
    const input = dialog.querySelector(`[name='password']`) as HTMLInputElement;
    return input.value;
}