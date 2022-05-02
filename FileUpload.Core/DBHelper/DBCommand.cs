using System;
using System.Collections.Generic;
using System.Data;

namespace FileUpload.Core.DBHelper
{
    public class DBCommand
    {
        public string CommandText { get; set; }
        public bool DoesUseTransaction { get; set; }
        public CommandType CommandType { get; set; }
        public IList<InputParameter> InputParameters { get; set; }
        public IList<OutputParameter> OutputParameters { get; set; }

        public DBCommand() { }

        public DBCommand(string commandText, bool doesUseTransaction = true, IList<InputParameter> inputParameters = null, IList<OutputParameter> outputParameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            this.CommandText = commandText ?? throw new ArgumentNullException(nameof(commandText));
            this.DoesUseTransaction = doesUseTransaction;
            this.InputParameters = inputParameters;
            this.OutputParameters = outputParameters;
            this.CommandType = commandType;
        }
    }
}
