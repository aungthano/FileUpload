namespace FileUpload.Core.DBHelper
{
    public class InputParameter
    {
        public string ParameterName { get; set; }
        public object Value { get; set; }

        public InputParameter() { }

        public InputParameter(string parameterName, object value)
        {
            this.ParameterName = parameterName;
            this.Value = value;
        }
    }
}
