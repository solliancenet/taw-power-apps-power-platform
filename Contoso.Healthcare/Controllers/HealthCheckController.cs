using Contoso.Healthcare.Models;
using Contoso.Healthcare.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contoso.Healthcare.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly ICosmosDbService<HealthCheck> _cosmosDbService;
        public HealthCheckController(ICosmosDbService<HealthCheck> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService ?? throw new ArgumentNullException(nameof(cosmosDbService));
        }


        // GET /HealthCheck
        [HttpGet]
        [ProducesResponseType(typeof(List<HealthCheck>), 200)]
        public async Task<IActionResult> List()
        {
            return Ok(await _cosmosDbService.GetMultipleAsync("SELECT * FROM c WHERE c.type='status'"));
        }

        // GET /HealthCheck/5/23dbf68d-3f40-41de-ae1b-8e3558cd17f9
        [HttpGet("{patientId}/{id}")]
        [ProducesResponseType(typeof(List<HealthCheck>), 200)]
        public async Task<IActionResult> Get(string patientId, string id)
        {
            return Ok(await _cosmosDbService.GetAsync(id, patientId));
        }

        // POST /HealthCheck
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HealthCheck item)
        {
            item.id = Guid.NewGuid().ToString();
            await _cosmosDbService.AddAsync(item);
            return CreatedAtAction(nameof(Get), new { patientId = item.PatientId, id = item.id }, item);
        }

        // PUT /HealthCheck/23dbf68d-3f40-41de-ae1b-8e3558cd17f9
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromBody] HealthCheck item)
        {
            var result = await _cosmosDbService.UpdateAsync(item.id, item);
            return new StatusCodeResult((int)result.StatusCode);
        }

        // DELETE /HealthCheck/5/23dbf68d-3f40-41de-ae1b-8e3558cd17f9
        [HttpDelete("{patientId}/{id}")]
        public async Task<IActionResult> Delete(string patientId, string id)
        {
            var result = await _cosmosDbService.DeleteAsync(id,patientId);
            return new StatusCodeResult((int)result.StatusCode);
        }

        [HttpGet]
        [Route("GetStatus")]
        public HealthCheck GetStatus()
        {
            var symptoms = new string[]{"Hair loss", "Internal bleeding", "Temporary blindness", "Ennui"};

            HealthCheck hc = new HealthCheck();
            hc.id = Guid.NewGuid().ToString();
            hc.PatientId = "5";
            hc.Date = DateTime.Now;
            hc.HealthStatus = "I feel unwell";
            hc.Symptoms = symptoms;
            return hc;
        }
    }
}
