export const getCookieValue = (name: string) => {
    const matchingEntry = document.cookie
        .split(';')
        .find(entry => entry.trimLeft().startsWith(`${name}=`));
    if(!matchingEntry) {
        return undefined;
    }
    return matchingEntry.split('=')[1];
}

export const setCookie = (name: string, value: string, secure: boolean, expires?: Date, path?: string) => {
    let cookieValue = `${name}=${value}`;
    if(secure) {
        cookieValue += ";secure";
    }
    if(expires) {
        cookieValue += `;expires=${expires.toUTCString()}`;
    }
    if(path) {
        cookieValue += `;path=${path}`;
    }
    document.cookie = cookieValue;
}

export const removeCookie = (name: string) => {
    setCookie(name, '', false, new Date(0));
}