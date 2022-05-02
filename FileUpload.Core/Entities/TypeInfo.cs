using System;
using System.Collections.Generic;
using System.Text;

namespace FileUpload.Core.Entities
{
    public class CsvFieldTypeInfo
    {
        public string PropertyName { get; set; }
        public string TypeName { get; set; }
        public int? Length { get; set; }

        public CsvFieldTypeInfo(string propertyName, string typeName, int? length = 0)
        {
            this.PropertyName = propertyName;
            this.TypeName = typeName;
            this.Length = length;
        }
    }

    public class XmlFieldTypeInfo
    {
        public string FieldName { get; set; }
        public string PropertyName { get; set; }
        public string TypeName { get; set; }
        public int? Length { get; set; }

        public XmlFieldTypeInfo(string fieldName, string propertyName, string typeName, int? length = 0)
        {
            this.FieldName = fieldName;
            this.PropertyName = propertyName;
            this.TypeName = typeName;
            this.Length = length;
        }
    }
}
