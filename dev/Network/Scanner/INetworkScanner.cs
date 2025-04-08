namespace ServerNetworkAPI.dev.Network.Scanner
{
    public interface INetworkScanner
    {
        /// <summary>
        /// Führt den Scan aus und gibt die Liste gefundener IP-Adressen zurück.
        /// </summary>
        /// <param name="ipPrefix">Subnetz-Prefix, z. B. 192.168.1.</param>
        /// <returns>Menge aktiver IP-Adressen</returns>
        HashSet<string> Scan(string ipPrefix);
    }
}
