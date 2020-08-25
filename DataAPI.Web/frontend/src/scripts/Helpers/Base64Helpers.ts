export const decodeUnicode = (str: string) => {
    // Going backwards: from bytestream, to percent-encoding, to original string.
    // From https://stackoverflow.com/questions/30106476/using-javascripts-atob-to-decode-base64-doesnt-properly-decode-utf-8-strings

    return decodeURIComponent(atob(str).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
}

export const decodeJwtAccessToken = (accessToken: string) => {
    const parts = accessToken.split('.');
    //const algorithmPart = parts[0];
    const claimsPart = parts[1];
    //const signiturePare = parts[2];
    return JSON.parse(decodeUnicode(claimsPart));
}