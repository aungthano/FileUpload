using FileUpload.Core.DBHelper;
using FileUpload.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FileUpload.Core.Services
{
    public class InvoiceTransService
    {
        DBManager _dbManager;

        public InvoiceTransService(DBManager dbManager)
        {
            _dbManager = dbManager;
        }

        public IEnumerable<InvoiceTrans> GetAllInvTransByCurrency(string currCode)
        {
            var command = new DBCommand("spl_invtrans_by_currency");
            command.InputParameters = new List<InputParameter>()
            {
                new InputParameter("CurrCode", currCode)
            };
            return _dbManager.Select<InvoiceTrans>(command);
        }

        public IEnumerable<InvoiceTrans> GetAllInvTransByDate(DateTime fromTransDate, DateTime toTransDate)
        {
            var command = new DBCommand("spl_invtrans_by_transdate");
            command.InputParameters = new List<InputParameter>()
            {
                new InputParameter("FromTransDate", fromTransDate),
                new InputParameter("ToTransDate", toTransDate)
            };
            return _dbManager.Select<InvoiceTrans>(command);
        }

        public IEnumerable<InvoiceTrans> GetAllInvTransByStatus(string status)
        {
            var command = new DBCommand("spl_invtrans_by_status");
            command.InputParameters = new List<InputParameter>()
            {
                new InputParameter("Status", status),
            };
            return _dbManager.Select<InvoiceTrans>(command);
        }

        public MessageStatus CreateInvoiceTrans(List<InvoiceTrans> invTrans)
        {
            DataTable dtInvTrans = new DataTable("InvoiceTransTableType");
            dtInvTrans.Columns.Add("TransId", typeof(string));
            dtInvTrans.Columns.Add("Amount", typeof(decimal));
            dtInvTrans.Columns.Add("CurrCode", typeof(string));
            dtInvTrans.Columns.Add("TransDate", typeof(DateTime));
            dtInvTrans.Columns.Add("Status", typeof(string));

            foreach (var invTran in invTrans)
            {
                dtInvTrans.Rows.Add(invTran.TransId, invTran.Amount, invTran.CurrCode, invTran.TransDate, invTran.Status);
            }

            var command = new DBCommand("spc_invtrans");
            command.InputParameters = new List<InputParameter>()
            {
                new InputParameter("@TblInvTrans", dtInvTrans)
            };
            var msgStatus = this._dbManager.Execute(command);
            return msgStatus;
        }
    }
}
