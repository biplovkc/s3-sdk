namespace Biplov.S3.Sdk.File.Responses;

public record AddFileResponse(string FileId, string PreSignedUrl);