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
    public class PatientController : ControllerBase
    {
        private readonly ICosmosDbService<Patient> _cosmosDbService;
        public PatientController(ICosmosDbService<Patient> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService ?? throw new ArgumentNullException(nameof(cosmosDbService));
        }


        // GET /Patient
        [HttpGet]
        [ProducesResponseType(typeof(List<Patient>), 200)]
        public async Task<IActionResult> List()
        {
            return Ok(await _cosmosDbService.GetMultipleAsync("SELECT * FROM c WHERE c.type='patient'"));
        }

        // GET /Patient/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(List<Patient>), 200)]
        public async Task<IActionResult> Get(string id)
        {
            return Ok(await _cosmosDbService.GetAsync(id,id));
        }

        // POST /Patient
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Patient item)
        {
            item.id = item.PatientId;
            await _cosmosDbService.AddAsync(item);
            return CreatedAtAction(nameof(Get), new { id = item.id }, item);
        }

        // PUT /Patient/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromBody] Patient item)
        {
            var result = await _cosmosDbService.UpdateAsync(item.PatientId, item);            
            return new StatusCodeResult((int)result.StatusCode);
        }

        // DELETE /Patient/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _cosmosDbService.DeleteAsync(id, id);
            return new StatusCodeResult((int)result.StatusCode);
        }

        [HttpGet]
        [Route("GetPatient")]
        public Patient GetPatient()
        {            
            Patient pt = new Patient();
            pt.id = Guid.NewGuid().ToString();
            pt.PatientId = "5";
            pt.Name = "Contoso Patient 5";
            return pt;
        }
    }
}
