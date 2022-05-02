using System;
using System.Data;

namespace FileUpload.Core.DBHelper
{
    public class OutputParameter
    {
        public string ParameterName { get; set; }
        public DbType DbType { get; set; }
        public SqlDbType SqlDbType { get; set; }
        public int Size { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public object Value { get; set; }

        public OutputParameter() { }

        public OutputParameter(string parameterName, DbType dbType)
            : this(parameterName, dbType, 0, 0, 0, (object)DBNull.Value)
        {
        }

        public OutputParameter(string parameterName, DbType dbType, int size)
            : this(parameterName, dbType, size, 0, 0, (object)DBNull.Value)
        {
        }

        public OutputParameter(string parameterName, DbType dbType, int size, object value)
            : this(parameterName, dbType, size, 0, 0, value)
        {
        }

        public OutputParameter(string parameterName, DbType dbType, byte precision, byte scale, object value)
            : this(parameterName, dbType, 0, precision, scale, value)
        {
        }

        public OutputParameter(string parameterName, DbType dbType, int size, byte precision, byte scale, object value)
        {
            this.ParameterName = parameterName;
            this.DbType = dbType;
            this.Size = size;
            this.Precision = precision;
            this.Scale = scale;
            this.Value = value;
        }
    }
}
