using System;

namespace SBD_3
{
    public class NegativeDeltaException : Exception
    {
        public NegativeDeltaException(string message = "Negative delta!")
            : base(message)
        {
        }

        public NegativeDeltaException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class PageReadException : Exception
    {
        public PageReadException(string message = "Page read fault")
            : base(message)
        {
        }
    }

    public class NoPageFoundException : Exception
    {
        public NoPageFoundException(string message = "No such page in directory")
            : base(message)
        {
        }
    }

    public class RecordNotFoundException : Exception
    {
        public RecordNotFoundException(string message = "Couldn't find record")
            : base(message)
        {
        }
    }
}