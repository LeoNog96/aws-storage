using System;

namespace Aws.Commons.Exceptions
{
    public class DeleteException : Exception
    {
        public DeleteException() : base(Messages.DeleteError) { }

        public DeleteException(Exception inner) : base(Messages.DeleteError, inner) { }
    }
}
