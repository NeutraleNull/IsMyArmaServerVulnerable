using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsMyArmaServerVulnerableApi.Services
{
    public static class BrokenServerList
    {
        public static int VulnerableServer = 0;
        public static int UnreachableServer = 0;
        public static int TotalServer = 0;
        public static float PercentageVulnerable => (float) VulnerableServer / (float) TotalServer;
        public static string PercentageVulnerableString => PercentageVulnerable.ToString("P");

        public static float PercentageVulnerableWithoutTrash =>
            (float) VulnerableServer / (float) (TotalServer - UnreachableServer);

        public static string PercentageVulnerableWithoutTrashString => PercentageVulnerableWithoutTrash.ToString("P");

    }
}