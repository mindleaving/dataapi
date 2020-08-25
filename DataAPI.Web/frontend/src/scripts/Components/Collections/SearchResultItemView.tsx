import React from 'react';
import Accordion from 'react-bootstrap/Accordion';
import Card from 'react-bootstrap/Card';
import JsonObjectView from './JsonObjectView';
import { Link } from 'react-router-dom';
import Button from 'react-bootstrap/Button';
import { confirmAlert } from 'react-confirm-alert';
import { dataApiClient } from '../../Communication/DataApiClient';

interface SearchResultItemViewProps {
    collectionName: string;
    item: any;
    itemIndex: number;
    onDeleted?: (dataType: string, id: string) => void;
}

const SearchResultItemView = (props: SearchResultItemViewProps) => {

    const item = props.item;
    const id = item._id;

    const deleteEntry = async (dataType: string, id: string, force: boolean = false) => {
        if(!force) {
            confirmDelete(dataType, id);
            return;
        }
        try {
            await dataApiClient.deleteOne(dataType, id);
            if(props.onDeleted) {
                props.onDeleted(dataType, id);
            }
        } catch(e) {
            alert('Could not delete: ' + e.message);
        }
    }

    const confirmDelete = (dataType: string, id: string) => {
        confirmAlert({
            'title': 'Confirm delete data entry',
            'message': `Are you sure you want to delete entry with ID '${id}' from collection '${dataType}'?`,
            buttons: [
                {
                    label: 'Yes, delete!',
                    onClick: () => deleteEntry(dataType, id, true)
                }, 
                {
                    label: 'No, abort!',
                    onClick: () => {}
                }
            ]
        })
    }

    return (
        <Card>
            <Accordion.Toggle as={Card.Header} eventKey={props.itemIndex + ''}>
                <span>{item._id ?? item.id ?? item.Id ?? item.ID ?? "(no ID)"}</span>
                {id ? 
                <div className="float-right">
                    <Link to={`/edit/${encodeURIComponent(props.collectionName)}/${encodeURIComponent(id)}`}>Edit</Link>
                    <Button className="mx-2" variant="danger" size="sm" onClick={(e: any) => { e.stopPropagation(); confirmDelete(props.collectionName, id)}}>X</Button>
                </div> : null
                }
            </Accordion.Toggle>
            <Accordion.Collapse eventKey={props.itemIndex + ''}>
                <Card.Body>
                    <JsonObjectView obj={props.item} />
                </Card.Body>
            </Accordion.Collapse>
        </Card>
    );

}

export default SearchResultItemView;