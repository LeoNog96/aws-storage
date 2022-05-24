using Amazon.Glacier.Transfer;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Aws.Commons.Configurations;
using Aws.S3.Glacier.Service;

AwsConfiguration s3Configuration = new("AKIARZ3C4N5FQACTYMPK", "gqNfaepGW/U2x12vhq4h7QbQccXkV+6cS8K25TkN", "dev-dd", "sa-east-1", "default");
var options = new CredentialProfileOptions
{
    AccessKey = s3Configuration.AccessKey,
    SecretKey = s3Configuration.SecretKey
};
var profile = new CredentialProfile("default", options);
var sharedFile = new SharedCredentialsFile();
sharedFile.RegisterProfile(profile);
AmazonS3Client s3Client = new(s3Configuration.GetRegion());
var archiveTransferManager = new ArchiveTransferManager(s3Configuration.GetRegion());

StorageS3GlacierService service = new(archiveTransferManager, s3Configuration);

var fileToUpload = await File.ReadAllBytesAsync(@"C:\Users\Leonardo\Downloads\TUTORIAL - MELHORIA ASSINATURA - ABASE.pdf");
//var fileToUpload = await File.ReadAllBytesAsync(@"C:\Users\Leonardo\Downloads\openjdk-18.0.1.1_windows-x64_bin.zip");


//var t = await service.UploadFileSinglePartAsync($"ddv4-revs-{15}", $"{45647857}", fileToUpload);
var t = await service.UploadFileMultiPartAsync($"ddv4-revs-{17}", $"{45647857}", fileToUpload);

Console.WriteLine(t);

//var f = await service.DownloadFileAsync($"ddv4-revs-{16}", "GjVs9FvhCD0EJZh7Qq2csRYJCxN8Bs6Re1QV6nXfGMdXAjPwjhQ7F_9hgFTEKMpq6NCNH9GA_ABXPMFtqDu9vSI_Ns187KiVixU4HpFbxcohx5D87I4ibun8KQi9CNlyD8HAFZsSwg");

//await File.WriteAllBytesAsync(@"C:\Users\Leonardo\Downloads\t.pdf", f);