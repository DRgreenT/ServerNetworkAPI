namespace ServerNetworkAPI.dev.Network.Scanner
{
    public interface INetworkScanner
    {
        HashSet<string> Scan(string ipPrefix);
    }
}
