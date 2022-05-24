using System;

namespace Aws.Commons.Exceptions
{
    public class DownloadException : Exception
    {
        public DownloadException() : base(Messages.DownloadError) { }

        public DownloadException(Exception inner) : base(Messages.DownloadError, inner) { }
    }
}
