import React from 'react';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Button from 'react-bootstrap/Button';
import EditJsonProperty from './EditJsonProperty';
import { FrontendTypes } from '../../../../types/frontend';
import { v4 as uuidv4 } from 'uuid';
import { JsonSchemaPropertyType } from '../../../../types/frontendEnums';

interface EditJsonObjectProps
 {
    schema: FrontendTypes.JsonSchemaObject;
    updateObject: (update: FrontendTypes.Update<FrontendTypes.JsonSchemaObject>) => void;
}

const EditJsonObject = (props: EditJsonObjectProps) => {


    const updateProperty = (
        propertyGuid: string, 
        update: FrontendTypes.Update<FrontendTypes.JsonSchemaProperty>) => {

        props.updateObject(obj => ({
            properties: obj.properties.map(p => {
                if(p.guid === propertyGuid) {
                    return update(p);
                } else {
                    return p;
                }
            })
        }))
    }

    const addNewProperty = () => {
        const newProperty: FrontendTypes.JsonSchemaProperty = {
            guid: uuidv4(),
            name: '',
            type: JsonSchemaPropertyType.string,
            isMandatory: false
        };
        props.updateObject(obj => ({
            properties: obj.properties.concat([ newProperty ])
        }));
    }

    return (
        <Row>
            <Col>
                {props.schema.properties.map(property => (
                    <EditJsonProperty
                        key={property.guid}
                        property={property}
                        updateProperty={update => updateProperty(property.guid, update)}
                    />
                ))}
                <Button variant="secondary" size="sm" className="my-2" onClick={addNewProperty}>+ Add property</Button>
            </Col>
        </Row>
    );

}

export default EditJsonObject;