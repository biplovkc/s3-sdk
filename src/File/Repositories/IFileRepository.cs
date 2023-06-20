using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Biplov.S3.Sdk.File.Responses;
using OneOf;
using OneOf.Types;

namespace Biplov.S3.Sdk.File.Repositories;

public interface IFileRepository
{
    ValueTask<OneOf<AddFileResponse, Error<string>, Exception>> UploadFile(
        string folderName, string fileId,
        IFormFile formFile,
        IReadOnlyCollection<(string, string)> tags = default, 
        bool isTagsSupported = false,
        CancellationToken cancellationToken = default);

    ValueTask<OneOf<IReadOnlyCollection<ListFileResponse>, Exception>> ListFiles(string folderName,
        string bucketName = null,
        CancellationToken cancellationToken = default);

    OneOf<GetFileDownloadUrlResponse, Exception> GetFileById(
        string key,
        string bucketName = null);
}
