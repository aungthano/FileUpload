using FileUpload.Core.Entities;
using FileUpload.Core.Extensions;
using FileUpload.Core.Utils;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileUpload.Core.Services
{
    public class CSVFileService
    {
        public MessageResult<List<T>> ReadCSVFile<T>(string filePath)
            where T : class, new()
        {
            var msgResult = new MessageResult<List<T>>();
            List<T> result = new List<T>();

            List<CsvFieldTypeInfo> fieldInfos = new List<CsvFieldTypeInfo>
            {
                new CsvFieldTypeInfo("TransId", "string", 50),
                new CsvFieldTypeInfo("Amount", "decimal"),
                new CsvFieldTypeInfo("CurrCode", "string"),
                new CsvFieldTypeInfo("TransDate", "datetime"),
                new CsvFieldTypeInfo("Status", "string"),
            };

            try
            {
                int errorCount = 0;
                char delimiter = ',';

                var reader = new StreamReader(filePath, Encoding.Default);
                using (CsvReader csv = new CsvReader(reader, false, delimiter))
                {
                    csv.DefaultParseErrorAction = ParseErrorAction.ThrowException;
                    csv.SkipEmptyLines = true;

                    int fieldCount = csv.FieldCount;

                    // validate fields
                    if (fieldCount != 5)
                    {
                        msgResult.IsSucceed = false;
                        msgResult.Message = "CSV file format not match!";
                        return msgResult;
                    }

                    while (csv.ReadNextRecord())
                    {
                        long rowIndex = csv.CurrentRecordIndex;
                        Dictionary<string, object> fields = new Dictionary<string, object>();
                        for (int i = 0; i < csv.FieldCount; i++)
                        {
                            try
                            {
                                bool isValidData = true;
                                var fieldInfo = fieldInfos[i];
                                string field = csv[i].Trim('”').Trim('“');
                                object value = null;

                                // validate empty value
                                if (string.IsNullOrEmpty(field))
                                {
                                    msgResult.IsSucceed = false;
                                    msgResult.Message += $"\nError at [Record Index] - {rowIndex}, [Column Index] = {i}, Error Info - Value missing!";
                                    isValidData = false;
                                    errorCount += 1;
                                }

                                // validate string length
                                if (fieldInfo.Length > 0 && field.Length > fieldInfo.Length)
                                {
                                    msgResult.IsSucceed = false;
                                    msgResult.Message += $"\nError at [Record Index] - {rowIndex}, [Column Index] = {i}, Error Info - Input Data is longer than expected! Maximum Text length 50";
                                    isValidData = false;
                                    errorCount += 1;
                                }

                                // convert types
                                if (isValidData)
                                {
                                    if (fieldInfo.TypeName == "string")
                                    {
                                        value = DataConverter.ConvertToType<string>(field);
                                    }
                                    else if (fieldInfo.TypeName == "decimal")
                                    {
                                        field = field.Replace(",", "");
                                        value = DataConverter.ConvertToType<decimal>(field);
                                    }
                                    else if (fieldInfo.TypeName == "datetime")
                                    {
                                        value = ConvertToDateTime(field);
                                    }

                                    fields.Add(fieldInfo.PropertyName, value);
                                }
                            }
                            catch (Exception ex)
                            {
                                msgResult.IsSucceed = false;
                                msgResult.Message += $"\nError at [Record Index] - {csv.CurrentRecordIndex}, [Column Index] - {i}, Error Info - {ex.Message}";
                                msgResult.AdditionalInformation = ex.ToString();
                                errorCount += 1;
                            }
                        }

                        var obj = fields.ToObject<T>();
                        result.Add(obj);
                    }
                }

                if (errorCount > 0)
                    return msgResult;

                msgResult.IsSucceed = true;
                msgResult.Message = "File uploaded successfully.";
                msgResult.Result = result;
                return msgResult;

            }
            catch (Exception ex)
            {
                msgResult.IsSucceed = false;
                msgResult.Message = ex.Message;
                msgResult.AdditionalInformation = ex.ToString();
                return msgResult;
            }
        }

        private DateTime ConvertToDateTime(object o)
        {
            return DateTime.ParseExact(o.ToString(), "dd/MM/yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
