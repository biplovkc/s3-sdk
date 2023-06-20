using Amazon;
using Amazon.S3;
using Biplov.S3.Sdk.Bucket;
using Biplov.S3.Sdk.File.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Biplov.S3.Sdk;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterFileServices(this IServiceCollection services, FileOptions options)
    {
        var s3Client = CreateS3Client(options);
        services.AddSingleton<IAmazonS3>(s3Client);
        services.AddTransient<IBucketRepository, BucketRepository>();
        services.AddTransient<IFileRepository, FileRepository>();
        return services;
    }

    private static AmazonS3Client CreateS3Client(FileOptions options)
    {
        var credentialsProvided = !string.IsNullOrEmpty(options.AccessKey) && !string.IsNullOrEmpty(options.SecretKey);
        var endpoint = RegionEndpoint.GetBySystemName(options.Region);
        if (credentialsProvided)
        {
            var config = new AmazonS3Config();
            if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                config.ServiceURL = options.ServiceUrl;
                config.ForcePathStyle = true;
                config.AuthenticationRegion = options.Region;
            }
            else
            {
                config.RegionEndpoint = endpoint;
            }

            return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
        }

        return new AmazonS3Client(endpoint);
    }
}