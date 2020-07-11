
export function buildUrl(endpoint: string, path: string, params: { [key: string]: string | undefined }) {
    let url = endpoint;
    if(!url.endsWith('/')) {
        url += '/';
    }
    if(path.startsWith('/')) {
        url += path.substring(1);
    } else {
        url += path;
    }
    let firstQueryParameter = true;
    for(const name in params) {
        const value = params[name];
        if(value) {
            if(firstQueryParameter) {
                url += '?';
                firstQueryParameter = false;
            } else {
                url += '&';
            }
            url += `${encodeURIComponent(name)}=${encodeURIComponent(value)}`;
        }
    }
    return url;
}