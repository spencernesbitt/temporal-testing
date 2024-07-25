namespace Temporal.Worker
{
    internal class InvalidAccountException : Exception
    {
        public InvalidAccountException(string message) : base(message) { }
    }
}
