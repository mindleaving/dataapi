import React from 'react';
import Form from 'react-bootstrap/Form';
import { JsonSchemaPropertyType } from '../../../../types/frontendEnums';

interface JsonPropertyTypeSelectorProps {
    value: JsonSchemaPropertyType, 
    onChange: (type: JsonSchemaPropertyType) => void
}

const JsonPropertyTypeSelector = (props: JsonPropertyTypeSelectorProps) => {
    return (
        <Form.Control
            as="select"
            value={props.value}
            onChange={(e: any) => props.onChange(e.target.value as JsonSchemaPropertyType)}
        >
            {[
                JsonSchemaPropertyType.string,
                JsonSchemaPropertyType.number,
                JsonSchemaPropertyType.boolean,
                JsonSchemaPropertyType.array,
                JsonSchemaPropertyType.object
            ].map(type => <option key={type} value={type}>{type}</option>)}
        </Form.Control>
    )
}

export default JsonPropertyTypeSelector;