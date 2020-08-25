import React, { Component, FormEvent } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import JsonSchemaForm, { ISubmitEvent } from '@rjsf/core';
import { v4 as uuidv4 } from 'uuid';
import { DataAPI } from '../../types/dataApiDataStructures';
import { JSONSchema7 } from 'json-schema';
import { dataApiClient } from '../Communication/DataApiClient';
import { ValidatorType } from '../../types/dataApiDataStructuresEnums';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/esm/Button';

interface CreateEditDataObjectPageParams {
    dataType: string;
    id: string;
}
interface CreateEditDataObjectPageProps extends RouteComponentProps<CreateEditDataObjectPageParams> {

}
interface CreateEditDataObjectPageState {
    isNewData: boolean;
    dataType: string;
    id: string;
    isLoading: boolean;
    isSaving: boolean;
    validators: DataAPI.DataStructures.Validation.ValidatorDefinition[];
    schema: JSONSchema7 | null;
    rawJson: string;
}

class CreateEditDataObjectPage extends Component<CreateEditDataObjectPageProps, CreateEditDataObjectPageState> {

    constructor(props: CreateEditDataObjectPageProps) {
        super(props);

        const id = props.match.params.id;
        this.state = {
            isNewData: id === undefined,
            dataType: props.match.params.dataType,
            id: id,
            isLoading: true,
            isSaving: false,
            validators: [],
            schema: null,
            rawJson: '{\n\t\n}'
        };
    }

    async componentDidMount() {
        this.setState({ isLoading: true })
        try {
            await this.loadValidators();
            if (!this.state.isNewData) {
                await this.loadData();
            }
        } catch {

        } finally {
            this.setState({ isLoading: false })
        }
    }

    loadValidators = async () => {
        const collectionInformation = await dataApiClient.getCollectionInformation(this.state.dataType);
        const jsonSchemas = collectionInformation.validatorDefinitions
            .filter(x => x.validatorType === ValidatorType.JsonSchema)
            .map(x => JSON.parse(x.ruleset!));
        let mergedSchema: JSONSchema7 | null = null;
        if(jsonSchemas.length > 0) {
            mergedSchema = {
                allOf: jsonSchemas
            };
        }
        this.setState({
            validators: collectionInformation.validatorDefinitions,
            schema: mergedSchema
        });
    }

    loadData = async () => {
        const data = await dataApiClient.getById(this.state.dataType, this.state.id);
        this.setState({
            rawJson: JSON.stringify(data)
        });
    }

    saveRawJson = async (e: FormEvent) => {
        e.preventDefault();
        this.setState({ isSaving: true });
        try {
            const obj = JSON.parse(this.state.rawJson);
            const id = this.state.id ?? obj.id ?? obj.Id ?? obj.ID ?? uuidv4();
            await dataApiClient.replace(this.state.dataType, obj, id);
            this.props.history.push(`/explore/collections/${this.state.dataType}`);
        } catch(e) {
            alert('Could not save: ' + e.message);
        } finally {
            this.setState({ isSaving: false });
        }
    }

    saveJsonSchemaForm = async (e: ISubmitEvent<unknown>) => {
        this.setState({ isSaving: true });
        try {

        } catch {

        } finally {
            this.setState({ isSaving: false });
        }
    }

    render() {
        let form = null;
        if(this.state.schema) {
            form = (
                <JsonSchemaForm
                    schema={this.state.schema}
                    onSubmit={this.saveJsonSchemaForm}
                />
            );
        } else {
            form = (
                <Form onSubmit={this.saveRawJson}>
                    <Form.Group>
                        <Form.Label>Raw JSON:</Form.Label>
                        <Form.Control
                            as="textarea"
                            style={{ height: '240px' }}
                            value={this.state.rawJson}
                            onChange={(e: any) => this.setState({ rawJson: e.target.value })}
                        />
                    </Form.Group>
                    <Button
                        type="submit"
                    >
                        {this.state.isSaving 
                            ? 'Saving...'
                            : 'Save' 
                        }
                    </Button>
                </Form>
            );
        }
        return (
            <Container>
                <Row>
                    <Col>
                        <h1>
                            {this.state.isNewData 
                            ? `Create data for collection '${this.state.dataType}'` 
                            : `Edit ID '${this.state.id}' in collection '${this.state.dataType}'`}
                        </h1>
                    </Col>
                </Row>
                <Row>
                    <Col>
                        {form}
                    </Col>
                </Row>
            </Container>
        )
    }

}

export default CreateEditDataObjectPage;