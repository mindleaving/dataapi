import { dataApiClient } from './DataApiClient';

interface DataProject {
    id: string;
}

export const autocomplete = async (searchText: string) => {
    return await dataApiClient.getMany('DataProject', `_id LIKE '%${searchText}%'`);
}

export const getById = async (id: string) => {
    return await dataApiClient.getById<DataProject>('DataProject', id);
}