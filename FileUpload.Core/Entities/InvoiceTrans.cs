using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FileUpload.Core.Entities
{
    public class InvoiceTrans
    {
        public string TransId { get; set; }
        public decimal Amount { get; set; }
        public string CurrCode { get; set; }
        public DateTime TransDate { get; set; }
        public string Status { get; set; }
    }
}
