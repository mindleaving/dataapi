import React from 'react';
import Form from 'react-bootstrap/Form';
import EditJsonObject from './EditJsonObject';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import { ValidatorType } from '../../../../types/dataApiDataStructuresEnums';
import { FrontendTypes } from '../../../../types/frontend';
import TextRulesView from './TextRulesView';
import Button from 'react-bootstrap/esm/Button';

interface ValidatorViewProps {
    validator: FrontendTypes.CollectionValidator;
    updateValidator: (update: FrontendTypes.Update<FrontendTypes.CollectionValidator>) => void;
    deleteValidator: (validatorId: string, existsInDataAPI: boolean) => void | Promise<void>;
}

const ValidatorView = (props: ValidatorViewProps) => {

    return (
        <div className="border p-3 my-2">
            <Button 
                className="float-right"
                variant="danger" 
                size="sm" 
                onClick={() => props.deleteValidator(props.validator.id, props.validator.existsInDataAPI)}
            >
                    X
            </Button>
            <Form.Group as={Row}>
                <Form.Label column xs={4}>Validator type:</Form.Label>
                <Col xs={4}>
                    <Form.Control
                        as="select"
                        value={props.validator.validatorType}
                        onChange={(e: any) => {
                            const validatorType = e.target.value as ValidatorType;
                            props.updateValidator(v => ({ ...v, validatorType: validatorType }))
                        }}
                    >
                        <option value={ValidatorType.TextRules}>{ValidatorType.TextRules}</option>
                        <option value={ValidatorType.JsonSchema}>{ValidatorType.JsonSchema}</option>
                    </Form.Control>
                </Col>
            </Form.Group>
            {props.validator.validatorType === ValidatorType.JsonSchema ?
                <EditJsonObject
                    schema={props.validator.schema}
                    updateObject={update => props.updateValidator(v => ({ ...v, schema: update(v.schema) }))}
                /> : null
            }
            {props.validator.validatorType === ValidatorType.TextRules ?
                <TextRulesView 
                    rules={props.validator.rules}
                    onTextRulesChanged={rules => props.updateValidator(v => ({ ...v, rules: rules }))}
                /> : null
            }
        </div>
    );

}

export default ValidatorView;