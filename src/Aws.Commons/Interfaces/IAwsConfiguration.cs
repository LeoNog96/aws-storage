using Amazon;

namespace Aws.Commons.Interfaces
{
    public interface IAwsConfiguration
    {
        public RegionEndpoint GetRegion();
        public string Region { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string ProfileName { get; set; }
        public string AccountId { get; set; }
    }
}
