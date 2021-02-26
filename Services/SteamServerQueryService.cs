using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IsMyArmaServerVulnerableApi.Services
{
    public class SteamServerQueryService
    {
        public SteamServerQueryService()
        {

        }

        
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


                if (Equals(serverList.Last().Address, IPAddress.Parse("0.0.0.0")))
                    break;
            }


            lastEndPoint = serverList.LastOrDefault();

            return serverList;
        }

        private static byte[] BuildHeader(IPEndPoint ipEndPoint)
        {
            var request = new List<byte> { 0x31, 0xFF };

            request.AddRange(Encoding.ASCII.GetBytes($"{ipEndPoint.Address}:{ipEndPoint.Port}"));
            request.AddRange(Encoding.ASCII.GetBytes("\\appid\\233780"));

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
