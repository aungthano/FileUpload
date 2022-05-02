using FileUpload.Core.Entities;
using FileUpload.Core.Extensions;
using FileUpload.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace FileUpload.Core.Services
{
    public class XMLFileService
    {
        public MessageResult<List<T>> ReadXMLFile<T>(string filePath)
            where T : class, new()
        {
            var msgResult = new MessageResult<List<T>>();
            List<T> result = new List<T>();

            List<XmlFieldTypeInfo> fieldInfos = new List<XmlFieldTypeInfo>
            {
                new XmlFieldTypeInfo("Transaction", "TransId", "string", 50),
                new XmlFieldTypeInfo("Amount", "Amount", "decimal"),
                new XmlFieldTypeInfo("CurrencyCode", "CurrCode", "string"),
                new XmlFieldTypeInfo("TransactionDate", "TransDate", "datetime"),
                new XmlFieldTypeInfo("Status", "Status", "string"),
            };

            try
            {
                int errorCount = 0;

                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    int rowIndex = 0;
                    Dictionary<string, object> fields = null;

                    while (reader.Read())
                    {
                        if (reader.Name == "Transaction" && !reader.IsStartElement())
                        {
                            if (fields != null)
                            {
                                var obj = fields.ToObject<T>();
                                result.Add(obj);
                            }
                        }

                        if (!reader.IsStartElement()) continue;
                        if (string.IsNullOrEmpty(reader.Name)) continue;

                        string field = string.Empty;

                        var fieldInfo = fieldInfos.SingleOrDefault(fi => fi.FieldName == reader.Name);
                        if (fieldInfo != null)
                        {
                            try
                            {
                                bool isValidData = true;

                                // special case Transaction, since value at attribute
                                if (reader.Name == "Transaction")
                                {
                                    fields = new Dictionary<string, object>();
                                    rowIndex += 1;

                                    field = reader.GetAttribute("id");
                                }
                                else
                                {
                                    field = reader.ReadInnerXml();
                                }

                                // validate empty value
                                if (string.IsNullOrEmpty(field))
                                {
                                    msgResult.IsSucceed = false;
                                    msgResult.Message += $"\nError at [Record Index] - {rowIndex}, [Field] = {fieldInfo.FieldName}, Error Info - Value missing!";
                                    isValidData = false;
                                    errorCount += 1;
                                }

                                // validate string length
                                if (fieldInfo.Length > 0 && field.Length > fieldInfo.Length)
                                {
                                    msgResult.IsSucceed = false;
                                    msgResult.Message += $"\nError at [Record Index] - {rowIndex}, [Field] = {fieldInfo.FieldName}, Error Info - Input Data is longer than expected! Maximum Text length 50";
                                    isValidData = false;
                                    errorCount += 1;
                                }

                                // convert types
                                if (isValidData)
                                {
                                    object value = null;
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
                                msgResult.Message += $"\nError at [Record Index] - {rowIndex}, [Field] - {fieldInfo.FieldName}, Error Info - {ex.Message}";
                                msgResult.AdditionalInformation = ex.ToString();
                                errorCount += 1;
                            }
                        }
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
            return DateTime.ParseExact(o.ToString(), "yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
