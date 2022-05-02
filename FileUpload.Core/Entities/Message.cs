using System;
using System.Collections.Generic;
using System.Text;

namespace FileUpload.Core
{
    public class MessageStatus
    {
        public bool IsSucceed { get; set; }
        public int MessageNumber { get; set; }
        public string Message { get; set; }
        public string AdditionalInformation { get; set; }
        public IDictionary<string, object> OutputParamValues { get; set; }
        public int RowsAffected { get; set; }
    }

    public class MessageResult<T> : MessageStatus
    {
        public T Result { get; set; }
    }
}
