using Amazon.Runtime.CredentialManagement;
using Aws.Commons.Configurations;

var configuration = new AwsConfiguration("", "bucket", "key", "secrete", "default", "account id");

var options = new CredentialProfileOptions
{
    AccessKey = configuration.AccessKey,
    SecretKey = configuration.SecretKey
};
var profile = new CredentialProfile(configuration.ProfileName, options);
var sharedFile = new SharedCredentialsFile();
sharedFile.RegisterProfile(profile);


//chamada de codigo