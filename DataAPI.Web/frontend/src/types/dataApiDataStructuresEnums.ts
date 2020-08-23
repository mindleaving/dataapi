export enum LoginMethod {
    ActiveDirectory = "ActiveDirectory",
    JsonWebToken = "JsonWebToken"
}
export enum AuthenticationErrorType {
    Ok = "Ok",
    UserNotFound = "UserNotFound",
    InvalidPassword = "InvalidPassword",
    AuthenticationMethodNotAvailable = "AuthenticationMethodNotAvailable"
}
export enum IdGeneratorType {
    Guid = "Guid",
    Integer = "Integer"
}
export enum Role {
    Viewer = "Viewer",
    DataProducer = "DataProducer",
    Analyst = "Analyst",
    UserManager = "UserManager",
    Admin = "Admin"
}
export enum ResultFormat {
    Undefined = "Undefined",
    Json = "Json",
    Csv = "Csv"
}
export enum DataModificationType {
    Unknown = "Unknown",
    Created = "Created",
    Replaced = "Replaced",
    Deleted = "Deleted"
}
export enum ValidatorType {
    PythonScript = "PythonScript",
    Exe = "Exe",
    JsonSchema = "JsonSchema",
    TextRules = "TextRules"
}
export enum DataCollectionProtocolParameterType {
    Text = "Text",
    Number = "Number",
    Date = "Date",
    UnitValue = "UnitValue",
    DataType = "DataType"
}
