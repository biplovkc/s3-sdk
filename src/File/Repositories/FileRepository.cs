using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Biplov.S3.Sdk.File.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OneOf.Types;
using OneOf;
using Serilog;

namespace Biplov.S3.Sdk.File.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger _logger;
    private readonly FileOptions _options;

    public FileRepository(IAmazonS3 s3Client, IOptions<FileOptions> options, ILogger logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ = options ?? throw new ArgumentNullException(nameof(options));
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.BucketName))
            throw new ArgumentNullException(_options.BucketName, "Bucket name cannot be empty");
    }


    public async ValueTask<OneOf<AddFileResponse, Error<string>, Exception>> UploadFile(string folderName, string fileId, IFormFile formFile,
        IReadOnlyCollection<(string, string)> tags = default, bool isTagsSupported = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = formFile.OpenReadStream(),
                Key = $"{folderName}/{fileId}",
                BucketName = _options.BucketName,
                CannedACL = S3CannedACL.PublicRead
            };
            
            if (isTagsSupported)
                uploadRequest.TagSet = new List<Tag>(5) { new() { Key = "fileName", Value = formFile.FileName } };

            if (tags is { Count: > 0 } && isTagsSupported is true)
                foreach (var (key, value) in tags)
                    uploadRequest.TagSet.Add(new Tag { Key = key, Value = value });

            using var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);

            var expiryUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = fileId,
                Expires = DateTime.Now.AddHours(8)
            };

            var url = _s3Client.GetPreSignedURL(expiryUrlRequest);
            return new AddFileResponse(fileId, url);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error uploading file with fileId - {FileId} and fileName - {FileName}", fileId, formFile.FileName);
            return e;
        }

    }

    public async ValueTask<OneOf<IReadOnlyCollection<ListFileResponse>, Exception>> ListFiles(string folderName, string bucketName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = $"{folderName}/",
                Delimiter = "/"
            };

            var response = await _s3Client.ListObjectsV2Async(request, cancellationToken);
            var listFileResponses = new List<ListFileResponse>();

            foreach (var obj in response.S3Objects)
            {
                if (Path.GetFileName(obj.Key).Contains(folderName))
                {
                    listFileResponses.Add(new ListFileResponse(
                        BucketName: bucketName,
                        Key: obj.Key,
                        Owner: obj.Owner.DisplayName,
                        Size: obj.Size
                    ));
                }
            }

            return listFileResponses;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error listing files inside folder - {FolderName} for bucket - {BucketName}", folderName, bucketName);
            return ex;
        }

    }

    public OneOf<GetFileDownloadUrlResponse, Exception> GetFileById(string key, string bucketName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                bucketName = _options.BucketName;

            var expiryUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.Now.AddHours(8)
            };

            var url = _s3Client.GetPreSignedURL(expiryUrlRequest);
            return new GetFileDownloadUrlResponse(url);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error fetching file by id - {Key} of bucket - {Bucket}", key, bucketName);
            return e;
        }

    }
}
