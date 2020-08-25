import { v4 as uuidv4 } from 'uuid';
import { FrontendTypes } from "../../types/frontend";
import { JsonSchemaPropertyType } from "../../types/frontendEnums";
import { JSONSchema7, JSONSchema7Definition } from 'json-schema';

export const convertJsonSchemaObject = (obj: FrontendTypes.JsonSchemaObject): JSONSchema7 => {
    const schema: JSONSchema7 = {
        type: 'object',
        properties: { },
        required: obj.properties.filter(p => p.isMandatory).map(p => p.name)
    };
    obj.properties.forEach(p => schema.properties![p.name] = convertJsonSchemaProperty(p));
    return schema;
}

export const convertJsonSchemaProperty = (property: FrontendTypes.JsonSchemaProperty): JSONSchema7Definition => {
    if(property.type === JsonSchemaPropertyType.string
        || property.type === JsonSchemaPropertyType.number
        || property.type === JsonSchemaPropertyType.boolean
        || property.type === JsonSchemaPropertyType.null) {
        return {
            type: property.type
        }
    }
    if(property.type === JsonSchemaPropertyType.array) {
        return {
            type: property.type,
            items: convertItemType(property.itemType!)
        }
    }
    if(property.type === JsonSchemaPropertyType.object) {
        return convertJsonSchemaObject(property.objectSchema!);
    }
    throw new Error(`Property type '${property.type}' is not supported`);
}

export const convertItemType = (itemType: FrontendTypes.JsonSchemaArrayItem): JSONSchema7Definition => {
    if(itemType.type === JsonSchemaPropertyType.string
        || itemType.type === JsonSchemaPropertyType.number
        || itemType.type === JsonSchemaPropertyType.boolean
        || itemType.type === JsonSchemaPropertyType.null) {
        return {
            type: itemType.type
        }
    }
    if(itemType.type === JsonSchemaPropertyType.array) {
        return {
            type: itemType.type,
            items: convertItemType(itemType.itemType!)
        }
    }
    if(itemType.type === JsonSchemaPropertyType.object) {
        return convertJsonSchemaObject(itemType.objectSchema!);
    }
    throw new Error(`Item type '${itemType.type}' is not supported`);
}

export const convertJsonSchemaToFrontend = (schema: JSONSchema7): FrontendTypes.JsonSchemaObject => {
    const frontendSchema: FrontendTypes.JsonSchemaObject = {
        properties: []
    };
    if(schema.properties) {
        Object.keys(schema.properties).forEach(propertyName => {
            const propertyValue = schema.properties![propertyName] as JSONSchema7;
            const frontendProperty: FrontendTypes.JsonSchemaProperty = {
                guid: uuidv4(),
                name: propertyName,
                isMandatory: schema.required?.includes(propertyName) ?? false,
                type: propertyValue.type as JsonSchemaPropertyType,
                itemType: propertyValue.type === "array" 
                    ? convertArrayToFrontend(propertyValue)
                    : undefined,
                objectSchema: propertyValue.type === "object" ? convertJsonSchemaToFrontend(propertyValue) : undefined
            };
            frontendSchema.properties.push(frontendProperty);
        });
    }
    return frontendSchema;
}

const convertArrayToFrontend = (property: JSONSchema7): FrontendTypes.JsonSchemaArrayItem => {
    const schemaItems = property.items as JSONSchema7;
    const type = schemaItems?.type as JsonSchemaPropertyType ?? JsonSchemaPropertyType.string;
    return {
        type: type,
        itemType: type === JsonSchemaPropertyType.array ? convertArrayToFrontend(schemaItems) : undefined,
        objectSchema: type === JsonSchemaPropertyType.object ? convertJsonSchemaToFrontend(schemaItems) : undefined
    };
}