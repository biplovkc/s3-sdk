using System.Collections.Generic;

namespace Biplov.S3.Sdk.Bucket.Requests;

public record CreateBucketRequest
(
    string BucketName,
    Dictionary<string, string> Tags,
    string? CorrelationId = null
);