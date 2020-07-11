export const enum IdGeneratorType {
    Guid = 0,
    Integer = 1
}
export const enum ResultFormat {
    Undefined = 0,
    Json = 1,
    Csv = 2
}
export const enum DataCollectionProtocolParameterType {
    Text = 0,
    Number = 1,
    Date = 2,
    UnitValue = 3,
    DataType = 4
}
export const enum IdSourceSystem {
    SelfAssigned = 0
}
export const enum DataModificationType {
    Unknown = 0,
    Created = 1,
    Replaced = 2,
    Deleted = 3
}
export const enum Role {
    Viewer = 1,
    DataProducer = 2,
    Analyst = 3,
    UserManager = 4,
    Admin = 99
}
export const enum ValidatorType {
    PythonScript = 1,
    Exe = 2,
    JsonRuleset = 3,
    TextRules = 4
}