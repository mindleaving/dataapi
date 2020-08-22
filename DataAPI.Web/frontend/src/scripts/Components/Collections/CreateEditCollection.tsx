import React, { Component } from 'react';
import Button from 'react-bootstrap/Button';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import { FrontendTypes } from '../../../types/frontend.d';
import EditJsonObject from './EditJsonObject';
import { v4 as uuidv4 } from 'uuid';
import '../../../styles/createEditCollection.css';
import { JsonSchemaPropertyType } from '../../../types/frontendEnums';
import JsonSchemaForm from '@rjsf/core';
import { convertJsonSchemaObject } from '../../Helpers/JsonSchemaBuilder';

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
                properties: [
                    {
                        guid: uuidv4(),
                        name: '',
                        type: JsonSchemaPropertyType.string,
                        isMandatory: false
                    }
                ]
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
                        <h1>Create/edit collection</h1>
                        <Form className="needs-validation was-validated">
                            <Form.Group>
                                <Form.Label><b>Collection Name</b></Form.Label>
                                <Form.Control
                                    type="text"
                                    value={this.state.collectionName}
                                    onChange={(e: any) => this.setState({ collectionName: e.target.value })}
                                />
                            </Form.Group>
                            <div className="mb-2 border border-dark p-3" style={{ maxWidth: '1000px' }}>
                                <h3>Schema</h3>
                                <EditJsonObject
                                    schema={this.state.schema}
                                    updateObject={this.onSchemaChanged}
                                />
                            </div>
                            <Button onClick={this.save}>
                            {this.state.isSaving 
                                ? 'Saving...'
                                : 'Save'
                            }
                            </Button>
                        </Form>
                    </Col>
                </Row>
                <Row>
                    <Col>
                        <JsonSchemaForm schema={convertJsonSchemaObject(this.state.schema)} />
                    </Col>
                </Row>
            </Container>
        );
    }

}

export default CreateEditCollection;