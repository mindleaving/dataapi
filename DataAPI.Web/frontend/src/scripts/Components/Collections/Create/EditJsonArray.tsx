import React, { useState } from 'react';
import Form from 'react-bootstrap/Form';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import JsonPropertyTypeSelector from './JsonPropertyTypeSelector';
import { FrontendTypes } from '../../../../types/frontend';
import { JsonSchemaPropertyType } from '../../../../types/frontendEnums';
import EditJsonObject from './EditJsonObject';
import { confirmAlert } from 'react-confirm-alert';
import Button from 'react-bootstrap/esm/Button';

interface EditJsonArrayProps {
    itemType: FrontendTypes.JsonSchemaArrayItem;
    updateItemType: (update: FrontendTypes.Update<FrontendTypes.JsonSchemaArrayItem>) => void;
}

const EditJsonArray = (props: EditJsonArrayProps) => {
    const [ showSubSchema, setShowSubSchema ] = useState(true);

    const confirmTypeChange = (type: JsonSchemaPropertyType) => {
        confirmAlert({
            title: 'Confirm type change',
            message: `Changing the type to '${type}' will result in permanent loss of the current item schema. Change anyway?`,
            buttons: [
                {
                    label: 'Yes, change type',
                    onClick: () => onItemTypeChanged(type, true)
                },
                {
                    label: 'No, abort',
                    onClick: () => {}
                }
            ]
        })
    }

    const onItemTypeChanged = (type: JsonSchemaPropertyType, force: boolean = false) => {
        if(props.itemType.type === type) {
            return;
        }
        const typesNeedingConfirmation = [
            JsonSchemaPropertyType.array,
            JsonSchemaPropertyType.object
        ];
        if(typesNeedingConfirmation.includes(props.itemType.type) && !force) {
            confirmTypeChange(type);
            return;
        }
        let itemType: FrontendTypes.JsonSchemaArrayItem | undefined = undefined;
        let objectSchema: FrontendTypes.JsonSchemaObject | undefined = undefined;
        if(type === JsonSchemaPropertyType.array) {
            itemType = {
                type: JsonSchemaPropertyType.string
            };
        } else if(type === JsonSchemaPropertyType.object) {
            objectSchema = {
                properties: []
            };
        }
        props.updateItemType(item => ({ 
            ...item, 
            type: type,
            itemType: itemType,
            objectSchema: objectSchema
        }));
    }

    let typeSpecificFormControls = null;
    if(props.itemType.type === JsonSchemaPropertyType.array) {
        typeSpecificFormControls = (
            <EditJsonArray
                itemType={props.itemType.itemType!}
                updateItemType={(update) => props.updateItemType(item => ({ ...item, itemType: update(item.itemType!) }) )}
            />
        );
    } else if(props.itemType.type === JsonSchemaPropertyType.object) {
        typeSpecificFormControls = (
            <EditJsonObject
                schema={props.itemType.objectSchema!}
                updateObject={(update) => props.updateItemType(item => ({ ...item, objectSchema: update(item.objectSchema!) }))}
            />
        );
    }

    return (
        <>
            <Form.Group as={Row}>
                <Form.Label column>of type</Form.Label>
                <Col>
                    <JsonPropertyTypeSelector
                        value={props.itemType.type}
                        onChange={onItemTypeChanged}
                    />
                    {typeSpecificFormControls !== null 
                        ? <Button variant="link" size="sm" onClick={() => setShowSubSchema(!showSubSchema)}>{showSubSchema ? 'Collapse' : 'Expand'}</Button>
                        : null
                    }
                </Col>
            </Form.Group>
            {typeSpecificFormControls !== null ?
            <Row>
                <Col>
                    {showSubSchema ?
                    <div className="ml-3 pl-3 pb-2 border-left border-bottom border-warning">
                        {typeSpecificFormControls}
                    </div> : null}
                </Col>
            </Row> : null}
        </>
    );
}

export default EditJsonArray;