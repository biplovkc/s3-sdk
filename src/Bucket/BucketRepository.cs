using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;
using Biplov.S3.Sdk.Bucket.Responses;
using OneOf.Types;
using OneOf;
using System.Net;
using System.Threading.Tasks;

namespace Biplov.S3.Sdk.Bucket;

internal class BucketRepository : IBucketRepository
{
    private readonly IAmazonS3 _s3Client;

    public BucketRepository(IAmazonS3 s3Client)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
    }

    public async ValueTask<OneOf<bool, Exception>> DoesS3BucketExistAsync(string bucketName)
    {
        try
        {
            return await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async ValueTask<OneOf<CreateBucketResponse, Error<string>, Exception>> CreateBucketAsync(string bucketName)
    {
        try
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true,
            };

            var response = await _s3Client.PutBucketAsync(putBucketRequest);

            return response.HttpStatusCode is HttpStatusCode.OK
                ? new CreateBucketResponse(response.ResponseMetadata.RequestId, bucketName)
                : new Error<string>(response.HttpStatusCode.ToString());
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async ValueTask<OneOf<IReadOnlyCollection<ListBucketResponse>, Exception>> ListBucketsAsync()
    {
        try
        {
            var response = await _s3Client.ListBucketsAsync();
            return response.Buckets.Select(x => new ListBucketResponse(x.BucketName, x.CreationDate)).ToArray();
        }
        catch (Exception e)
        {
            return e;
        }
    }
}