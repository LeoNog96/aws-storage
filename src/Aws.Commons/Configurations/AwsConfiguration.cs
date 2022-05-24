using Amazon;
using Aws.Commons.Interfaces;

namespace Aws.Commons.Configurations
{
    public class AwsConfiguration : IAwsConfiguration
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public string Region { get; set; }
        public string ProfileName { get; set; }

        public AwsConfiguration()
        {
        }

        public AwsConfiguration(string accessKey, string secretKey, string bucketName, string region, string profileName)
        {
            AccessKey = accessKey;
            SecretKey = secretKey;
            BucketName = bucketName;
            Region = region;
            ProfileName = profileName;
        }

        public RegionEndpoint GetRegion() => RegionEndpoint.GetBySystemName(Region);
    }
}
