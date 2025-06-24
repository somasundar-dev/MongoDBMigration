using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDBMigration.Services;

namespace MongoDBMigration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MigrationController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddIndex(
            [FromServices] MigrationService migrationService,
            [FromQuery] string indexName,
            [FromQuery] string fieldName
        )
        {
            if (string.IsNullOrWhiteSpace(indexName) || string.IsNullOrWhiteSpace(fieldName))
            {
                return BadRequest("Index name and field name must be provided.");
            }

            try
            {
                await migrationService.AddIndexAsync(indexName, fieldName);
                return Ok($"Index '{indexName}' on field '{fieldName}' created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Error creating index: {ex.Message}"
                );
            }
        }
    }
}
