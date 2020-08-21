import { JsonSchemaPropertyType } from "./frontendEnums";

export namespace FrontendTypes {
    interface JsonSchemaProperty {
        guid: string;
        name: string;
        type: JsonSchemaPropertyType;
        itemType?: JsonSchemaArrayItem;
        objectSchema?: JsonSchemaObject;
        isMandatory: boolean;
    }
    interface JsonSchemaObject {
        properties: JsonSchemaProperty[];
    }
    interface JsonSchemaArrayItem {
        type: JsonSchemaPropertyType;
        itemType?: JsonSchemaArrayItem;
        objectSchema?: JsonSchemaObject;
    }
    type Update<T> = (item: T) => T;
}