// https://developers.google.com/web/updates/2018/03/clipboardapi
export function set(value: string) : Promise<void> {
    return (navigator as any).clipboard.writeText(value);;
}