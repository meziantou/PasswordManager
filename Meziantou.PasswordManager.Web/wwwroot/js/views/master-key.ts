import { dataType, DataType, required, autoComplete } from '../data-annotations';
import { openModalForm } from '../ui/form-component';
import { isNullOrEmpty, throttle } from '../utilities';

let masterKey: string | null = null;
let timeoutHandle: number | undefined;

function setCache(key: string) {
    console.log("Store master key in cache");
    masterKey = key;
    clearTimeout();
    timeoutHandle = setTimeout(clearCache, 30000);
}

export function clearCache() {
    console.log("Remove master key from cache");
    masterKey = null;
    clearTimeout();
}

function clearTimeout() {
    window.clearTimeout(timeoutHandle);
    timeoutHandle = undefined;
}

export interface GetMasterKeyOptions {
    attempt?: number;
    useCache?: boolean;
}

export async function usingMasterKey<T>(f: (masterKey: string) => Promise<T>): Promise<T | undefined> {
    const options: Required<GetMasterKeyOptions> = {
        useCache: true,
        attempt: 0
    };

    while (true) {
        const masterKey = await getMasterKey(options);
        if (isNullOrEmpty(masterKey)) {
            return undefined;
        }

        try {
            const result = await f(masterKey);
            setCache(masterKey);
            return result;
        } catch (ex) {
            if (ex instanceof DOMException) {
                options.attempt += 1;
                clearCache();
                continue;
            } else {
                throw ex;
            }
        }
    }
}

async function getMasterKey(options: GetMasterKeyOptions): Promise<string | null> {
    if (masterKey && options && options.useCache) {
        return masterKey;
    }

    const result = await openModalForm(new MasterKey());
    if (result) {
        return result.password;
    }

    return null;
}

@autoComplete(false)
class MasterKey {
    @required
    @dataType(DataType.password)
    public password: string = "";
}