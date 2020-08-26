import React, { Component } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import CollectionList from './../Components/Collections/CollectionList';
import Button from 'react-bootstrap/Button';
import CollectionExplorer from '../Components/Collections/CollectionExplorer';

interface CollectionsPageParams {
    selectedCollection?: string;
}
interface CollectionsPageProps extends RouteComponentProps<CollectionsPageParams> {
}
interface CollectionsPageState {
    selectedCollection: string | null;
}

class CollectionsPage extends Component<CollectionsPageProps, CollectionsPageState> {

    constructor(props: CollectionsPageProps) {
        super(props);

        this.state = { 
            selectedCollection: this.props.match.params.selectedCollection ?? null
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
                        <Button className="mt-1" onClick={() => this.props.history.push("/explore/collections/new")}>+ Create collection</Button>

                        <div style={{ width: '420px' }}>{/* Placeholder for sizing column */}</div>
                        <div style={{ 
                            width: '400px',
                            position: 'fixed', 
                            bottom: '20px', 
                            top: '160px',
                            left: '0', 
                            zIndex: 1,
                            overflowX: 'hidden',
                            overflowY: 'auto',
                            paddingTop: '20px',
                            paddingBottom: '20px',
                            paddingLeft: '30px',
                            paddingRight: '20px'
                        }}>
                            <CollectionList 
                                onCollectionSelected={this.onCollectionSelected}
                                editCollection={(collectionName) => this.props.history.push(`/explore/collections/edit/${collectionName}`)}
                            />
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