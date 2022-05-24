using Amazon.Glacier.Transfer;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Aws.Commons.Configurations;
using Aws.S3.Glacier.Service;
using Aws.S3.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aws.Configuration.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void ConfigureAws(this IServiceCollection services, IConfiguration configuration)
        {
            var awsConfiguration = new AwsConfiguration();
            new ConfigureFromConfigurationOptions<AwsConfiguration>(
                    configuration.GetSection("AwsConfiguration"))
                .Configure(awsConfiguration);

            var options = new CredentialProfileOptions
            {
                AccessKey = awsConfiguration.AccessKey,
                SecretKey = awsConfiguration.SecretKey
            };
            var profile = new CredentialProfile(awsConfiguration.ProfileName, options);
            var sharedFile = new SharedCredentialsFile();

            sharedFile.RegisterProfile(profile);
            services.AddSingleton(awsConfiguration);
        }

        public static void ConfigureStorageS3(this IServiceCollection services)
        {
            services.AddTransient(x => {
                var awsConfiguration = x.GetService<AwsConfiguration>();
                return new AmazonS3Client(awsConfiguration.GetRegion());
            });
            services.AddTransient<StorageS3Service>();
        }

        public static void ConfigureStorageS3Glacier(this IServiceCollection services)
        {
            services.AddTransient(x => {
                var awsConfiguration = x.GetService<AwsConfiguration>();
                return new ArchiveTransferManager(awsConfiguration.GetRegion());
            });
            services.AddTransient<StorageS3GlacierService>();
        }
    }
}
