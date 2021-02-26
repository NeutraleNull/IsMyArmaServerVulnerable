using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IsMyArmaServerVulnerableApi.Services
{
    public class ArmaServerQueryService
    {
        public ArmaServerQueryService()
        {

        }

        //C# implementation of https://gist.github.com/DerZade/661a85478d2df937659144562d030dee
        public async Task<TestResponse> TestServerAsync(string hostname, int port, int timeout = 10, int maxTries = 5)
        {
            TestResponse testResponse = new();
            var ipAddresses = await Dns.GetHostAddressesAsync(hostname);
            var armaServerEndpoint = new IPEndPoint(ipAddresses[0], port);

            var client = new UdpClient(0) {Client = {ReceiveTimeout = timeout * 1000}};

            //Console.WriteLine("Sending A2S_INFO message to the server");
            
            //build package
            var encodedA2SInfo = new List<byte>();
            encodedA2SInfo.AddRange(new byte[] {0xff, 0xff, 0xff, 0xff});
            encodedA2SInfo.AddRange(Encoding.ASCII.GetBytes("TSource Engine Query"));
            encodedA2SInfo.AddRange(new byte[] {0x00});

            var response = Array.Empty<byte>();
            var tries = 0;

            while (tries < maxTries)
            {
                try
                {
                    await client.SendAsync(encodedA2SInfo.ToArray(), encodedA2SInfo.Count, armaServerEndpoint);
                    response = client.Receive(ref armaServerEndpoint);
                    break;
                }
                catch
                {
                    tries++;
                    if (tries == maxTries)
                    {
                        return new TestResponse {IsReachable = false, IsVulnerable = false};
                    }
                }
            }

            var unpacked = UnpackResponse(response);

            testResponse.Header = unpacked.Item1;
            testResponse.Data = unpacked.Item2;

            switch (testResponse.Header)
            {
                case 0x49:
                    //Console.WriteLine("Received an A2S_INFO response.The server did not respond with a challenge.This means you may be vulnerable to reflection attacks.`n(Make sure you're not testing this from localhost, because the steam query will never challenge requests from localhost.)");
                    testResponse.IsVulnerable = true;
                    testResponse.IsReachable = true;
                    client.Close();
                    return testResponse;
                case 0x41:
                    testResponse.IsVulnerable = false;
                    testResponse.IsReachable = true;
                    break;
                default:
                    client.Close();
                    throw new Exception("Unknown response");
            }

            //add challenge data
            encodedA2SInfo.AddRange(testResponse.Data);
            await client.SendAsync(encodedA2SInfo.ToArray(), encodedA2SInfo.Count, armaServerEndpoint);

            response = client.Receive(ref armaServerEndpoint);

            unpacked = UnpackResponse(response);

            if (unpacked.Item1 != 0x49)
                throw new Exception("Seems like the server did not respond with an A2S_INFO response.");

            testResponse.Data = unpacked.Item2;

            client.Close();

            return testResponse;
        }

        private static (byte, byte[]) UnpackResponse(byte[] response)
        {
            if (response.Length < 5)
                throw new Exception("Looks like this is not a valid response from the steam query. It is too short.");

            var headerBytes = response[..4];
            if (headerBytes[0] != 0xff || headerBytes[1] != 0xff || headerBytes[2] != 0xff || headerBytes[3] != 0xff)
                throw new Exception(
                    "Looks like this is not a valid response from the steam query. It does not have a valid header.");

            return (response[4], response[5..response.Length]);
        }

        public class TestResponse
        {
            public bool IsVulnerable;
            public bool IsReachable;
            public byte Header;
            public byte[] Data;
        }
    }
}
