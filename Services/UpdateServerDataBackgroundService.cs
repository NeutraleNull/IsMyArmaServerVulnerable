using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IsMyArmaServerVulnerableApi.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IsMyArmaServerVulnerableApi.Services
{
    public class UpdateServerDataBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly SteamServerQueryService _steamServerQueryService;

        public UpdateServerDataBackgroundService(ILogger<UpdateServerDataBackgroundService> logger, SteamServerQueryService steamServerQueryService)
        {
            _logger = logger;
            _steamServerQueryService = steamServerQueryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Updating server list");
                    var serverList = await _steamServerQueryService.GetArmaServerListApiAsync();

                    _logger.LogInformation($"Server list updated! Found: {serverList.Count} server");
                    var resultList = new ThreadSafeList<ArmaServerQueryService.TestResponse>();

                    _logger.LogInformation("Starting query run");
                    ThreadPool.SetMaxThreads(100, 200);
                    Parallel.ForEach(serverList,
                        async point =>
                        {
                            var armaServerQuery = new ArmaServerQueryService();
                            try
                            {
                                var result = await
                                    armaServerQuery.TestServerAsync(point.Address.ToString(), point.Port, 2, 2);
                                await resultList.AddAsync(result, stoppingToken);
                            }
                            catch
                            {
                                //this server is broken in a different way. Malformed header or honeypot. Will ignore
                            }
                        });

                    var resultSet = await resultList.GetList(stoppingToken);

                    BrokenServerList.TotalServer = resultSet.Count;
                    BrokenServerList.UnreachableServer = resultSet.Count(x => x.IsReachable == false);
                    BrokenServerList.VulnerableServer = resultSet.Count(x => x.IsVulnerable && x.IsReachable);

                    _logger.LogInformation("Update completed!");
                    _logger.LogInformation(
                        $"Results are:\nTotal Servers: {BrokenServerList.TotalServer}\nServers wrong configured: " +
                        $"{BrokenServerList.UnreachableServer}\nServers vulnerable: {BrokenServerList.VulnerableServer}, that is {BrokenServerList.PercentageVulnerableWithoutTrashString}");

                }
                catch
                {
                    _logger.LogError("Failed to update steam server list");
                }

                await Task.Delay(1000 * 60 * 15, stoppingToken);
            }
        }
    }
}
