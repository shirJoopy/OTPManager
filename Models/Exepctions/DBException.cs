using Vonage.Messages.Webhooks;

namespace OTPManager.Models.Exepctions
{
    public class DBException : Exception
    {
        object[] _params = new object[0];
        public DBException()
        {
        }

        public DBException(string message)
            : base(message)
        {
        }

        public DBException(string message, object[] dbParams)
            : base(message)
        {
            this._params = dbParams;
        }


        public DBException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
