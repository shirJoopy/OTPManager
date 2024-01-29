using OneTimeCodeApi.Services;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace OneTimeCodeApi.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly OracleConnection _oracleConnection;

        public VerificationService(OracleConnection oracleConnection)
        {
            _oracleConnection = oracleConnection;
        }

        public bool ValidateUser(string userName, string? phoneNumber)
        {
            try
            {
                _oracleConnection.Open();

                string sql = "select count(*) from t010_authorizations t where t.username = :userName";

                // Add logic to validate the user against the database
                using (var cmd = new OracleCommand(sql, _oracleConnection))
                {
                    // Bind parameters
                    cmd.Parameters.Add("userName", OracleDbType.Varchar2).Value = userName;

                    // Execute the query
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    // Check if the user exists in the database
                    return count > 0;
                }
            }
            catch (OracleException ex)
            {
                // Handle exceptions
                // Log the error, return false, or take appropriate action
                return false;
            }
            finally
            {
                if (_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }
        }

        public string GenerateAndSendCode(string? phoneNumber)
        {
            try
            {
                _oracleConnection.Open();

                var oneTimeCode = GenerateOneTimeCode();

                // Add logic to send the code via SMS

                return oneTimeCode;
            }
            catch (OracleException ex)
            {
                // Handle exceptions
                // Log the error, return null, or take appropriate action
                return "";
            }
            finally
            {
                if (_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }
        }

        private string GenerateOneTimeCode()
        {
            // Add logic to generate a one-time code
            return "123456"; // Placeholder
        }
    }
}
