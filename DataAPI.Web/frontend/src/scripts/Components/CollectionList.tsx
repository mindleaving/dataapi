import React, { Component } from 'react';
import { NotificationManager } from 'react-notifications';
import ListGroup from 'react-bootstrap/ListGroup';
import CollectionListItem from './Collections/CollectionListItem';
import { DataAPI } from '../../types/dataApiDataStructures.d';

interface CollectionListProps {
    onCollectionSelected: (collectionName: string) => void;
}
interface CollectionListState {
    collections: DataAPI.DataStructures.CollectionInformation[];
}

class CollectionList extends Component<CollectionListProps, CollectionListState> {

    constructor(props: CollectionListProps) {
        super(props);

        this.state = { 
            collections: []
        };
    }

    async componentDidMount() {
        await this.loadCollections();
    }

    loadCollections = async () => {
        try {
            throw new Error("Not implemented");
            
        } catch(e) {
            NotificationManager.error('Could not load collections', '', 5000);
        }
    }

    render() {
        return (
        <>
            <ListGroup>
                {this.state.collections.map(collection => 
                    <CollectionListItem
                        collection={collection}
                    />
                )}
            </ListGroup>
        </>
        );
    }

}

export default CollectionList;