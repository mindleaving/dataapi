import { ApiError } from "./ApiError";
import { buildUrl } from '../Helpers/UrlBuilder';
import { v4 as uuidv4 } from 'uuid';
import { DataAPI } from "../../types/dataApiDataStructures";
import { LoginMethod, Role, ResultFormat, DataModificationType, AuthenticationErrorType } from "../../types/dataApiDataStructuresEnums";
import { getCookieValue, setCookie, removeCookie } from "../Helpers/CookieHelpers";
import { decodeJwtAccessToken } from "../Helpers/Base64Helpers";

export class DataApiClient {
    endpoint: string;
    loginMethod: LoginMethod;

    constructor(endpoint: string) {
        this.endpoint = endpoint;
        if(!endpoint.endsWith('/')) {
            this.endpoint += '/';
        }
        this.loginMethod = LoginMethod.ActiveDirectory;
    }

    isAvailable = async (): Promise<boolean> => {
        try {
            const response = await this._sendRequest('GET', 'api/ServiceStatus/ping');
            return response.ok;
        } catch {
            return false;
        }
    }

    isLoggedIn = () => {
        const isLoggedIn = getCookieValue('isLoggedIn');
        if(!isLoggedIn) {
            return false;
        }
        return isLoggedIn === 'true';
    }
    getLoggedInUsername = () => {
        return getCookieValue('loggedInUsername');
    }

    getUserProfiles = async () => {
        const response = await this._sendRequest('GET', 'api/Account/GetUserProfiles');
        return await response.json() as DataAPI.DataStructures.UserManagement.UserProfile[];
    }

    getGlobalRolesForUser = async (username: string): Promise<Role[]> => {
        const response = await this._sendRequest('GET', 'api/Account/GetGlobalRoles', { username: username });
        return await response.json() as Role[];
    }

    loginWithAD = async (): Promise<DataAPI.DataStructures.UserManagement.AuthenticationResult> => {
        const response = await this._sendRequest('GET', 'api/Account/LoginWithAD', {}, undefined, false, false);
        if(!response.ok) {
            if(response.status === 401) {
                // Because the 401 code comes from IIS and not the controller, 
                // no AuthenticationResult object is returned so we have to build it ourselves
                const userNotFoundResult: DataAPI.DataStructures.UserManagement.AuthenticationResult = {
                    isAuthenticated: false,
                    error: AuthenticationErrorType.UserNotFound,
                    accessToken: '',
                    username: ''
                };
                return userNotFoundResult;
            }
            return this._handleError(response);
        }
        const authenticationResult = await response.json() as DataAPI.DataStructures.UserManagement.AuthenticationResult;
        if(authenticationResult.isAuthenticated) {
            this.setAccessToken(authenticationResult.username!, authenticationResult.accessToken!);
        }
        return authenticationResult;
    }

    login = async (username: string, password: string): Promise<DataAPI.DataStructures.UserManagement.AuthenticationResult> => {
        const response = await this._sendRequest('POST', 'api/account/login', {}, { username: username, password: password }, false);
        if(!response.ok && response.status !== 401) {
            return this._handleError(response);
        }
        const authenticationResult = await response.json() as DataAPI.DataStructures.UserManagement.AuthenticationResult;
        if(authenticationResult.isAuthenticated) {
            this.setAccessToken(authenticationResult.username!, authenticationResult.accessToken!);
        }
        return authenticationResult;
    }

    setAccessToken = (username: string, accessToken: string): void => {
        const accessTokenObject = decodeJwtAccessToken(accessToken);
        const expiresEpochSeconds = accessTokenObject.exp;
        const expiresDate = new Date(expiresEpochSeconds*1000);
        const now = new Date();
        const millisecondsToExpiration = expiresDate.getTime() - now.getTime();
        if(millisecondsToExpiration < 0) {
            alert('DataAPI access token has expired. You need to log in again');
            return;
        }

        setCookie('isLoggedIn', 'true', false, expiresDate);
        setCookie('loggedInUsername', username, false, expiresDate);
        setCookie('accessToken', accessToken, false, expiresDate);
        
        const refreshBeforeExpirationInMilliseconds = 5*60*1000;
        if(millisecondsToExpiration < refreshBeforeExpirationInMilliseconds) {
            this._refreshLogin();
        } else {
            setTimeout(this._refreshLogin, millisecondsToExpiration - refreshBeforeExpirationInMilliseconds);
        }
    }

    logout = async (): Promise<void> => {
        removeCookie('isLoggedIn');
        removeCookie('loggedInUsername');
        removeCookie('accessToken');
        return Promise.resolve();
    }

    registerNewUser = async (username: string, firstName: string, lastName: string, password: string, email: string): Promise<void> => {
        const body: DataAPI.DataStructures.UserManagement.RegistrationInformation = {
            firstName: firstName,
            lastName: lastName,
            username: username,
            email: email,
            password: password
        }
        await this._sendRequest('POST', 'api/account/register', {}, body);
    }

    changePassword = async (username: string, password: string): Promise<void> => {
        const body = {
            username: username,
            password: password
        };
        await this._sendRequest('POST', 'api/account/changepassword', {}, body);
    }

    addGlobalRoleToUser = async (username: string, role: Role): Promise<void> => {
        await this._sendRequest('GET', 'api/account/addrole', { username: username, role: role });
    }

    addCollectionRoleToUser = async (username: string, role: Role, dataType: string): Promise<void> => {
        await this._sendRequest('GET', 'api/account/addrole', { username: username, role: role, dataType: dataType });
    }

    setGlobalRolesForUser = async (username: string, roles: Role[]): Promise<void> => {
        await this._sendRequest('GET', 'api/account/setroles', { username: username, roles: roles.join('|') });
    }

    setCollectionRoleForUser = async (username: string, roles: Role[], dataType: string): Promise<void> => {
        await this._sendRequest('GET', 'api/account/setroles', { username: username, roles: roles.join('|'), dataType: dataType });
    }

    removeGlobalRoleFromUser = async (username: string, role: Role): Promise<void> => {
        await this._sendRequest('GET', 'api/account/removerole', { username: username, role: role });
    }

    removeCollectionRoleFromUser = async (username: string, role: Role, dataType: string): Promise<void> => {
        await this._sendRequest('GET', 'api/account/removerole', { username: username, role: role, dataType: dataType });
    }

    deleteUser = async (username: string): Promise<void> => {
        await this._sendRequest('DELETE', 'api/account/deleteuser', { username: username });
    }

    getCollectionPermissions = async (collectionName: string): Promise<DataAPI.DataStructures.CollectionManagement.CollectionUserPermissions[]> => {
        const response = await this._sendRequest('GET', 'api/account/getcollectionpermissions', { collectionName: collectionName });
        return await response.json() as DataAPI.DataStructures.CollectionManagement.CollectionUserPermissions[];
    }

    insert = async (dataType: string, obj: object, id?: string): Promise<string> => {
        return await this._submit(dataType, obj, false, id);
    }

    replace = async (dataType: string, obj: object, id: string): Promise<string> => {
        if(!id) {
            throw new Error("ID must be specified");
        }
        return await this._submit(dataType, obj, true, id);
    }

    _submit = async (dataType: string, obj: object, overwrite: boolean, id?: string): Promise<string> => {
        const body : DataAPI.DataStructures.PostBodies.SubmitDataBody = {
            dataType: dataType,
            id: id,
            overwrite: overwrite,
            data: obj
        };
        const response = await this._sendRequest('POST', '/api/DataIO/Submit', {}, body, false);
        if(response.ok) {
            return await response.text();
        } else if(response.status === 409) {
            throw new ApiError(409, "An object with that ID already exists");
        }
        return await this._handleError(response);
    }

    exists = async (dataType: string, id: string): Promise<boolean> => {
        const response =  await this._sendRequest('GET', '/api/DataIO/Exists', {
            dataType: dataType,
            id: id
        }, undefined, false);
        if(response.status === 200) {
            return true;
        } else if(response.status === 404) {
            return false;
        }
        return await this._handleError(response);
    }

    getById = async <T>(dataType: string, id: string): Promise<T | null> => {
        const response = await this._sendRequest('GET', '/api/DataIO/Get', {
            dataType: dataType,
            id: id
        }, undefined, false);
        if(!response.ok) {
            if(response.status === 404) {
                return null;
            }
            return await this._handleError(response);
        }
        return await response.json() as T;
    }

    getMany = async <T>(dataType: string, whereArguments?: string, orderByArguments?: string, limit?: number): Promise<T[]> => {
        const response = await this._sendRequest('GET', '/api/DataIO/GetMany', {
            dataType: dataType,
            whereArguments: whereArguments,
            orderByArguments: orderByArguments,
            limit: limit?.toString()
        });
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

    firstOrDefault = async <T>(dataType: string, filter: string, orderBy: string): Promise<T | null> => {
        const results = await this.getMany<T>(dataType, filter, orderBy, 1);
        if(results.length > 0) {
            return results[0];
        }
        return null;
    }

    deleteOne = async (dataType: string, id: string): Promise<void> => {
        await this._sendRequest('DELETE', 'api/dataio/delete', { dataType: dataType, id: id });
    }

    deleteMany = async (dataType: string, filter: string): Promise<DataAPI.DataStructures.DataIo.DeleteResult[]> => {
        const response = await this._sendRequest('DELETE', 'api/dataio/deletemany', {
            dataType: dataType,
            whereArguments: filter
        });
        return await response.json() as DataAPI.DataStructures.DataIo.DeleteResult[];
    }

    search = async (query: string): Promise<any[]> => {
        const resultFormat = ResultFormat.Json;
        const body = {
            format: resultFormat,
            query: query
        };
        const response = await this._sendRequest('POST', 'api/dataio/search', {}, body);
        const allLines = await response.text();
        if(allLines.length === 0) {
            return [];
        }
        const lines = allLines.split('\n');
        const results = [];
        for(const lineIndex in lines) {
            const line = lines[lineIndex];
            if(!line) {
                continue;
            }
            try {
                const result = JSON.parse(lines[lineIndex]);
                results.push(result);
            } catch {
                // Ignore parsing erros
            }
        }
        return results;
    }
    
    setDataRedirection = async (dataType: string, dataSourceSystem: string): Promise<void> => {
        await this._sendRequest('GET', 'api/dataio/setredirection', {
            dataType: dataType,
            dataSourceSystem: dataSourceSystem
        });
    }

    setCollectionOptions = async (collectionOptions: DataAPI.DataStructures.CollectionManagement.CollectionOptions): Promise<void> => {
        await this._sendRequest('POST', 'api/dataio/setcollectionoptions', {}, collectionOptions);
    }

    getCollectionInformation = async (collectionName: string): Promise<DataAPI.DataStructures.CollectionInformation> => {
        const response = await this._sendRequest('GET', 'api/dataio/getcollectioninformation', { collectionName: collectionName });
        return await response.json() as DataAPI.DataStructures.CollectionInformation;
    }

    listCollectionNames = async (includeHidden: boolean = false): Promise<string[]> => {
        const response = await this._sendRequest('GET', 'api/dataio/listcollectionnames', {
            includeHidden: includeHidden + ''
        });
        return await response.json() as string[];
    }

    listCollections = async (includeHidden: boolean = false): Promise<DataAPI.DataStructures.CollectionInformation[]> => {
        const response = await this._sendRequest('GET', 'api/dataio/listcollections', {
            includeHidden: includeHidden + ''
        });
        return await response.json() as DataAPI.DataStructures.CollectionInformation[];
    }

    submitValidator = async (validatorDefinition: DataAPI.DataStructures.Validation.ValidatorDefinition, suppressAutoApprove: boolean = false) => {
        const body = {
            validatorDefinition: validatorDefinition,
            suppressAutoApprove: suppressAutoApprove
        };
        await this._sendRequest('POST', 'api/validator/submit', {}, body);
    }

    applyValidator = async (dataType: string, obj: any, validatorId: string): Promise<boolean> => {
        const body = {
            dataType: dataType,
            validatorId: validatorId,
            data: obj
        };
        const response = await this._sendRequest('POST', 'api/validator/apply', {}, body, false);
        if(response.ok) {
            return true;
        }
        if(response.status === 400) {
            return false;
        }
        return this._handleError(response);
    }

    getValidatorDefinition = async (validatorId: string): Promise<DataAPI.DataStructures.Validation.ValidatorDefinition> => {
        const response = await this._sendRequest('GET', 'api/validator/get', {
            validatorId: validatorId
        });
        return await response.json() as DataAPI.DataStructures.Validation.ValidatorDefinition;
    }

    getAllValidatorDefinitions = async (dataType?: string): Promise<DataAPI.DataStructures.Validation.ValidatorDefinition[]> => {
        const response = await this._sendRequest('GET', 'api/validator/getall', dataType ? { dataType: dataType} : {});
        return await response.json() as DataAPI.DataStructures.Validation.ValidatorDefinition[];
    }

    approveValidator = async (validatorId: string): Promise<void> => {
        await this._sendRequest('GET', 'api/validator/approve', { validatorId: validatorId });
    }

    unapproveValidator = async (validatorId: string): Promise<void> => {
        await this._sendRequest('GET', 'api/validator/unapprove', { validatorId: validatorId });
    }

    deleteValidator = async (validatorId: string): Promise<void> => {
        await this._sendRequest('DELETE', 'api/validator/delete', { validatorId: validatorId });
    }

    subscribe = async (dataType: string, modificationTypes: DataModificationType[], filter?: string): Promise<string> => {
        const body = {
            dataType: dataType,
            modificationTypes: modificationTypes,
            filter: filter
        };
        const response = await this._sendRequest('POST', 'api/subscription/subscribe', {}, body);
        return await response.text();
    }

    unsubscribe = async (subscriptionId: string): Promise<void> => {
        await this._sendRequest('GET', 'api/subscription/unsubscribe', { id: subscriptionId });
    }

    unsubscribeAll = async (dataType: string): Promise<void> => {
        await this._sendRequest('GET', 'api/subscription/unsubscribeall', { dataType: dataType });
    }

    getSubscriptions = async (dataType?: string): Promise<DataAPI.DataStructures.DataSubscription.SubscriptionInfo[]> => {
        const response = await this._sendRequest('GET', 'api/subscription/getsubscriptions', dataType ? { dataType: dataType } : {})
        return await response.json() as DataAPI.DataStructures.DataSubscription.SubscriptionInfo[];
    }

    getSubscribedObjects = async (dataType?: string): Promise<DataAPI.DataStructures.DataSubscription.SubscriptionNotification[]> => {
        const response = await this._sendRequest('GET', 'api/subscription/getsubscribedobjects', dataType ? { dataType: dataType } : {});
        return await response.json() as DataAPI.DataStructures.DataSubscription.SubscriptionNotification[];
    }

    deleteNotification = async (notificationId: string): Promise<void> => {
        await this._sendRequest('DELETE', 'api/subscription/deletenotification', { notificationId: notificationId });
    }

    reportTo = async (recipient: string, dataType: string, dataObjectId: string): Promise<void> => {
        await this._sendRequest('GET', 'api/subscription/reportto', {
            recipient: recipient,
            dataType: dataType,
            id: dataObjectId
        });
    }

    getNewId = async (dataType: string): Promise<string> => {
        return (await this.getMultipleNewIds(dataType, 1))[0];
    }

    getMultipleNewIds = async (dataType: string, count: number): Promise<string[]> => {
        const response = await this._sendRequest('GET', 'api/id/getnew', {
            dataType: dataType,
            count: count + ''
        });
        return (await response.text()).split('\n');
    }

    reserveId = async (dataType: string, id: string): Promise<void> => {
        await this._sendRequest('GET', 'api/id/reserve', {
            dataType: dataType,
            id: id
        });
    }

    createDataCollectionProtocol = async (
        protocolName: string, 
        parameters: DataAPI.DataStructures.DataManagement.DataCollectionProtocolParameter[],
        expectedData: DataAPI.DataStructures.DataManagement.DataPlaceholder[]
    ) : Promise<void> => {
        const protocol: DataAPI.DataStructures.DataManagement.DataCollectionProtocol = {
            id: protocolName,
            parameters: parameters,
            expectedData: expectedData
        };
        await this.insert('DataCollectionProtocol', protocol, protocol.id);
    }

    createDataProject = async (
        projectId: string,
        projectSourceSystem: string,
        protocolName: string,
        protocolParameterResponses: { [parameterName: string]: string }
    ) : Promise<void> => {
        const protocol = await this.getById<DataAPI.DataStructures.DataManagement.DataCollectionProtocol>('DataCollectionProtocol', protocolName);
        if(!protocol) {
            throw new Error(`Protocol with name '${protocolName}' doesn't exist`);
        }
        const manadatoryResponses = protocol.parameters.filter(p => p.isMandatory);
        const missingResponses = manadatoryResponses.filter(p => !protocolParameterResponses[p.name]);
        if(missingResponses.length > 0) {
            const missingParameters = missingResponses.map(p => p.name).join(', ');
            throw new Error(`You are missing parameter responses for the following mandatory parameters: ${missingParameters}`);
        }
        const project: DataAPI.DataStructures.DataManagement.DataProject = {
            id: projectId,
            idSourceSystem: projectSourceSystem,
            protocol: protocol,
            parameterResponse: protocolParameterResponses
        };
        await this.insert('DataProject', project, project.id);
    }

    addToDataProject = async (
        projectId: string,
        dataType: string,
        dataObjectId: string,
        derivedData?: DataAPI.DataStructures.DataManagement.DataReference[],
        filename?: string
    ): Promise<void> => {
        if(!await this.exists('DataProject', projectId)) {
            throw new Error(`No data project with ID '${projectId}' exists`)
        }
        const loggedInUsername = this.getLoggedInUsername();
        if(!loggedInUsername) {
            throw new Error('Not logged in!');
        }
        const uploadInfo: DataAPI.DataStructures.DataManagement.DataProjectUploadInfo = {
            id: uuidv4(),
            dataProjectId: projectId,
            uploadTimestamp: new Date(),
            uploaderInitials: loggedInUsername,
            rawData: {
                dataType: dataType,
                id: dataObjectId
            },
            derivedData: derivedData ?? [],
            filename: filename
        };
        await this.insert('DataProjectUploadInfo', uploadInfo, uploadInfo.id);
    }

    addToDataSet = async (
        dataSetId: string,
        dataType: string,
        dataObjectId: string
    ): Promise<void> => {
        if(!await this.exists('DataSet', dataSetId)) {
            const dataSet = {
                id: dataSetId
            };
            await this.insert('DataSet', dataSet, dataSet.id);
        }
        const dataTag: DataAPI.DataStructures.DataManagement.DataTag = {
            id: uuidv4(),
            tagName: dataSetId,
            dataReference: {
                dataType: dataType,
                id: dataObjectId
            }
        };
        await this.insert('DataTag', dataTag, dataTag.id);
    }

    _getAccessToken = () => {
        return getCookieValue('accessToken');
    }

    _refreshLogin = async () => {
        await this.loginWithAD();
    }

    _handleError = async (response: Response) => {
        throw new ApiError(response.status, await response.text());
    }

    _sendRequest = async (
        method: "GET" | "POST" | "PATCH" | "DELETE", 
        resourcePath: string, 
        params?: { [key: string]: string | undefined }, 
        body?: any,
        handleError: boolean = true,
        includeAccessToken: boolean = true
    ): Promise<Response> => {
        const url = buildUrl(this.endpoint, resourcePath, params ?? {});
        const headers: HeadersInit = {
            "Content-Type": "application/json"
        };
        if(includeAccessToken && this.isLoggedIn()) {
            headers["Authorization"] = "Bearer " + this._getAccessToken();
        }
        const options: RequestInit =  {
            method: method,
            headers: headers
        };
        if(body) {
            options.body = typeof body === "string" ? body : JSON.stringify(body)
        }
        const response = await fetch(url, options);
        if(handleError && !response.ok) {
            return await this._handleError(response);
        }
        return response;
    }
}

export const dataApiClient = new DataApiClient("https://localhost:44387/");