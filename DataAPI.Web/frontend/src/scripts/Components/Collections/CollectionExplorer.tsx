import React, { Component } from 'react';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import QueryEditor from './QueryEditor';
import SearchResultView from './SearchResultView';
import { dataApiClient } from '../../Communication/DataApiClient';

interface CollectionExplorerProps {
    collectionName: string;
}
interface CollectionExplorerState {
    isRunningQuery: boolean;
    loadedItems: any[] | null;
}

class CollectionExplorer extends Component<CollectionExplorerProps, CollectionExplorerState> {

    constructor(props: CollectionExplorerProps) {
        super(props);

        this.state = { 
            isRunningQuery: false,
            loadedItems: null
        };
    }

    runQuery = async (query: string) => {
        this.setState({ isRunningQuery: true });
        try {
            const results = await dataApiClient.search(query);
            this.setState({ loadedItems: results });
        } catch(e) {
            alert('Search failed: ' + e.message);
        } finally {
            this.setState({ isRunningQuery: false });
        }
    }

    render() {
        return (
        <>
            <Row className="mb-3">
                <h2>{this.props.collectionName}</h2>
            </Row>
            <Row>
                <Col>
                    {this.state.loadedItems ?
                        <SearchResultView
                            items={this.state.loadedItems}
                        />
                        : <i>Edit query below and click 'Run' to search for items</i>
                    }
                </Col>
            </Row>
            <Row className="mt-3 py-2 border-top">
                <QueryEditor 
                    collectionName={this.props.collectionName}
                    onRunQuery={this.runQuery}
                    isRunningQuery={this.state.isRunningQuery}
                />
            </Row>
        </>
        );
    }

}

export default CollectionExplorer;