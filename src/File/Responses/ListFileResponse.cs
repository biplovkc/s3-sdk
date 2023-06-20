namespace Biplov.S3.Sdk.File.Responses;

public record ListFileResponse(string BucketName, string Key, string Owner, long Size);
