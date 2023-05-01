using InvoiceSystem.Core.Entities;
using InvoiceSystem.Core.ExternalModels;
using InvoiceSystem.Core.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceSystem.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(IInvoiceService invoiceService,
                                 ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateInvoice([FromBody] ExternalInvoice request)
        {
            var invoice = await _invoiceService.GenerateInvoice(request);
            if (invoice == null)
            {
                return BadRequest("An error occured while creating invoice");
            }
            return Ok(invoice);
        }
    }
}
