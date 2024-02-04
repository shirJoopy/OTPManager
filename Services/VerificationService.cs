using Microsoft.AspNetCore.DataProtection;
using Oracle.ManagedDataAccess.Client;
using OTPManager.Services.Interfaces;
using OTPManager.Utilities;
using System;
using System.Data;
using System.Transactions;

namespace OTPManager.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly OracleConnection _oracleConnection;

        private readonly IEncryptionService _encryptionService;

        public VerificationService(OracleConnection oracleConnection, IEncryptionService encryptionService)
        {
            _oracleConnection = oracleConnection;
            _encryptionService = encryptionService;
        }


        public void CleanSecrets(string userName)
        {
            OracleTransaction transaction = null;

            try
            {
                _oracleConnection.Open();

                // Start a new transaction
                transaction = _oracleConnection.BeginTransaction();

                string sql = $"DELETE FROM T010_OTP_SECRETS WHERE USERNAME = :userName";
                using (var cmd = new OracleCommand(sql, _oracleConnection))
                {
                    cmd.Parameters.Add("userName", OracleDbType.Varchar2).Value = userName;

                    cmd.Transaction = transaction; // Associate the command with the transaction
                    cmd.ExecuteNonQuery();
                }

                // Commit the transaction to make the changes permanent
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Handle exceptions if needed
                // You can choose to rollback the transaction here if an exception occurs
                if (transaction != null)
                {
                    transaction.Rollback();
                }
            }
            finally
            {
                if (_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }
        }

        private string GenerateAndSaveSecret(string userName)
        {
            OracleTransaction transaction = null;

            try
            {
                string secret = SecretGenerator.GenerateRandomSecret();
                string sql = $"insert into T010_OTP_SECRETS values ('{userName}','{_encryptionService.Encrypt(secret)}',sysdate + INTERVAL '120' SECOND)";
                using (var cmd = new OracleCommand(sql, _oracleConnection))
                {
                    transaction = _oracleConnection.BeginTransaction();
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();

                }
                transaction.Commit();

                // Handle exceptions
                // Log the error, return false, or take appropriate action
                return secret;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return SecretGenerator.GenerateRandomSecret();

            }
            finally
            {
                if (_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }

        }


        public string? GetUserSecret(string userName, string? phoneNumber)
        {
            try
            {
                _oracleConnection.Open();

                string sql = "SELECT ts.SECRET FROM T010_OTP_SECRETS ts, T010_AUTHORIZATIONS t10 " +
                             "WHERE t10.username = :userName AND t10.username = ts.username AND ts.EXPIRATION > SYSTIMESTAMP";

                // Add logic to validate the user against the database
                using var cmd = new OracleCommand(sql, _oracleConnection);
                cmd.Parameters.Add(new OracleParameter("userName", OracleDbType.Varchar2)).Value = userName;

                // Execute the query
                var key = cmd.ExecuteScalar();

                // Check if the user exists in the database
                if (key != null)
                {
                    return _encryptionService.Decrypt(key.ToString());
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                return GenerateAndSaveSecret(userName);
            }
            finally
            {
                if (_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }
        }

    }
}
