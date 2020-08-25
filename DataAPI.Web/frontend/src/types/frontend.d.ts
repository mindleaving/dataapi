import { JsonSchemaPropertyType } from "./frontendEnums";
import { ValidatorType } from "./dataApiDataStructuresEnums";

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
    interface CollectionValidator {
        id: string;
        existsInDataAPI: boolean;
        validatorType: ValidatorType;
        schema: FrontendTypes.JsonSchemaObject;
        rules: string;
    }
}