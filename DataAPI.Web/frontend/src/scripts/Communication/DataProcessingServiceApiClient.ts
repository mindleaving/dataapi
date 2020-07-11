export class DataProcessingServiceApiClient {
    endpoint: string;

    constructor(endpoint: string) {
        this.endpoint = endpoint;
        if(!endpoint.endsWith('/')) {
            this.endpoint += '/';
        }
    }

    
}

export const dataProcessingServiceApiClient = new DataProcessingServiceApiClient("https://localhost:44388");