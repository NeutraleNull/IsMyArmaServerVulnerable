using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IsMyArmaServerVulnerableApi.Services
{
    public class SteamServerQueryService
    {
        private readonly ILogger<SteamServerQueryService> _logger;
        private readonly string apiKey;
        public SteamServerQueryService(IConfiguration configuration, ILogger<SteamServerQueryService> logger)
        {
            _logger = logger;
            apiKey = configuration["SteamApiKey"];
        }


        public async Task<List<IPEndPoint>> GetArmaServerListApiAsync()
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync(
                "https://api.steampowered.com/IGameServersService/GetServerList/v1/?key="+apiKey+"&filter=appid\\107410&limit=20000");

            var responseDeserialized = JsonSerializer.Deserialize<Root>(response);
            var ipEndpoints = new List<IPEndPoint>();


            foreach (var server in responseDeserialized.Response.Servers)
            {
                string[] address = server.Addr.Split(':');


                ipEndpoints.Add(new IPEndPoint(IPAddress.Parse(address[0]), int.Parse(address[1])));
            }

            return ipEndpoints;
        }

        public class Server
        {
            [JsonPropertyName("addr")]
            public string Addr { get; set; }

            [JsonPropertyName("gameport")]
            public int Gameport { get; set; }

            [JsonPropertyName("steamid")]
            public string Steamid { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("appid")]
            public int Appid { get; set; }

            [JsonPropertyName("gamedir")]
            public string Gamedir { get; set; }

            [JsonPropertyName("version")]
            public string Version { get; set; }

            [JsonPropertyName("product")]
            public string Product { get; set; }

            [JsonPropertyName("region")]
            public int Region { get; set; }

            [JsonPropertyName("players")]
            public int Players { get; set; }

            [JsonPropertyName("max_players")]
            public int MaxPlayers { get; set; }

            [JsonPropertyName("bots")]
            public int Bots { get; set; }

            [JsonPropertyName("map")]
            public string Map { get; set; }

            [JsonPropertyName("secure")]
            public bool Secure { get; set; }

            [JsonPropertyName("dedicated")]
            public bool Dedicated { get; set; }

            [JsonPropertyName("os")]
            public string Os { get; set; }

            [JsonPropertyName("gametype")]
            public string Gametype { get; set; }
        }

        public class Response
        {
            [JsonPropertyName("servers")]
            public List<Server> Servers { get; set; }
        }

        public class Root
        {
            [JsonPropertyName("response")]
            public Response Response { get; set; }
        }

        //Don't use this!!! Kinda broken
        public async Task<List<IPEndPoint>> GetArmaServerListAsync(int maxServers)
        {
            var serverList = new List<IPEndPoint>();
            var steamMasterServerEndPoint =
                new IPEndPoint((await Dns.GetHostAddressesAsync("hl2master.steampowered.com"))[0], 27011);
            var packet = 0;
            IPEndPoint lastEndPoint = null;


            using var client = new UdpClient(0);
            client.Client.ReceiveTimeout = 4 * 1000;
            client.Connect(steamMasterServerEndPoint);

            while (packet < maxServers)
            {
                var request = BuildHeader(lastEndPoint ?? new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0));
                var tries = 0;
                var response = Array.Empty<byte>();

                while (tries < 7)
                {
                    try
                    {
                        await client.SendAsync(request, request.Length);
                        response = client.Receive(ref steamMasterServerEndPoint);
                        break;
                    }
                    catch
                    {
                        tries++;
                        if (tries == 7)
                            return serverList;
                        //Console.WriteLine($"Receiving data timeout, tries: {tries}/7");
                    }
                }

                var packetEndpoints = ReadIpEndpoints(response);
                serverList.AddRange(packetEndpoints);

                packet += packetEndpoints.Length;

                lastEndPoint = serverList.LastOrDefault();

                if (serverList.Last().Address.ToString() == "0.0.0.0")
                    break;
            }
            
            return serverList;
        }

        private static byte[] BuildHeader(IPEndPoint ipEndPoint)
        {
            var request = new List<byte> { 0x31, 0xFF };

            request.AddRange(Encoding.ASCII.GetBytes($"{ipEndPoint.Address}:{ipEndPoint.Port}"));
            request.AddRange(Encoding.ASCII.GetBytes("\\appid\\107410"));

            return request.ToArray();
        }

        private static IPEndPoint[] ReadIpEndpoints(byte[] data)
        {
            if (data[0] != 0xFF || data[1] != 0xFF || data[2] != 0xFF || data[3] != 0xFF || data[4] != 0x66 || data[5] != 0x0A)
                throw new Exception("Malformed header");

            var tmp = new List<IPEndPoint>();



            for (int i = 0; i < data.Length - 12; i += 6)
            {
                var port = BitConverter.ToUInt16(data[(10 + i)..(12 + i)].Reverse().ToArray(), 0);
                var ipAddress = new IPAddress(data[(6 + i)..(10 + i)]);
                tmp.Add(new IPEndPoint(ipAddress, port));
                //Console.WriteLine($"{ipAddress}:{port}");
            }

            return tmp.ToArray();
        }

    }
}
