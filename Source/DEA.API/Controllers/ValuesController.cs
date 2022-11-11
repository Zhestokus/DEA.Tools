using DEA.Core;
using DEA.Tools;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DEA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly DeaProcessor _deaProcessor;

        public ValuesController(DeaProcessor deaProcessor)
        {
            _deaProcessor = deaProcessor;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var eventName = $"{nameof(ValuesController)}.{nameof(Get)}";
            var result = await _deaProcessor.ProcessAsync<List<String>>(eventName);

            return result;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var eventName = $"{nameof(ValuesController)}.{nameof(Get)}";
            var result = await _deaProcessor.ProcessAsync<String>(eventName, id);

            return result;
        }

        // POST api/values
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] String value)
        {
            var eventName = $"{nameof(ValuesController)}.{nameof(Post)}";
            var result = await _deaProcessor.ProcessAsync<String>(eventName, value);

            return Ok(result);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        [DeaEvent]
        public async Task<ActionResult> Put(int id, [FromBody] string value)
        {
            var result = await _deaProcessor.ProcessAsync<String>(id, value);
            return Ok(result);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        [DeaEvent("ValuesController.Delete")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _deaProcessor.ProcessAsync<String>(id);
            return Ok(result);
        }

        [HttpGet("Test")]
        public async Task<ActionResult> Test(byte? @byte, short? @short, int? @int, long? @long, float? @float, double? @double, Guid? @guid, DateTime? @dateTime)
        {
            var eventName = $"{nameof(ValuesController)}.{nameof(Test)}";
            var result = await _deaProcessor.ProcessAsync<String>(eventName, @byte, @short, @int, @long, @float, @double, @guid, @dateTime);

            return Ok(result);
        }
    }
}
