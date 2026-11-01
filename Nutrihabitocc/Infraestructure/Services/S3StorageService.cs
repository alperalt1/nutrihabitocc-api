using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Services;

public class S3StorageService: IS3StorageService
{
    private readonly IAmazonS3 _client;
    private readonly string _bucket;

    public S3StorageService(IConfiguration configuration)
    {
        var endpoint = configuration["S3:Endpoint"];
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")
                        ?? configuration["S3:AccessKey"];
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
                        ?? configuration["S3:SecretKey"];
        var region = configuration["S3:Region"] ?? "us-east-2";
        _bucket = configuration["S3:Bucket"] ?? "assets";

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("La configuración S3:Endpoint es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("Las credenciales S3 no están configuradas.");
        }

        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = region
        };

        _client = new AmazonS3Client(accessKey, secretKey, s3Config);
    }

    public string? GetPresignedUrl(string key, int expiresMinutes = 60)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(expiresMinutes)
        };

        return _client.GetPreSignedURL(request);
    }

    public async Task<S3ObjectResult?> GetObjectAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        try
        {
            var response = await _client.GetObjectAsync(
                new GetObjectRequest
                {
                    BucketName = _bucket,
                    Key = key
                },
                cancellationToken);

            return new S3ObjectResult
            {
                Stream = response.ResponseStream,
                ContentType = response.Headers.ContentType ?? "application/octet-stream"
            };
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
