import React from 'react';
import Form from 'react-bootstrap/Form';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import { FrontendTypes } from '../../../types/frontend.d';
import { JsonSchemaPropertyType } from '../../../types/frontendEnums';
import EditJsonArray from './EditJsonArray';
import JsonPropertyTypeSelector from './JsonPropertyTypeSelector';
import EditJsonObject from './EditJsonObject';
import { confirmAlert } from 'react-confirm-alert';

interface EditJsonPropertyProps {
    property: FrontendTypes.JsonSchemaProperty;
    updateProperty: (update: FrontendTypes.Update<FrontendTypes.JsonSchemaProperty>) => void;
}

const EditJsonProperty = (props: EditJsonPropertyProps) => {

    const confirmTypeChange = (type: JsonSchemaPropertyType) => {
        confirmAlert({
            title: 'Confirm type change',
            message: `Changing the type to '${type}' will result in permanent loss of the properties schema. Change anyway?`,
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
        if(props.property.type === type) {
            return;
        }
        const typesNeedingConfirmation = [
            JsonSchemaPropertyType.array,
            JsonSchemaPropertyType.object
        ];
        if(typesNeedingConfirmation.includes(props.property.type) && !force) {
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
        props.updateProperty(p => ({ 
            ...p, 
            type: type,
            itemType: itemType,
            objectSchema: objectSchema
        }));
    }

    const property = props.property;
    let typeSpecificFormControls = null;
    if(property.type === JsonSchemaPropertyType.array) {
        typeSpecificFormControls = (
            <EditJsonArray
                itemType={property.itemType!}
                updateItemType={(update) => props.updateProperty(p => ({ ...p, itemType: update(p.itemType!)}) )}
            />
        );
    } else if(property.type === JsonSchemaPropertyType.object) {
        typeSpecificFormControls = (
            <EditJsonObject
                schema={property.objectSchema!}
                updateObject={(update) => props.updateProperty(p => ({ ...p, objectSchema: update(p.objectSchema!) }))}
            />
        );
    }
    return (
        <>
            <Form.Group as={Row}>
                <Col>
                    <Form.Control
                        type="text"
                        value={property.name}
                        onChange={(e: any) => {
                                const value = e.target.value;
                                props.updateProperty(p => ({ ...p, name: value }))
                            }
                        }
                    />
                </Col>
                <Col>
                    <JsonPropertyTypeSelector
                        value={property.type}
                        onChange={onItemTypeChanged}
                    />
                </Col>
            </Form.Group>
            {typeSpecificFormControls}
        </>
    );

}

export default EditJsonProperty;