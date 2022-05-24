using System;

namespace Aws.Commons.Exceptions
{
    public class MoveException : Exception
    {
        public MoveException() : base(Messages.DownloadError) { }

        public MoveException(Exception inner) : base(Messages.DownloadError, inner) { }
    }
}
