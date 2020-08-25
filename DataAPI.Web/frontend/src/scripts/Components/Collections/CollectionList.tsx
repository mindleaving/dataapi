import React, { Component } from 'react';
import { NotificationManager } from 'react-notifications';
import ListGroup from 'react-bootstrap/ListGroup';
import CollectionListItem from './CollectionListItem';
import { dataApiClient } from '../../Communication/DataApiClient';
import { DataAPI } from '../../../types/dataApiDataStructures.d';

interface CollectionListProps {
    onCollectionSelected: (collectionName: string) => void;
    editCollection: (collectionName: string) => void;
}
interface CollectionListState {
    includeHidden: boolean;
    collections: DataAPI.DataStructures.CollectionInformation[];
}

class CollectionList extends Component<CollectionListProps, CollectionListState> {

    constructor(props: CollectionListProps) {
        super(props);

        this.state = { 
            includeHidden: false,
            collections: []
        };
    }

    async componentDidMount() {
        await this.loadCollections();
    }

    loadCollections = async () => {
        try {
            const collectionInfos = await dataApiClient.listCollections(this.state.includeHidden);
            this.setState({ collections: collectionInfos });
        } catch(e) {
            NotificationManager.error('Could not load collections', '', 5000);
        }
    }

    render() {
        return (
        <>
            <ListGroup className="collection-list">
                {this.state.collections.map(collection => 
                    <CollectionListItem
                        key={collection.collectionName}
                        collection={collection}
                        onCollectionSelected={this.props.onCollectionSelected}
                        editCollection={this.props.editCollection}
                    />
                )}
            </ListGroup>
        </>
        );
    }

}

export default CollectionList;