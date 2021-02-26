namespace IsMyArmaServerVulnerableApi.Dto
{
    public class TestServerResponse
    {
        public bool IsVulnerable { get; set; }
        public bool IsReachable { get; set; }
        public bool RequestFailed { get; set; }
    }
}