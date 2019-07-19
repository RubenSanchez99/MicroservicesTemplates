using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRUDService.Model;
using Microsoft.AspNetCore.Mvc;
using CRUDService.Services.Interfaces;

namespace CRUDService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        private readonly IDefaultService defaultService;

        public DefaultController(IDefaultService defaultService)
        {
            this.defaultService = defaultService;
        }

        // GET api/v1/[controller]
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var result = await defaultService.GetEntities();
            return Ok(result);
        }

        // GET api/v1/[controller]/id
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var result = await defaultService.GetEntity(id);
            return Ok(result);
        }

        // POST api/v1/[controller]
        [Route("")]
        [HttpPost]
        public async Task<IActionResult> AddProduct(Entity entity)
        {
            var result = await defaultService.AddEntity(entity);
            return CreatedAtAction(nameof(GetProduct), new { id = result }, null);
        }

        // PUT api/v1/[controller]
        [Route("")]
        [HttpPut]
        public async Task<IActionResult> UpdateProduct(Entity entity)
        {
            var result = await defaultService.UpdateEntity(entity);
            return CreatedAtAction(nameof(GetProduct), new { id = result }, null);
        }

        // DELETE api/v1/[controller]/id
        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await defaultService.DeleteEntity(id);
            return Ok();
        }
    }
}
