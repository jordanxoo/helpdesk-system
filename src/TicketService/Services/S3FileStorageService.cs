using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using TicketService.Configuration;

namespace TicketService.Services;




public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string key);
    string GetPresignedUrl(string key);
    Task DeleteFileAsync(string key);
}


public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly FileStorageSettings _settings;
    private readonly ILogger<S3FileStorageService> _logger;


    public S3FileStorageService(IAmazonS3 amazonS3, IOptions<FileStorageSettings> settings, ILogger<S3FileStorageService> logger)
    {
        _s3Client = amazonS3;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string key)
    {
        try
        {
            await EnsureBucketExsistsAsync();

            using var stream = file.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = _settings.BucketName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            _logger.LogInformation("File uploaded to S3: {key}",key);

            return key;
        }catch(Exception ex)
        {
            _logger.LogError(ex, "S3 Upload failed for key: {key}",key);
            throw;
        }
    }

    public string GetPresignedUrl(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddHours(1),
            Verb = HttpVerb.GET,
            Protocol = Protocol.HTTP
        };
        return _s3Client.GetPreSignedURL(request);
    }

    public async Task DeleteFileAsync(string key)
    {
        await _s3Client.DeleteObjectAsync(_settings.BucketName,key);
    }

    private async Task EnsureBucketExsistsAsync()
    {
        try
        {
            var bucketExsists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client,_settings.BucketName);

            if(!bucketExsists)
            {
                var putBucketRequest = new PutBucketRequest{
                    BucketName = _settings.BucketName,
                    UseClientRegion = true
                };
                await _s3Client.PutBucketAsync(putBucketRequest);
            }
        }catch(Exception ex)
        {
            _logger.LogError(ex,"Error creating S3 Bucket");
            throw;
        }
    }

}

