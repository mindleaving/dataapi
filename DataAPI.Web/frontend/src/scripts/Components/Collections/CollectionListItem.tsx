import React, { Component } from 'react';
import ListGroup from 'react-bootstrap/ListGroup';
import { DataAPI } from '../../../types/dataApiDataStructures.d';
import Button from 'react-bootstrap/Button';

interface CollectionListItemProps {
    collection: DataAPI.DataStructures.CollectionInformation;
    onCollectionSelected: (collectionName: string) => void;
    editCollection: (collectionName: string) => void;
}
interface CollectionListItemState {}

class CollectionListItem extends Component<CollectionListItemProps, CollectionListItemState> {

    constructor(props: CollectionListItemProps) {
        super(props);

        this.state = { };
    }

    render() {
        const collection = this.props.collection;
        return (
            <ListGroup.Item onClick={() => this.props.onCollectionSelected(collection.collectionName)}>
                <b>{collection.collectionName}</b>
                <Button className="float-right" variant="link" size="sm" onClick={() => this.props.editCollection(collection.collectionName)}>Edit</Button>
            </ListGroup.Item>
        );
    }

}

export default CollectionListItem;