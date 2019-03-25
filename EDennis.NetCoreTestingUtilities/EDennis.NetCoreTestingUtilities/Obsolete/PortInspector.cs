using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;

namespace EDennis.NetCoreTestingUtilities {

    /// <summary>
    /// This class finds available ports.
    /// Adapted from https://gist.github.com/jrusbatch/4211535
    /// </summary>
    [Obsolete]
    public class PortInspector {

        /// <summary>
        /// checks for used ports and retrieves the first free port
        /// </summary>
        /// <returns>the free port or 0 if it did not find a free port</returns>
        public static List<int> GetAvailablePorts(int startingPort, int portCount) {

            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();
            List<int> availablePorts = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            //getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            //getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (int i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i)) {
                    availablePorts.Add(i);
                    if (availablePorts.Count == portCount)
                        return availablePorts;
                }

            throw new KeyNotFoundException($"Insufficient number of available ports.  Ports available: {availablePorts.Count}");
        }
    }
}
