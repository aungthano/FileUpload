using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace FileUpload.Core.DBHelper
{
    public class DBManager
    {
        public string ConnectionString { get; internal set; }

        public DBManager(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            this.ConnectionString = connectionString;
        }

        public IEnumerable<T> Select<T>(DBCommand command)
        {
            IEnumerable<T> result = null;

            DynamicParameters parameters = GenerateParameters(command.InputParameters);
            using (var dbConnection = new SqlConnection(this.ConnectionString))
            {
                result = dbConnection.Query<T>(command.CommandText, parameters, commandType: command.CommandType);
            }

            return result;
        }

        public IEnumerable<dynamic> Select(DBCommand command)
        {
            return this.Select<dynamic>(command);
        }

        public T GetValue<T>(DBCommand command)
        {
            T result = default;

            DynamicParameters parameters = GenerateParameters(command.InputParameters);
            using (var dbConnection = new SqlConnection(this.ConnectionString))
            {
                result = dbConnection.QueryFirstOrDefault<T>(command.CommandText, parameters, commandType: command.CommandType);
            }

            return result;
        }

        public object GetValue(DBCommand command)
        {
            return this.GetValue<object>(command);
        }

        public MessageResult<IEnumerable<T>> GetMsgResult<T>(DBCommand command)
        {
            var msgResult = new MessageResult<IEnumerable<T>>();
            IDbTransaction transactionScope = null;
            Dictionary<string, object> outputParamValues = new Dictionary<string, object>();

            using (var dbConnection = new SqlConnection(this.ConnectionString))
            {
                try
                {
                    DynamicParameters parameters = GenerateParameters(command.InputParameters, command.OutputParameters);
                    if (!parameters.ParameterNames.AsList().Contains("o_succeed"))
                    {
                        parameters.Add("o_succeed", DbType.Boolean, direction: ParameterDirection.Output);
                    }
                    if (!parameters.ParameterNames.AsList().Contains("o_msgnum"))
                    {
                        parameters.Add("o_msgnum", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    }
                    if (!parameters.ParameterNames.AsList().Contains("o_addinfo"))
                    {
                        parameters.Add("o_addinfo", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    }

                    dbConnection.Open();
                    transactionScope = command.DoesUseTransaction ? dbConnection.BeginTransaction() : null;

                    msgResult.Result = dbConnection.Query<T>(command.CommandText, parameters, commandType: command.CommandType, transaction: transactionScope);

                    if (command.OutputParameters != null && command.OutputParameters.Count > 0)
                    {
                        foreach (var outputParameter in command.OutputParameters)
                        {
                            var value = parameters.Get<object>(outputParameter.ParameterName);
                            if (msgResult.OutputParamValues == null)
                            {
                                msgResult.OutputParamValues = new Dictionary<string, object>();
                            }
                            msgResult.OutputParamValues.Add(outputParameter.ParameterName, value);
                        }
                    }

                    msgResult.IsSucceed = parameters.Get<int>("o_succeed") != 0;
                    msgResult.MessageNumber = parameters.Get<int>("o_msgnum");
                    msgResult.AdditionalInformation = parameters.Get<string>("o_addinfo");

                    if (transactionScope != null)
                    {
                        if (msgResult.IsSucceed)
                            transactionScope.Commit();
                        else
                            transactionScope.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    if (transactionScope != null)
                        transactionScope.Rollback();

                    msgResult.IsSucceed = false;
                    msgResult.MessageNumber = Constant.MessageNumber.DefaultError;
                    msgResult.Message = ex.Message;
                    msgResult.AdditionalInformation = ex.ToString();
                }
            }

            return msgResult;
        }

        public MessageResult<IEnumerable<dynamic>> GetMsgResult(DBCommand command)
        {
            return this.GetMsgResult<dynamic>(command);
        }

        public MessageStatus Execute(DBCommand command)
        {
            var msgStatus = new MessageStatus();
            SqlTransaction transactionScope = null;

            using (var dbConnection = new SqlConnection(this.ConnectionString))
            {
                try
                {
                    DynamicParameters parameters = GenerateParameters(command.InputParameters, command.OutputParameters);
                    if (!parameters.ParameterNames.AsList().Contains("o_succeed"))
                    {
                        parameters.Add("o_succeed", DbType.Boolean, direction: ParameterDirection.Output);
                    }
                    if (!parameters.ParameterNames.AsList().Contains("o_msgnum"))
                    {
                        parameters.Add("o_msgnum", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    }
                    if (!parameters.ParameterNames.AsList().Contains("o_addinfo"))
                    {
                        parameters.Add("o_addinfo", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    }

                    dbConnection.Open();
                    transactionScope = command.DoesUseTransaction ? dbConnection.BeginTransaction() : null;

                    msgStatus.RowsAffected = dbConnection.Execute(command.CommandText, parameters, transactionScope, commandType: command.CommandType);

                    if (command.OutputParameters != null && command.OutputParameters.Count > 0)
                    {
                        foreach (var outputParameter in command.OutputParameters)
                        {
                            var value = parameters.Get<object>(outputParameter.ParameterName);
                            if (msgStatus.OutputParamValues == null)
                            {
                                msgStatus.OutputParamValues = new Dictionary<string, object>();
                            }
                            msgStatus.OutputParamValues.Add(outputParameter.ParameterName, value);
                        }
                    }

                    msgStatus.IsSucceed = parameters.Get<int>("o_succeed") != 0;
                    msgStatus.MessageNumber = parameters.Get<int>("o_msgnum");
                    msgStatus.AdditionalInformation = parameters.Get<string>("o_addinfo");

                    if (transactionScope != null)
                    {
                        if (msgStatus.IsSucceed)
                            transactionScope.Commit();
                        else
                            transactionScope.Rollback();
                    }
                }
                catch (DbException ex)
                {
                    if (transactionScope != null)
                        transactionScope.Rollback();

                    msgStatus = new MessageStatus()
                    {
                        IsSucceed = false,
                        MessageNumber = Constant.MessageNumber.DefaultError,
                        Message = ex.Message,
                        AdditionalInformation = ex.ToString()
                    };
                }
            }

            return msgStatus;
        }

        #region Internal Methods

        private DynamicParameters GenerateParameters(IList<InputParameter> inputParameters, IList<OutputParameter> outputParameters = null)
        {
            DynamicParameters parameters = new DynamicParameters();
            object value = null;

            if (inputParameters != null)
            {
                foreach (var parameter in inputParameters)
                {
                    if (parameter.Value != null)
                    {
                        if (parameter.Value.GetType().Equals(typeof(string)))
                        {
                            value = parameter.Value.ToString().Trim();
                        }
                        else if (parameter.Value.GetType().Equals(typeof(DataTable)))
                        {
                            var dataTable = (DataTable)parameter.Value;
                            
                            if (dataTable.Rows.Count > 0)
                                value = dataTable.AsTableValuedParameter(dataTable.TableName);
                            else continue;
                        }
                        else value = parameter.Value;
                    }
                    else value = null;

                    parameters.Add(parameter.ParameterName, value);
                }
            }

            if (outputParameters != null)
            {
                foreach (var parameter in outputParameters)
                {
                    if (parameter.Value != null)
                    {
                        if (parameter.Value.GetType().Equals(typeof(string)))
                        {
                            value = parameter.Value.ToString().Trim();
                        }
                        else value = parameter.Value;
                    }
                    else value = parameter.Value;

                    int? size = null;
                    if (parameter.Size != 0)
                    {
                        size = parameter.Size;
                    }

                    byte? precision = null;
                    if (parameter.Precision != 0)
                    {
                        precision = parameter.Precision;
                    }

                    byte? scale = null;
                    if (parameter.Scale != 0)
                    {
                        scale = parameter.Scale;
                    }
                    
                    parameters.Add(parameter.ParameterName,
                        value,
                        parameter.DbType,
                        ParameterDirection.InputOutput,
                        size,
                        precision,
                        scale
                    );
                }
            }

            parameters.RemoveUnused = false;
            return parameters;
        }

        private IList<IDbDataParameter> GenerateParameters2(IList<InputParameter> inputParameters, IList<OutputParameter> outputParameters = null)
        {
            IList<IDbDataParameter> parameters = new List<IDbDataParameter>();
            object value = null;

            if (inputParameters != null)
                foreach (var parameter in inputParameters)
                {
                    if (parameter.Value != null)
                    {
                        if (parameter.Value.GetType().Equals(typeof(string)))
                            value = parameter.Value.ToString().Trim();
                        else if (parameter.Value.GetType().Equals(typeof(DataTable)))
                        {
                            var dataTable = (DataTable)parameter.Value;

                            if (dataTable.Rows.Count > 0)
                                value = dataTable;
                            else
                                continue;
                        }
                        else
                            value = parameter.Value;

                    }
                    else
                        value = DBNull.Value;

                    parameters.Add(this.CreateInputParameter(parameter.ParameterName, value));
                }

            if (outputParameters != null)
                foreach (var parameter in outputParameters)
                {
                    value = parameter.Value != null ? (parameter.Value.GetType().Equals(typeof(string)) ? parameter.Value.ToString().Trim() : parameter.Value) : (object)DBNull.Value;
                    parameters.Add(this.CreateOutputParameter(parameter.ParameterName, parameter.SqlDbType, parameter.Size, parameter.Precision, parameter.Scale, value));
                }

            return parameters;
        }

        public IDbDataParameter CreateInputParameter(string name, object value)
        {
            return new SqlParameter
            {
                ParameterName = name,
                Value = value
            };
        }

        private IDbDataParameter CreateOutputParameter(string name, SqlDbType sqlDbType)
        {
            return this.CreateOutputParameter(name, sqlDbType, 1);
        }

        private IDbDataParameter CreateOutputParameter(string name, SqlDbType sqlDbType, int size)
        {
            return this.CreateOutputParameter(name, sqlDbType, size, 0, 0, (object)DBNull.Value);
        }

        private IDbDataParameter CreateOutputParameter(string name, SqlDbType sqlDbType, byte precision, byte scale)
        {
            return this.CreateOutputParameter(name, sqlDbType, 0, precision, scale, (object)DBNull.Value);
        }

        private IDbDataParameter CreateOutputParameter(string name, SqlDbType sqlDbType, int size, byte precision, byte scale, object value)
        {
            return new SqlParameter
            {
                ParameterName = name,
                SqlDbType = sqlDbType,
                Size = size,
                Precision = precision,
                Scale = scale,
                Value = value,
                Direction = ParameterDirection.InputOutput
            };
        }

        #endregion
    }
}
