using System;

namespace Aws.Commons.Exceptions
{
    public class UploadException : Exception
    {
        public UploadException() : base(Messages.UploadError) { }

        public UploadException(Exception inner) : base(Messages.UploadError, inner) { }
    }
}
