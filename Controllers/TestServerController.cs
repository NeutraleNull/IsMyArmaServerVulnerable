using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IsMyArmaServerVulnerableApi.Dto;
using IsMyArmaServerVulnerableApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IsMyArmaServerVulnerableApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestServerController : ControllerBase
    {
        private readonly ILogger<TestServerController> _logger;
        private readonly ArmaServerQueryService _armaServerQuery;

        public TestServerController(ILogger<TestServerController> logger, ArmaServerQueryService armaServerQuery)
        {
            _logger = logger;
            _armaServerQuery = armaServerQuery;
        }

        [HttpPost("testserver")]
        public async Task<IActionResult> TestServerAsync([FromBody] TestServerRequest request)  
        {
            try
            {
                var response = await _armaServerQuery.TestServerAsync(request.Hostname, request.Port, 3, 3);
                return Ok(new TestServerResponse
                {
                    IsReachable = response.IsReachable,
                    IsVulnerable = response.IsVulnerable, 
                    RequestFailed = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"IP {HttpContext.Connection.RemoteIpAddress} failed, error: {ex.Message}");
                return Ok(new TestServerResponse
                {
                    IsReachable = false,
                    IsVulnerable = false, 
                    RequestFailed = true
                });
            }
        }

        [HttpGet("serversvulnerable")]
        public IActionResult GetServersVulnerable()
        {
            return Ok(new ServersVulnerableResponse
            {
                Servers = BrokenServerList.TotalServer,
                ServersReachable = BrokenServerList.TotalServer - BrokenServerList.UnreachableServer,
                ServersVulnerable = BrokenServerList.VulnerableServer
            });
        }

    }
}
