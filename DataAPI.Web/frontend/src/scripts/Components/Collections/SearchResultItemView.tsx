import React, { Component } from 'react';
import Accordion from 'react-bootstrap/Accordion';
import Card from 'react-bootstrap/Card';
import JsonObjectView from './JsonObjectView';

interface SearchResultItemViewProps {
    item: any;
    itemIndex: number;
}
interface SearchResultItemViewState {}

class SearchResultItemView extends Component<SearchResultItemViewProps, SearchResultItemViewState> {

    constructor(props: SearchResultItemViewProps) {
        super(props);

        this.state = { };
    }

    render() {
        const item = this.props.item;
        return (
            <Card>
                <Accordion.Toggle as={Card.Header} eventKey={this.props.itemIndex + ''}>
                    {item.id ?? item._id ?? "(no ID)"}
                </Accordion.Toggle>
                <Accordion.Collapse eventKey={this.props.itemIndex + ''}>
                    <Card.Body>
                        <JsonObjectView obj={this.props.item} />
                    </Card.Body>
                </Accordion.Collapse>
            </Card>
        );
    }

}

export default SearchResultItemView;