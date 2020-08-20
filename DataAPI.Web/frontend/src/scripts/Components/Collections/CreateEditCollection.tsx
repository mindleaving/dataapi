import React, { Component } from 'react';
import Button from 'react-bootstrap/Button';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import { FrontendTypes } from '../../../types/frontend.d';
import EditJsonObject from './EditJsonObject';
import { dataApiClient } from '../../Communication/DataApiClient';

interface CreateEditCollectionProps {}
interface CreateEditCollectionState {
    collectionName: string;
    schema: FrontendTypes.JsonSchemaObject;
    isSaving: boolean;
}

class CreateEditCollection extends Component<CreateEditCollectionProps, CreateEditCollectionState> {

    constructor(props: CreateEditCollectionProps) {
        super(props);

        this.state = { 
            collectionName: '',
            schema: {
                properties: []
            },
            isSaving: false
        };
    }

    onSchemaChanged = (update: FrontendTypes.Update<FrontendTypes.JsonSchemaObject>) => {
        this.setState(state => ({
            schema: update(state.schema)
        }));
    }

    save = async () => {
        this.setState({ isSaving: true });
        try {
            alert('Not implemented');
            //const response = await dataApiClient.submitValidator(this.state.collectionName, this.state.schema);
        } catch(e) {

        } finally {
            this.setState({ isSaving: false });
        }
    }

    render() {
        return (
            <Container>
                <Row>
                    <Col>
                        <Form>
                            <Form.Group>
                                <Form.Label>Collection Name</Form.Label>
                                <Form.Control
                                    type="text"
                                    value={this.state.collectionName}
                                    onChange={(e: any) => this.setState({ collectionName: e.target.value })}
                                />
                            </Form.Group>
                            <EditJsonObject
                                schema={this.state.schema}
                                updateObject={this.onSchemaChanged}
                            />
                            <Button onClick={this.save}>
                            {this.state.isSaving 
                                ? 'Saving...'
                                : 'Save'
                            }
                            </Button>
                        </Form>
                    </Col>
                </Row>
            </Container>
        );
    }

}

export default CreateEditCollection;