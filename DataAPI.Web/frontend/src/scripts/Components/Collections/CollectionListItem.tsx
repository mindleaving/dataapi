import React, { Component } from 'react';
import ListGroup from 'react-bootstrap/ListGroup';
import { DataAPI } from '../../../types/dataApiDataStructures.d';

interface CollectionListItemProps {
    collection: DataAPI.DataStructures.CollectionInformation;
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
            <ListGroup.Item>
                <b>{collection.collectionName}</b>
            </ListGroup.Item>
        );
    }

}

export default CollectionListItem;