import { ApiError } from "./ApiError";
import { buildUrl } from '../Helpers/UrlBuilder';
import { DataAPI } from "../../types/dataApiDataStructures";

export class DataApiClient {
    endpoint: string;

    constructor(endpoint: string) {
        this.endpoint = endpoint;
        if(!endpoint.endsWith('/')) {
            this.endpoint += '/';
        }
    }

    getMany = async <T>(dataType: string, whereArguments?: string, orderByArguments?: string, limit?: number) => {
        const url = buildUrl(this.endpoint, '/api/DataIO/GetMany', {
            dataType: dataType,
            whereArguments: whereArguments,
            orderByArguments: orderByArguments,
            limit: limit?.toString()
        });
        const response = await fetch(url);
        if(!response.ok) {
            await this._handleError(response);
        }
        const allLines = await response.text();
        if(allLines.length === 0) {
            return [];
        }
        const lines = allLines.split('\n');
        const results: T[] = [];
        for(const line in lines) {
            const result = JSON.parse(line) as T;
            results.push(result);
        }
        return results;
    }

    exists = async (dataType: string, id: string) => {
        const url = buildUrl(this.endpoint, '/api/DataIO/Exists', {
            dataType: dataType,
            id: id
        });
        const response = await fetch(url);
        if(response.status === 200) {
            return true;
        } else if(response.status === 404) {
            return false;
        }
        await this._handleError(response);
    }

    getById = async <T>(dataType: string, id: string) => {
        const url = buildUrl(this.endpoint, '/api/DataIO/Get', {
            dataType: dataType,
            id: id
        });
        const response = await fetch(url);
        if(!response.ok) {
            if(response.status === 404) {
                return null;
            }
            return await this._handleError(response);
        }
        return await response.json() as T;
    }

    insert = async (dataType: string, obj: object, id?: string) => {
        return await this._submit(dataType, obj, false, id);
    }

    replace = async (dataType: string, obj: object, id: string) => {
        if(!id) {
            throw new Error("ID must be specified");
        }
        return await this._submit(dataType, obj, true, id);
    }

    getUserProfiles = async () => {
        const url = buildUrl(this.endpoint, '/api/Account/GetUserProfiles', {});
        const response = await fetch(url, { mode: 'cors' });
        if(!response.ok) {
            return await this._handleError(response);
        }
        return await response.json() as DataAPI.DataStructures.UserManagement.UserProfile[];
    }

    _handleError = async (response: Response) => {
        throw new ApiError(response.status, await response.text());
    }

    _submit = async (dataType: string, obj: object, overwrite: boolean, id?: string) => {
        const body : DataAPI.DataStructures.PostBodies.SubmitDataBody = {
            dataType: dataType,
            id: id ?? null,
            overwrite: overwrite,
            data: obj
        };
        const url = buildUrl(this.endpoint, '/api/DataIO/Submit', {});
        const response = await fetch(url, { 
            method: "POST", 
            body: JSON.stringify(body)
        });
        if(response.ok) {
            return await response.text();
        } else if(response.status === 409) {
            throw new ApiError(409, "An object with that ID already exists");
        }
        await this._handleError(response);
    }
}

export const dataApiClient = new DataApiClient("https://localhost:44387/");