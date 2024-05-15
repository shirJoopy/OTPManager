using Microsoft.AspNetCore.DataProtection;
using Oracle.ManagedDataAccess.Client;
using OTPManager.Services.Interfaces;
using OTPManager.Utilities;
using System;
using System.Data;
using System.Transactions;
using Vonage.Accounts;
using Vonage.Messages.Webhooks;
using Vonage.Users;

namespace OTPManager.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly OracleConnection _oracleConnection;
        private readonly IConfiguration _configuration;

        private readonly IEncryptionService _encryptionService;


        private readonly string userFieldsSql = @"SELECT TENANT_ID,
                               USERNAME,
                               CHANNEL_ID,
                               ROLE_ID,
                               USER_LEVEL,
                               DEP_ID,
                               DATA_VISIBLE,
                               SALARY_VISIBLE,
                               LOCK_PERIOD,
                               LOCK_OL,
                               MGR_READ_ONLY,
                               TYPE_ID,
                               ALLOW_APPROVE_PLANS,
                               SUBROLE_ID,
                               EMAIL,
                               LANG,
                               USER_ID,
                               PERMISSION_ROLE_ID,
                               MANAGER_PERM_ROLE_ID,
                               CAN_SEE_OTHER_EMPLOYEES,
                               PROFILE_ID,
                               LOCKED_UNTIL,
                               PHONE_NUMBER,
                               TOTP_IDENTIFIER ";

        public VerificationService(OracleConnection oracleConnection, IEncryptionService encryptionService, IConfiguration configuration)
        {
            _oracleConnection = oracleConnection;
            _encryptionService = encryptionService;
            _configuration = configuration;
        }


        public void CleanSecrets(string userName, string type)
        {
            OracleTransaction transaction = null;

            try
            {
                _oracleConnection.Open();

                // Start a new transaction
                transaction = _oracleConnection.BeginTransaction();

                string sql = $"DELETE FROM T010_OTP_SECRETS WHERE USERNAME = :userName AND TYPE=upper(:type)";
                using (var cmd = new OracleCommand(sql, _oracleConnection))
                {
                    cmd.Parameters.Add("userName", OracleDbType.Varchar2).Value = userName;
                    cmd.Parameters.Add("type", OracleDbType.Varchar2).Value = type;

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

        public string GenerateAndSaveSecret(int tenantId, string userName, string type = "TOTP")
        {
            OracleTransaction transaction = null;

            try
            {
                CleanSecrets(userName, type);
                _oracleConnection.Open();
                string validate_field = type.ToUpper() == "SMS" ? "phone" : "email";
                string secret = SecretGenerator.GenerateRandomSecret();
                string sql = $"insert into T010_OTP_SECRETS " +
                             $"SELECT tt.username,:secret ," +
                             $"sysdate + INTERVAL '{_configuration["KeyInterval"]?.ToString() ?? "600"}' SECOND," +
                             $"UPPER(:type) " +
                             $"from t010_authorizations tt " +
                             $"where tt.username = :username " +
                             $"and tt.tenant_id = :tenantId " +
                             $"and tt.valid_{validate_field} = 'Y'";

                using (var cmd = new OracleCommand(sql, _oracleConnection))
                {
                    cmd.Parameters.Add("secret", OracleDbType.Varchar2).Value = _encryptionService.Encrypt(secret);
                    cmd.Parameters.Add("type", OracleDbType.Varchar2).Value = type;
                    cmd.Parameters.Add("username", OracleDbType.Varchar2).Value = userName;
                    cmd.Parameters.Add("tenantId", OracleDbType.Int64).Value = tenantId;
                    transaction = _oracleConnection.BeginTransaction();
                    cmd.Transaction = transaction;
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0) // No rows inserted
                    {
                        transaction.Rollback();
                        return null; // Return null when no records are inserted
                    }
                }
                transaction.Commit();

                return secret; // Return the secret if rows were inserted
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                // Log the exception here
                return null; // Return null in case of exception
            }
            finally
            {
                if (_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }
        }


        public int GetUserId(string userName, string? identifier)
        {
            try
            {
                _oracleConnection.Open();

                string sql = "SELECT user_id FROM T010_AUTHORIZATIONS " +
                             "WHERE username = :userName";

                // Add logic to validate the user against the database
                using var cmd = new OracleCommand(sql, _oracleConnection);
                cmd.Parameters.Add(new OracleParameter("userName", OracleDbType.Varchar2)).Value = userName;

                // Execute the query
                var key = cmd.ExecuteScalar();

                // Check if the user exists in the database
                return Int32.Parse(key.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }
        }

        public Dictionary<string, string> GetUser(int userId)
        {
            try
            {
                _oracleConnection.Open();


                string sql = $@"{this.userFieldsSql}
                          FROM T010_AUTHORIZATIONS t10
                          WHERE t10.USER_ID = :userId 
                          AND t10.SUSPEND_STATUS = 'NO'
                          AND (t10.locked_until is null OR t10.locked_until < sysdate)";

                using var cmd = new OracleCommand(sql, _oracleConnection);
                cmd.Parameters.Add("userId", OracleDbType.Int64).Value = userId;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Initialize a new dictionary to hold the row data
                        var rowData = new Dictionary<string, string>();

                        // Iterate through all columns in the row
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            // Add column name and value to the dictionary
                            // Use .ToString() method to ensure the value is a string
                            // Adjust this part if you need to handle complex types or null values differently
                            rowData[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
                        }

                        return rowData;
                    }
                    else
                    {
                        throw new KeyNotFoundException("User not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Consider logging the exception details
                throw;
            }
            finally
            {
                if (_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }

        }

        public Dictionary<string, string> GetUser(string username, string identifier)
        {
            try
            {
                _oracleConnection.Open();


                string sql = $@"{this.userFieldsSql}
                          FROM T010_AUTHORIZATIONS t10
                          WHERE t10.USERNAME = :username 
                          AND t10.TOTP_IDENTIFIER = :identifier
                          AND t10.SUSPEND_STATUS = 'NO'
                          AND (t10.locked_until is null OR t10.locked_until < sysdate)";

                using var cmd = new OracleCommand(sql, _oracleConnection);
                cmd.Parameters.Add("username", OracleDbType.Varchar2).Value = username;
                cmd.Parameters.Add("identifier", OracleDbType.Varchar2).Value = identifier;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Initialize a new dictionary to hold the row data
                        var rowData = new Dictionary<string, string>();

                        // Iterate through all columns in the row
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            // Add column name and value to the dictionary
                            // Use .ToString() method to ensure the value is a string
                            // Adjust this part if you need to handle complex types or null values differently
                            rowData[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
                        }

                        return rowData;
                    }
                    else
                    {
                        throw new KeyNotFoundException("User not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Consider logging the exception details
                throw;
            }
            finally
            {
                if (_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }

        }



        string IVerificationService.GetUserSecret(int tenantId, string userName, string identifer, string type)
        {
            try
            {
                _oracleConnection.Open();


                string sql = @"SELECT ts.SECRET FROM T010_OTP_SECRETS ts , T010_Authorizations t10
                                WHERE ts.USERNAME = :username
                                AND   ts.Username = t10.username
                                AND  ts.TYPE = upper(:type)
                                AND ts.EXPIRATION > SYSTIMESTAMP ";

                if (type.ToLower() == "sms")
                {
                    sql += " and t10.valid_phone = 'Y'";
                }
                else if (type.ToLower() == "email")
                {
                    sql += " and t10.valid_email = 'Y'";
                }

                using var cmd = new OracleCommand(sql, _oracleConnection);
                cmd.Parameters.Add("username", OracleDbType.Varchar2).Value = userName;
                cmd.Parameters.Add("type", OracleDbType.Varchar2).Value = type;

                var key = cmd.ExecuteScalar();


                return _encryptionService.Decrypt((key ?? "").ToString());


            }
            catch (Exception ex)
            {
                // Consider logging the exception details
                throw;
            }
            finally
            {
                if (_oracleConnection.State == System.Data.ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }
        }


        public string SetAndGetUserJoopyToken(int userId, string tokenType = "")
        {
            try
            {
                _oracleConnection.Open();
                string token = Guid.NewGuid().ToString();
                if (tokenType != string.Empty)
                {
                    tokenType = "_" + tokenType.ToUpper();
                }
                string sql = $@"update t010_authorizations t
                                set t.USER_TOKEN{tokenType} = '{token}' 
                                where t.user_id = :userId";

                using var cmd = new OracleCommand(sql, _oracleConnection);
                cmd.Parameters.Add("userId", OracleDbType.Int64).Value = userId;

                cmd.ExecuteNonQuery();

                return token;

            }
            catch (Exception ex)
            {
                // Consider logging the exception details
                throw;
            }
            finally
            {
                if (_oracleConnection.State == System.Data.ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }
        }

        public string GetRegistrationToken(int tenantId, string userName, string smsOrEmail)
        {
            return GenerateAndSaveSecret(tenantId, userName, smsOrEmail);
        }

        public void ValidateUser(int userId, string smsOrEmail)
        {
            try
            {
                _oracleConnection.Open();


                string field = smsOrEmail.ToUpper() == "EMAIL" ? "VALID_EMAIL" : "VALID_PHONE";

                string sql = $@"update t010_authorizations t
                                set t.{field} = 'Y' 
                                where t.user_id = :userId";

                using var cmd = new OracleCommand(sql, _oracleConnection);
                cmd.Parameters.Add("userId", OracleDbType.Int64).Value = userId;

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                // Consider logging the exception details
                throw;
            }
            finally
            {
                if (_oracleConnection.State == System.Data.ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }
            }
        }



    }
}
