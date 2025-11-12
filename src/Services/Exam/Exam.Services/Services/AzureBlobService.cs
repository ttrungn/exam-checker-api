using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Configurations;
using Microsoft.Extensions.Options;

namespace Exam.Services.Services;

public class AzureBlobService : IAzureBlobService
{
     private readonly BlobServiceClient _blobServiceClient;
     private readonly StorageSharedKeyCredential _credential;

    public AzureBlobService(BlobServiceClient blobServiceClient, IOptions<BlobSettings> blobOptions)
    {
        _blobServiceClient = blobServiceClient;
        // Lấy AccountName/Key từ connection string để ký SAS
        var conn = blobOptions.Value.ConnectionString;
        var parts = conn.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .Where(a => a.Length == 2)
            .ToDictionary(a => a[0], a => a[1], StringComparer.OrdinalIgnoreCase);

        var accountName = parts["AccountName"];
        var accountKey  = parts["AccountKey"];
        _credential = new StorageSharedKeyCredential(accountName, accountKey);
    }

    public async Task<string> UploadAsync(Stream fileStream, string blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        string contentType = GetContentTypeFromFileName(blobName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        var blobOptions = new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        };

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(fileStream, blobOptions);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadAsync();

        return response.Value.Content;
    }

    public async Task DeleteAsync(string blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<IEnumerable<string>> ListBlobsAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobs = containerClient.GetBlobsAsync();

        var result = new List<string>();
        await foreach (var blobItem in blobs)
        {
            result.Add(blobItem.Name);
        }

        return result;
    }

    private string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".tiff" or ".tif" => "image/tiff",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
    
    // tạo SAS URL read-only
    public string GetReadSasUrl(string containerName, string blobName, TimeSpan ttl)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var blob = container.GetBlobClient(blobName);

        var sas = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(ttl)
        };
        sas.SetPermissions(BlobSasPermissions.Read);

        var sasToken = sas.ToSasQueryParameters(_credential).ToString();
        return $"{blob.Uri}?{sasToken}";
    }

}
