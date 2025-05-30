using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.AspNetCore.Mvc;
using webapi.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SasTokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SasTokenController> _logger;

        public SasTokenController(IConfiguration configuration, ILogger<SasTokenController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("generate")]
        public IActionResult GenerateSasUrl([FromQuery] string filename)
        {
            _logger.LogInformation("Generating SAS upload URL for file: {Filename}", filename);

            if (string.IsNullOrWhiteSpace(filename))
            {
                _logger.LogWarning("Filename is null or empty when generating SAS upload URL.");
                return BadRequest("Filename must be provided.");
            }

            try
            {
                var accountName = _configuration["AzureStorage:AccountName"];
                var accountKey = _configuration["AzureStorage:AccountKey"];
                var containerName = _configuration["AzureStorage:ContainerName"];

                var blobUri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}/{filename}");

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = filename,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

                var credential = new StorageSharedKeyCredential(accountName, accountKey);
                var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();
                var uploadUrl = $"{blobUri}?{sasToken}";

                _logger.LogInformation("SAS upload URL generated successfully for file: {Filename}", filename);

                return Ok(new
                {
                    uploadUrl,
                    blobUrl = blobUri.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS upload URL for file: {Filename}", filename);
                return StatusCode(500, "An error occurred while generating the SAS URL.");
            }
        }

        [HttpGet("download")]
        public IActionResult GenerateReadSasUrl([FromQuery] string filename)
        {
            _logger.LogInformation("Generating SAS download URL for file: {Filename}", filename);

            if (string.IsNullOrWhiteSpace(filename))
            {
                _logger.LogWarning("Filename is null or empty when generating SAS download URL.");
                return BadRequest("Filename must be provided.");
            }

            try
            {
                var accountName = _configuration["AzureStorage:AccountName"];
                var accountKey = _configuration["AzureStorage:AccountKey"];
                var containerName = _configuration["AzureStorage:ContainerName"];

                var blobUri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}/{filename}");

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = filename,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var credential = new StorageSharedKeyCredential(accountName, accountKey);
                var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();
                var downloadUrl = $"{blobUri}?{sasToken}";

                _logger.LogInformation("SAS download URL generated successfully for file: {Filename}", filename);

                return Ok(new { downloadUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS download URL for file: {Filename}", filename);
                return StatusCode(500, "An error occurred while generating the download URL.");
            }
        }
    }
}