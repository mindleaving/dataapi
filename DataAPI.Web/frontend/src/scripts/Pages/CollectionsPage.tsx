import React, { Component } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import CollectionList from './../Components/Collections/CollectionList';
import Button from 'react-bootstrap/Button';
import CollectionExplorer from '../Components/Collections/CollectionExplorer';

interface CollectionsPageProps extends RouteComponentProps<any> {
}
interface CollectionsPageState {
    selectedCollection: string | null;
}

class CollectionsPage extends Component<CollectionsPageProps, CollectionsPageState> {

    constructor(props: CollectionsPageProps) {
        super(props);

        this.state = { 
            selectedCollection: null
        };
    }

    onCollectionSelected = (collectionName: string) => {
        this.setState({ selectedCollection: collectionName });
    }

    render() {
        return (
        <>
            <Container>
                <Row>
                    <Col>
                        <h1>Collections</h1>
                    </Col>
                </Row>
                <Row>
                    <Col md={'auto'}>
                        <div style={{ width: '300px', marginBottom: '30px' }}>
                            <CollectionList 
                                onCollectionSelected={this.onCollectionSelected}
                                editCollection={(collectionName) => this.props.history.push(`/explore/collections/edit/${collectionName}`)}
                            />
                            <Button className="mt-3" onClick={() => this.props.history.push("/explore/collections/new")}>+ Create collection</Button>
                        </div>
                    </Col>
                    <Col>
                        {this.state.selectedCollection !== null ?
                        <CollectionExplorer
                            key={this.state.selectedCollection}
                            collectionName={this.state.selectedCollection}
                        /> : null}
                    </Col>
                </Row>
            </Container>
        </>
        );
    }

}

export default CollectionsPage;