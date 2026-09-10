using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InovaGAB.API.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly IMongoDatabase _mongoDatabase;

        public HealthController(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        [AllowAnonymous]
        [HttpGet("mongo")]
        public async Task<IActionResult> CheckMongoAsync(
            CancellationToken cancellationToken)
        {
            var command = new BsonDocument("ping", 1);

            var result =
                await _mongoDatabase.RunCommandAsync<BsonDocument>(
                    command,
                    cancellationToken: cancellationToken);

            return Ok(new
            {
                status = "healthy",
                database =
                    _mongoDatabase.DatabaseNamespace.DatabaseName,
                mongoPing = result["ok"].ToDouble()
            });
        }
    }
}
