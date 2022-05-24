using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Glacier.Transfer;
using Amazon.Runtime;
using Aws.Commons;
using Aws.Commons.Configurations;
using Aws.Commons.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Aws.S3.Glacier.Service
{
    public class StorageS3GlacierService
    {
        private readonly ArchiveTransferManager _archiveTransferManager;
        private readonly AwsConfiguration _s3Configuration;

        public StorageS3GlacierService(ArchiveTransferManager archiveTransferManager, AwsConfiguration s3Configuration)
        {
            _archiveTransferManager = archiveTransferManager;
            _s3Configuration = s3Configuration;
        }

        public async Task<string> UploadFileSinglePartAsync(string vaultName, string description, byte[] archiveToUpload)
        {
            try
            {
                var pathToUpload = Path.GetTempFileName();
                await File.WriteAllBytesAsync(pathToUpload, archiveToUpload);
                await _archiveTransferManager.CreateVaultAsync(vaultName);
                var option = new UploadOptions();
                option.AccountId = _s3Configuration.AccountId;
                var archive = await _archiveTransferManager.UploadAsync(vaultName, description, pathToUpload);

                return archive.ArchiveId;
            }
            catch (AmazonGlacierException e) { throw new UploadException(e); }
            catch (AmazonServiceException e) { throw new UploadException(e); }
            catch (Exception) { throw new Exception(Messages.ErrorGeneric); }
        }

        public async Task<string> UploadFileMultiPartAsync(string vaultName, string description, byte[] archiveToUpload)
        {
            try
            {

                await _archiveTransferManager.CreateVaultAsync(vaultName);
                List<string> partChecksumList = new List<string>();
                string archiveId;
                var pathToUpload = Path.GetTempFileName();
                await File.WriteAllBytesAsync(pathToUpload, archiveToUpload);
                var partsize = 4194304;
                using (var client = new AmazonGlacierClient(_s3Configuration.GetRegion()))
                {
                    string uploadId = await InitiateMultipartUploadAsync(client, vaultName, description, partsize);
                    partChecksumList = await UploadPartsAsync(pathToUpload, vaultName,uploadId, partsize, client);
                    archiveId = await CompleteMPUAsync(pathToUpload, vaultName, uploadId, client, partChecksumList);
                }

                return archiveId;
            }
            catch (AmazonGlacierException e) { throw new UploadException(e); }
            catch (AmazonServiceException e) { throw new UploadException(e); }
            catch (Exception) { throw new Exception(Messages.ErrorGeneric); }
        }

        public async Task<byte[]> DownloadFileAsync(string vaultName, string archiveId)
        {
            try
            {
                var downloadFilePath = Path.GetTempFileName();
                var options = new DownloadOptions();
                options.StreamTransferProgress = Progress;
                options.AccountId = _s3Configuration.AccountId;
                await _archiveTransferManager.DownloadAsync(vaultName, archiveId, downloadFilePath, options);

                return await File.ReadAllBytesAsync(downloadFilePath);
            }
            catch (AmazonGlacierException e) { throw new DownloadException(e); }
            catch (AmazonServiceException e) { throw new DownloadException(e); }
            catch (Exception) { throw new Exception(Messages.ErrorGeneric); }
        }

        public async Task DeleteFileAsync(string vaultName, string archiveId)
        {
            try
            {
                await _archiveTransferManager.DeleteArchiveAsync(vaultName, archiveId);
            }
            catch (AmazonGlacierException e) { throw new DownloadException(e); }
            catch (AmazonServiceException e) { throw new DownloadException(e); }
            catch (Exception) { throw new Exception(Messages.ErrorGeneric); }
        }

        #region Static functions
        private async Task<string> InitiateMultipartUploadAsync(AmazonGlacierClient client, string vaultName, string description,long partSize)
        {
            InitiateMultipartUploadRequest initiateMPUrequest = new InitiateMultipartUploadRequest()
            {
                AccountId = _s3Configuration.AccountId,
                VaultName = vaultName,
                PartSize = partSize,
                ArchiveDescription = description
            };

            InitiateMultipartUploadResponse initiateMPUresponse = await client.InitiateMultipartUploadAsync(initiateMPUrequest);

            return initiateMPUresponse.UploadId;
        }

        private async Task<List<string>> UploadPartsAsync(string path, string vaultName, string uploadID, long partSize, AmazonGlacierClient client)
        {
            List<string> partChecksumList = new List<string>();
            long currentPosition = 0;
            var buffer = new byte[Convert.ToInt32(partSize)];

            long fileLength = new FileInfo(path).Length;
            using (FileStream fileToUpload = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                while (fileToUpload.Position < fileLength)
                {
                    Stream uploadPartStream = GlacierUtils.CreatePartStream(fileToUpload, partSize);
                    string checksum = TreeHashGenerator.CalculateTreeHash(uploadPartStream);
                    partChecksumList.Add(checksum);
                    UploadMultipartPartRequest uploadMPUrequest = new UploadMultipartPartRequest()
                    {
                        AccountId = _s3Configuration.AccountId,
                        VaultName = vaultName,
                        Body = uploadPartStream,
                        Checksum = checksum,
                        UploadId = uploadID
                    };
                    uploadMPUrequest.SetRange(currentPosition, currentPosition + uploadPartStream.Length - 1);
                    await client.UploadMultipartPartAsync(uploadMPUrequest);

                    currentPosition = currentPosition + uploadPartStream.Length;
                }
            }
            return partChecksumList;
        }

        private async Task<string> CompleteMPUAsync(string path, string vaultName, string uploadID, AmazonGlacierClient client, List<string> partChecksumList)
        {
            long fileLength = new FileInfo(path).Length;
            CompleteMultipartUploadRequest completeMPUrequest = new CompleteMultipartUploadRequest()
            {
                UploadId = uploadID,
                ArchiveSize = fileLength.ToString(),
                Checksum = TreeHashGenerator.CalculateTreeHash(partChecksumList),
                VaultName = vaultName
            };

            CompleteMultipartUploadResponse completeMPUresponse = await client.CompleteMultipartUploadAsync(completeMPUrequest);
            return completeMPUresponse.ArchiveId;
        }

        private int currentPercentage = -1;
        private void Progress(object sender, StreamTransferProgressArgs args)
        {
            if (args.PercentDone != currentPercentage)
            {
                currentPercentage = args.PercentDone;
                Console.WriteLine("Downloaded {0}%", args.PercentDone);
            }
        }
        #endregion
    }
}
