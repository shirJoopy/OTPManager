namespace OTPManager.Utilities
{
    public static class StaticDataStore
    {
        private static Dictionary<int, string> _langs = new Dictionary<int, string>();

        public static Dictionary<int, string> Languages
        {
            get => _langs;
            set => _langs = value;
        }

        public static void LoadData(string connectionString)
        {
            using (var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT LANG_ID, LANG_NAME FROM T250_LANG_MAP where active = 'Y'";
                    using (var reader = command.ExecuteReader())
                    {
                        var tempData = new Dictionary<int, string>();
                        while (reader.Read())
                        {
                            int key = Int32.Parse(reader["LANG_ID"].ToString());
                            string value = reader["LANG_NAME"].ToString();
                            tempData[key] = value;
                        }
                        _langs = tempData;
                    }
                }
            }
        }

    }

}
