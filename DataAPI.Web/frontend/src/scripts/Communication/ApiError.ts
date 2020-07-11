export class ApiError extends Error {
    statusCode: number;
    
    constructor(statusCode: number, errorMessage: string) {
        super(errorMessage);
        this.statusCode = statusCode;
    }
}