import React, { Component, FormEvent } from 'react';
import Button from 'react-bootstrap/Button';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import { FrontendTypes } from '../../types/frontend';
import { v4 as uuidv4 } from 'uuid';
import { convertJsonSchemaObject, convertJsonSchemaToFrontend } from '../Helpers/JsonSchemaBuilder';
import { dataApiClient } from '../Communication/DataApiClient';
import { DataAPI } from '../../types/dataApiDataStructures';
import { ValidatorType } from '../../types/dataApiDataStructuresEnums';
import { RouteComponentProps } from 'react-router-dom';
import ValidatorView from '../Components/Collections/Create/ValidatorView';
import { JsonSchemaPropertyType } from '../../types/frontendEnums';
import { confirmAlert } from 'react-confirm-alert';
import { NotificationManager } from 'react-notifications';

interface CreateEditCollectionParams {
    collectionName?: string;
}
interface CreateEditCollectionProps extends RouteComponentProps<CreateEditCollectionParams> {
}
interface CreateEditCollectionState {
    isNewCollection: boolean;
    collectionName: string;
    validators: FrontendTypes.CollectionValidator[];
    knownCollectionNames: string[];
    isSaving: boolean;
}

class CreateEditCollectionPage extends Component<CreateEditCollectionProps, CreateEditCollectionState> {

    constructor(props: CreateEditCollectionProps) {
        super(props);

        const collectionName = props.match.params.collectionName;
        this.state = { 
            isNewCollection: collectionName === undefined,
            collectionName: collectionName ?? '',
            validators: [],
            knownCollectionNames: [],
            isSaving: false
        };
    }

    async componentDidMount() {
        await this.loadKnownCollectionNames();
        if(!this.state.isNewCollection) {
            await this.loadCollectionInformation(this.state.collectionName);
        }
    }

    loadKnownCollectionNames = async () => {
        const collectionNames = await dataApiClient.listCollectionNames(true);
        this.setState({ knownCollectionNames: collectionNames });
    }

    loadCollectionInformation = async (collectionName: string) => {
        const collectionInformation = await dataApiClient.getCollectionInformation(collectionName);
        this.setState({
            validators: collectionInformation.validatorDefinitions.map(this.convertToFrontendValidator)
        });
    }

    convertToFrontendValidator = (validatorDefinition: DataAPI.DataStructures.Validation.ValidatorDefinition) => {
        const frontendValidator: FrontendTypes.CollectionValidator = {
            id: validatorDefinition.id,
            existsInDataAPI: true,
            validatorType: validatorDefinition.validatorType,
            rules: validatorDefinition.validatorType === ValidatorType.TextRules 
                ? validatorDefinition.ruleset ?? '' 
                : '',
            schema: validatorDefinition.validatorType === ValidatorType.JsonSchema 
                ? convertJsonSchemaToFrontend(JSON.parse(validatorDefinition.ruleset ?? '{}')) 
                : this.createEmptySchema()
        }
        return frontendValidator;
    }

    addValidator = () => {
        const newValidator: FrontendTypes.CollectionValidator = {
            id: uuidv4(),
            existsInDataAPI: false,
            validatorType: ValidatorType.JsonSchema,
            rules: '',
            schema: this.createEmptySchema()
        }
        this.setState(state => ({
            validators: state.validators.concat([ newValidator ])
        }))
    }
    
    createEmptySchema = () => {
        return {
            properties: [
                {
                    guid: uuidv4(),
                    name: '',
                    type: JsonSchemaPropertyType.string,
                    isMandatory: false
                }
            ]
        };
    }

    updateValidator = (validatorId: string, update: FrontendTypes.Update<FrontendTypes.CollectionValidator>) => {
        this.setState(state => ({
            validators: state.validators.map(validator => {
                if(validator.id === validatorId) {
                    return update(validator);
                } else {
                    return validator;
                }
            })
        }));
    }

    confirmDeleteValidator = (validatorId: string, existsInDataAPI: boolean) => {
        confirmAlert({
            title: 'Confirm delete validator',
            message: 'Deleting this validator cannot be undone. Are you sure you want to delete it?',
            buttons: [
                {
                    label: 'Yes, delete!',
                    onClick: () => this.deleteValidator(validatorId, existsInDataAPI, true)
                },
                {
                    label: 'No, abort!',
                    onClick: () => {}
                }
            ]
        })
    }

    deleteValidator = async (validatorId: string, existsInDataAPI: boolean, force: boolean = false) => {
        if(existsInDataAPI && !force) {
            this.confirmDeleteValidator(validatorId, existsInDataAPI);
            return;
        }
        try {
            if(existsInDataAPI) {
                await dataApiClient.deleteValidator(validatorId);
                NotificationManager.success('', 'Validator deleted!', 3000);
            }
        } finally {
            this.setState(state => ({
                validators: state.validators.filter(x => x.id !== validatorId)
            }));
        }
    }

    save = async (e: FormEvent) => {
        e.preventDefault();
        this.setState({ isSaving: true });
        try {
            const loggedInUsername = dataApiClient.getLoggedInUsername();
            if(!loggedInUsername) {
                NotificationManager.error('', 'Not logged in!', 3000);
                return;
            }
            if(this.state.isNewCollection) {
                // Trigger creation of collection
                const id = await dataApiClient.insert(this.state.collectionName, { });
                await dataApiClient.deleteOne(this.state.collectionName, id);
            }
            for (let index = 0; index < this.state.validators.length; index++) {
                const validator = this.state.validators[index];
                let ruleset;
                if(validator.validatorType === ValidatorType.TextRules) {
                    ruleset = validator.rules;
                } else if (validator.validatorType === ValidatorType.JsonSchema) {
                    const schema = convertJsonSchemaObject(validator.schema);
                    ruleset = JSON.stringify(schema);
                } else {
                    throw new Error(`Unsupported validator type '${validator.validatorType}'`);
                }
                const validatorDefinition: DataAPI.DataStructures.Validation.ValidatorDefinition = {
                    id: validator.id,
                    dataType: this.state.collectionName,
                    isApproved: false,
                    ruleset: ruleset,
                    submitter: loggedInUsername,
                    submitterEmail: '', //TODO
                    validatorType: validator.validatorType
                };
                await dataApiClient.submitValidator(validatorDefinition, false);
            }
            NotificationManager.success('', `Collection '${this.state.collectionName}' created`, 3000);
            this.props.history.push('/explore/collections');
        } catch(e) {
            NotificationManager.error(`Could not create collection: ${e.message}`, 'Error', 3000);
        } finally {
            this.setState({ isSaving: false });
        }
    }

    render() {
        return (
            <Container className="mt-2">
                <Row>
                    <Col>
                        <Form className="needs-validation was-validated" onSubmit={this.save}>
                            {this.state.isNewCollection 
                                ? <>
                                    <h1>Create new collection</h1>
                                    <Form.Group>
                                        <Form.Label><b>Collection Name</b></Form.Label>
                                        <Form.Control required
                                            type="text"
                                            value={this.state.collectionName}
                                            onChange={(e: any) => this.setState({ collectionName: e.target.value })}
                                            disabled={!this.state.isNewCollection}
                                            isInvalid={this.state.isNewCollection && this.state.knownCollectionNames.includes(this.state.collectionName)}
                                        />
                                        <Form.Control.Feedback type="invalid">A collection with this name already exists</Form.Control.Feedback>
                                    </Form.Group>
                                </> 
                                : <h1>Edit collection '{this.state.collectionName}'</h1>
                            }
                            <div className="mb-2 border border-dark p-3" style={{ maxWidth: '1000px' }}>
                                <h3>Validators</h3>
                                {this.state.validators.map(validator => (
                                    <ValidatorView
                                        key={validator.id}
                                        validator={validator} 
                                        updateValidator={(update) => this.updateValidator(validator.id, update)}
                                        deleteValidator={this.deleteValidator}
                                    />
                                ))}
                                <Button className="mt-3" onClick={this.addValidator}>+ Add validator</Button>
                            </div>
                            <Button type="submit" className="mt-3">
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

export default CreateEditCollectionPage;