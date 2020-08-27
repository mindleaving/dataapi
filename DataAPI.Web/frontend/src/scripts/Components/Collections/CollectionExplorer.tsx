import React, { Component } from 'react';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import QueryEditor from './QueryEditor';
import SearchResultView from './SearchResultView';
import { dataApiClient } from '../../Communication/DataApiClient';
import { Link } from 'react-router-dom';
import { NotificationManager } from 'react-notifications';

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
            NotificationManager.error(e.message, 'Search failed', 3000);
        } finally {
            this.setState({ isRunningQuery: false });
        }
    }

    onEntryDeleted = (dataType: string, id: string) => {
        this.setState(state => ({
            loadedItems: state.loadedItems?.filter(x => x._id !== id) ?? null
        }));
    }

    render() {
        return (
        <>
            <Row className="mb-3">
                <Col>
                    <h2>{this.props.collectionName}</h2>
                </Col>
            </Row>
            <Row>
                <Col>
                    <Link to={`/edit/${this.props.collectionName}`}>+ Add entry</Link>
                </Col>
            </Row>
            <Row>
                <Col>
                    {this.state.loadedItems ?
                        <SearchResultView
                            collectionName={this.props.collectionName}
                            items={this.state.loadedItems}
                            onEntryDeleted={this.onEntryDeleted}
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