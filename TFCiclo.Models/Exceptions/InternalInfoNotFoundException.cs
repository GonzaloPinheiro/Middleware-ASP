
namespace TFCiclo.Domain.Exceptions
{
    public class InternalInfoNotFoundException : Exception
    {
        public string Error { get; }

        public InternalInfoNotFoundException(string error)
            : base(error)
        {
            Error = error;
        }
    }
}
