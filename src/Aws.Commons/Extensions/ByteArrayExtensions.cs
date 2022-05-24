using System.IO;
using System.Threading.Tasks;

namespace Aws.Commons.Extensions
{
    public static class ByteArrayExtensions
    {
        public static async Task<string> GetFilePathAsync(this byte[] vs)
        {
            string outputFileName = Path.GetTempFileName();

            await File.WriteAllBytesAsync(outputFileName, vs).ConfigureAwait(false);
            return outputFileName;
        }
    }
}
