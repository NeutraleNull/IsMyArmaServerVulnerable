namespace IsMyArmaServerVulnerableApi.Dto
{
    public class ServersVulnerableResponse
    {
        public int Servers { get; set; }
        public int ServersReachable { get; set; }
        public int ServersVulnerable { get; set; }
    }
}