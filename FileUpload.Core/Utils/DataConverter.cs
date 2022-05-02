using System;
using System.Collections.Generic;
using System.Text;

namespace FileUpload.Core.Utils
{
    public static class DataConverter
    {
        public static T ConvertToType<T>(object o)
        {
            Type conversionType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T)Convert.ChangeType(o, conversionType);

        }
    }
}
