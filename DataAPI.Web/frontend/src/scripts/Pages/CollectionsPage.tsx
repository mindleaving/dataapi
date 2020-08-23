import React, { Component } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import CollectionList from './../Components/Collections/CollectionList';
import Button from 'react-bootstrap/Button';

interface CollectionsPageProps extends RouteComponentProps<any> {
}
interface CollectionsPageState {}

class CollectionsPage extends Component<CollectionsPageProps, CollectionsPageState> {

    constructor(props: CollectionsPageProps) {
        super(props);

        this.state = { };
    }

    onCollectionSelected = (collectionName: string) => {

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
                    <Col sm={3}>
                        <CollectionList 
                            onCollectionSelected={this.onCollectionSelected}
                        />
                        <Button onClick={() => this.props.history.push("/collections/new")}>+ Create collection</Button>
                    </Col>
                    <Col sm={9}>
                    </Col>
                </Row>
            </Container>
        </>
        );
    }

}

export default CollectionsPage;