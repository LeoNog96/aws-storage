using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Aws.Commons;
using Aws.Commons.Configurations;
using Aws.Commons.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aws.S3.Service
{
    public class StorageS3Service
    {
        private readonly AwsConfiguration _s3Configuration;
        private readonly IAmazonS3 _s3Client;

        public StorageS3Service(AmazonS3Client s3Client, AwsConfiguration s3Configuration)
        {
            _s3Configuration = s3Configuration;
            _s3Client = s3Client;
        }

        /// <summary>
        /// Upload Arquivo
        /// </summary>
        /// <param name="fileStream">Arquivo a ser feito o upload</param>
        /// <param name="keyName">Nome do arquivo que será salvo no storage</param>
        /// <returns></returns>
        public async Task UploadFile(FileStream fileStream, string keyName)
        {
            var fileTransferUtility =
                    new TransferUtility(_s3Client);

            try
            {
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _s3Configuration.BucketName,
                    InputStream = fileStream,
                    StorageClass = S3StorageClass.StandardInfrequentAccess,
                    PartSize = (long)(fileStream.Length * 0.30),
                    Key = keyName,
                    CannedACL = S3CannedACL.Private
                };

                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
            }
            catch (AmazonS3Exception e)
            {
                throw new UploadException(e);
            }
            catch (Exception)
            {
                throw new Exception(Messages.ErrorGeneric);
            }
        }

        /// <summary>
        ///  Download de um Arquivo
        /// </summary>
        /// <param name="keyName">Nome do arquivo salvo no storage</param>
        /// <returns></returns>
        public async Task<Stream> DownlodFileAsync(string keyName)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _s3Configuration.BucketName,
                    Key = keyName
                };
                using GetObjectResponse response = await _s3Client.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception e)
            {
                throw new DownloadException(e);
            }
            catch (Exception)
            {
                throw new Exception(Messages.ErrorGeneric);
            }
        }

        /// <summary>
        ///  Remoção do arquivo
        /// </summary>
        /// <param name="keyName">Nome do arquivo que está salvo no storage</param>
        /// <returns></returns>
        public async Task RemoveFileAsync(string keyName)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _s3Configuration.BucketName,
                    Key = keyName
                };
                await _s3Client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (AmazonS3Exception e)
            {
                throw new DeleteException(e);
            }
            catch (Exception)
            {
                throw new Exception(Messages.ErrorGeneric);
            }
        }

        /// <summary>
        /// Mover Arquivo para outra Pasta
        /// </summary>
        /// <param name="oldKeyName"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        /// <exception cref="MoveException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task MoveToFile(string oldKeyName, string keyName)
        {
            try
            {
                CopyObjectRequest copyObjectRequest = new CopyObjectRequest()
                {
                    SourceBucket = _s3Configuration.BucketName,
                    DestinationBucket = _s3Configuration.BucketName,
                    SourceKey = oldKeyName,
                    DestinationKey = keyName
                };

                await _s3Client.CopyObjectAsync(copyObjectRequest);
                await RemoveFileAsync(oldKeyName);
            }
            catch (AmazonS3Exception e)
            {
                throw new MoveException(e);
            }
            catch (Exception)
            {
                throw new Exception(Messages.ErrorGeneric);
            }
        }
    }
}
