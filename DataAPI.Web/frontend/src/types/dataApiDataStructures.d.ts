import * as Enums from './dataApiDataStructuresEnums';

export namespace DataAPI.DataStructures {
	interface CollectionInformation {
		collectionName: string;
		description: string;
		displayName: string;
		idGeneratorType: Enums.IdGeneratorType;
		isHidden: boolean;
		isProtected: boolean;
		nonAdminUsersCanOverwriteData: boolean;
		username: string;
		userRoles: Enums.Role[];
		validatorDefinitions: DataAPI.DataStructures.Validation.ValidatorDefinition[];
	}
	interface Image {
		data: number[];
		extension: string;
		filename: string;
		id: string;
	}
}
export namespace DataAPI.DataStructures.DataIo {
	interface DataBlob {
		data: number[];
		filename: string;
		id: string;
	}
	interface DeleteResult {
		dataType: string;
		errorMessage: string;
		id: string;
		isDeleted: boolean;
	}
}
export namespace DataAPI.DataStructures.DataManagement {
	interface DataCollectionProtocol {
		expectedData: DataAPI.DataStructures.DataManagement.DataPlaceholder[];
		id: string;
		parameters: DataAPI.DataStructures.DataManagement.DataCollectionProtocolParameter[];
	}
	interface DataCollectionProtocolParameter {
		dataType: string;
		defaultValue: string;
		id: string;
		isMandatory: boolean;
		name: string;
		type: Enums.DataCollectionProtocolParameterType;
	}
	interface DataPlaceholder {
		dataType: string;
		description: string;
		isMandatory: boolean;
		name: string;
	}
	interface DataProject {
		id: string;
		idSourceSystem: Enums.IdSourceSystem;
		parameterResponse: any[];
		protocol: DataAPI.DataStructures.DataManagement.DataCollectionProtocol;
	}
	interface DataProjectUploadInfo {
		dataProjectId: string;
		derivedData: DataAPI.DataStructures.DataManagement.DataReference[];
		filename: string;
		id: string;
		rawData: DataAPI.DataStructures.DataManagement.DataReference;
		uploaderInitials: string;
		uploadTimestamp: Date;
	}
	interface DataReference {
		dataType: string;
		id: string;
	}
	interface DataTag {
		dataReference: DataAPI.DataStructures.DataManagement.DataReference;
		id: string;
		tagName: string;
	}
}
export namespace DataAPI.DataStructures.DataSubscription {
	interface SubscriptionInfo {
		dataType: string;
		filter: string;
		id: string;
		modificationTypes: Enums.DataModificationType[];
	}
	interface SubscriptionNotification {
		dataObjectId: string;
		dataType: string;
		id: string;
		modificationType: Enums.DataModificationType;
	}
}
export namespace DataAPI.DataStructures.PostBodies {
	interface ApplyValidatorBody {
		data: any;
		dataType: string;
		validatorId: string;
	}
	interface ChangePasswordBody {
		password: string;
		username: string;
	}
	interface CreateViewBody {
		expires: Date | null;
		query: string;
		viewId: string;
	}
	interface SearchBody {
		format: Enums.ResultFormat;
		query: string;
	}
	interface SubmitDataBody {
		data: any;
		dataType: string;
		id: string | null;
		overwrite: boolean;
	}
	interface SubmitValidatorBody {
		suppressAutoApprove: boolean;
		validatorDefinition: DataAPI.DataStructures.Validation.ValidatorDefinition;
	}
	interface SubscriptionBody {
		dataType: string;
		filter: string;
		modificationTypes: Enums.DataModificationType[];
	}
}
export namespace DataAPI.DataStructures.UserManagement {
	interface RegistrationInformation {
		email: string;
		firstName: string;
		lastName: string;
		password: string;
		username: string;
	}
	interface UserProfile {
		email: string;
		firstName: string;
		lastName: string;
		username: string;
	}
}
export namespace DataAPI.DataStructures.Validation {
	interface RulesetValidationResult {
		isValid: boolean;
		ruleset: string;
		ruleValidationResults: DataAPI.DataStructures.Validation.RuleValidationResult[];
	}
	interface RuleValidationResult {
		errorDescription: string;
		isValid: boolean;
		rule: string;
	}
	interface ValidationResult {
		errorDescription: string;
		isValid: boolean;
		rejectingValidatorEmail: string;
		rejectingValidatorId: string;
	}
	interface ValidatorDefinition {
		dataType: string;
		executableBase64: string;
		id: string;
		isApproved: boolean;
		ruleset: string;
		submitter: string;
		submitterEmail: string;
		validatorType: Enums.ValidatorType;
	}
}
export namespace DataAPI.DataStructures.Views {
	interface ViewInformation {
		viewId: string;
	}
}
export namespace System.Collections.Generic {
	interface KeyValuePair<TKey, TValue> {
		key: TKey;
		value: TValue;
	}
}
