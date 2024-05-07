namespace OTPManager.Models
{
    public static class DataStore
    {
        private static Dictionary<int, string> _langs = new Dictionary<int, string>();

        public static Dictionary<int, string> Languages
        {
            get => _langs;
            set => _langs = value;
        }
       
    }

}
