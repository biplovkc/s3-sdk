using Biplov.S3.Sdk.Bucket.Responses;
using OneOf.Types;
using OneOf;

namespace Biplov.S3.Sdk.Bucket;

public interface IBucketRepository
{
    ValueTask<OneOf<bool, Exception>> DoesS3BucketExistAsync(string bucketName);
    ValueTask<OneOf<CreateBucketResponse, Error<string>, Exception>> CreateBucketAsync(string bucketName);
    ValueTask<OneOf<IReadOnlyCollection<ListBucketResponse>, Exception>> ListBucketsAsync();
}
