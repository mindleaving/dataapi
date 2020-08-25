import * as Enums from './dataApiDataStructuresEnums';

export namespace DataAPI {
    export namespace DataStructures {
        interface CollectionInformation {
            collectionName: string;
            displayName: string;
            description: string;
            isProtected: boolean;
            nonAdminUsersCanOverwriteData: boolean;
            isHidden: boolean;
            idGeneratorType: Enums.IdGeneratorType;
            username: string;
            userRoles: Enums.Role[];
            validatorDefinitions: DataAPI.DataStructures.Validation.ValidatorDefinition[];
        }
    
        interface Image {
            id: string;
            filename: string;
            data: System.Byte[];
            extension: string;
        }
    
        export namespace UserManagement {
            interface AuthenticationResult {
                isAuthenticated: boolean;
                username?: string;
                accessToken?: string;
                error: Enums.AuthenticationErrorType;
            }
        
            interface UserProfile {
                username: string;
                firstName: string;
                lastName: string;
                email: string;
            }
        
            interface RegistrationInformation {
                username: string;
                firstName: string;
                lastName: string;
                email: string;
                password: string;
            }
        }
    
        export namespace CollectionManagement {
            interface CollectionUserPermissions {
                collectionName: string;
                username: string;
                roles: Enums.Role[];
            }
        
            interface CollectionOptions {
                collectionName: string;
                displayName: string;
                description: string;
                isProtected?: boolean | null;
                nonAdminUsersCanOverwriteData?: boolean | null;
                isHidden?: boolean | null;
                idGeneratorType?: Enums.IdGeneratorType | null;
            }
        }
    
        export namespace PostBodies {
            interface ChangePasswordBody {
                username: string;
                password: string;
            }
        
            interface SubmitDataBody {
                dataType: string;
                id?: string;
                overwrite: boolean;
                data: any;
            }
        
            interface SearchBody {
                query: string;
                format: Enums.ResultFormat;
            }
        
            interface SubscriptionBody {
                dataType: string;
                modificationTypes: Enums.DataModificationType[];
                filter: string;
            }
        
            interface SubmitValidatorBody {
                validatorDefinition: DataAPI.DataStructures.Validation.ValidatorDefinition;
                suppressAutoApprove: boolean;
            }
        
            interface ApplyValidatorBody {
                dataType: string;
                validatorId: string;
                data: any;
            }
        
            interface CreateViewBody {
                query: string;
                expires?: Date | null;
                viewId: string;
            }
        }
    
        export namespace Validation {
            interface ValidatorDefinition {
                id: string;
                dataType: string;
                submitter: string;
                submitterEmail: string;
                validatorType: Enums.ValidatorType;
                executableBase64?: string;
                ruleset?: string;
                isApproved: boolean;
            }
        
            interface RulesetValidationResult {
                isValid: boolean;
                ruleset: string;
                ruleValidationResults: DataAPI.DataStructures.Validation.RuleValidationResult[];
            }
        
            interface ValidationResult {
                isValid: boolean;
                errorDescription: string;
                rejectingValidatorId: string;
                rejectingValidatorEmail: string;
            }
        
            interface RuleValidationResult {
                isValid: boolean;
                rule: string;
                errorDescription: string;
            }
        }
    
        export namespace DataSubscription {
            interface SubscriptionInfo {
                id: string;
                dataType: string;
                modificationTypes: Enums.DataModificationType[];
                filter: string;
            }
        
            interface SubscriptionNotification {
                id: string;
                dataType: string;
                dataObjectId: string;
                modificationType: Enums.DataModificationType;
            }
        }
    
        export namespace DataManagement {
            interface DataProject {
                id: string;
                idSourceSystem: string;
                protocol: DataAPI.DataStructures.DataManagement.DataCollectionProtocol;
                parameterResponse: { [key: string]: string };
            }
        
            interface DataProjectUploadInfo {
                id: string;
                uploaderInitials: string;
                uploadTimestamp: Date;
                dataProjectId: string;
                filename?: string;
                rawData: DataAPI.DataStructures.DataManagement.DataReference;
                derivedData: DataAPI.DataStructures.DataManagement.DataReference[];
            }
        
            interface DataTag {
                id: string;
                dataReference: DataAPI.DataStructures.DataManagement.DataReference;
                tagName: string;
            }
        
            interface DataCollectionProtocol {
                id: string;
                parameters: DataAPI.DataStructures.DataManagement.DataCollectionProtocolParameter[];
                expectedData: DataAPI.DataStructures.DataManagement.DataPlaceholder[];
            }
        
            interface DataReference {
                dataType: string;
                id: string;
            }
        
            interface DataCollectionProtocolParameter {
                id: string;
                name: string;
                defaultValue: string;
                isMandatory: boolean;
                type: Enums.DataCollectionProtocolParameterType;
                dataType: string;
            }
        
            interface DataPlaceholder {
                dataType: string;
                name: string;
                description: string;
                isMandatory: boolean;
            }
        }
    
        export namespace DataIo {
            interface DataBlob {
                id: string;
                filename: string;
                data: System.Byte[];
            }
        
            interface DeleteResult {
                dataType: string;
                id: string;
                isDeleted: boolean;
                errorMessage: string;
            }
        }
    
        export namespace Views {
            interface ViewInformation {
                viewId: string;
            }
        }
    }
}
export namespace System {
    interface Byte {
        
    }
}
