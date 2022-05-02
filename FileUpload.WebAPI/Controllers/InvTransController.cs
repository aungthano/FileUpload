using AutoMapper;
using FileUpload.Core.Entities;
using FileUpload.Core.Services;
using FileUpload.WebAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace FileUpload.WebAPI.Controllers
{
    [ApiController]
    public class InvTransController : ControllerBase
    {
        private readonly InvoiceTransService _invoiceTransService;
        private readonly IMapper _mapper;

        public InvTransController(
            IMapper mapper,
            InvoiceTransService invoiceTransService)
        {
            _mapper = mapper;
            _invoiceTransService = invoiceTransService;
        }

        [HttpGet]
        [Route("InvoiceTrans/ByCurrency")]
        public ActionResult<IEnumerable<InvTransDto>> GetAllInvTransByCurrency(string currCode)
        {
            List<InvTransDto> invTransDtos = new List<InvTransDto>();

            var invTrans = _invoiceTransService.GetAllInvTransByCurrency(currCode);
            foreach (var invTran in invTrans)
            {
                var invTransDto = _mapper.Map<InvTransDto>(invTran);
                invTransDtos.Add(invTransDto);
            }
            
            return Ok(invTransDtos);
        }

        [HttpGet]
        [Route("InvoiceTrans/ByTransDate")]
        public ActionResult<IEnumerable<InvoiceTrans>> GetAllInvTransByTransDate(DateTime fromTransDate, DateTime toTransDate)
        {
            List<InvTransDto> invTransDtos = new List<InvTransDto>();

            var invTrans = _invoiceTransService.GetAllInvTransByDate(fromTransDate, toTransDate);
            foreach (var invTran in invTrans)
            {
                var invTransDto = _mapper.Map<InvTransDto>(invTran);
                invTransDtos.Add(invTransDto);
            }

            return Ok(invTransDtos);
        }

        [HttpGet]
        [Route("InvoiceTrans/ByStatus")]
        public ActionResult<IEnumerable<InvoiceTrans>> GetAllInvTransByStatus(string status)
        {
            List<InvTransDto> invTransDtos = new List<InvTransDto>();

            var invTrans = _invoiceTransService.GetAllInvTransByStatus(status);
            foreach (var invTran in invTrans)
            {
                var invTransDto = _mapper.Map<InvTransDto>(invTran);
                invTransDtos.Add(invTransDto);
            }

            return Ok(invTransDtos);
        }
    }
}
