/*
 * Copyright (c) 2025 Akitsugu Komiyama
 * under the MIT License
 */

using System.Net.NetworkInformation;

internal partial class HmTinyHttpServer
{
    private static int getFreePort()
    {
        try
        {
            var ipGP = IPGlobalProperties.GetIPGlobalProperties();
            if (ipGP == null)
            {
                // Console.WriteLine("IPGlobalPropertiesの取得に失敗しました。");
                return 0;
            }

            var usedPorts = new HashSet<int>(ipGP.GetActiveTcpListeners()
                                              .Concat(ipGP.GetActiveUdpListeners())
                                              .Select(endpoint => endpoint.Port));

            for (int port = 49152; port <= 65535; port++)
            {
                if (!usedPorts.Contains(port))
                {
                    // Console.WriteLine($"利用可能なポートが見つかりました: {port}");
                    return port;
                }
            }

            // Console.WriteLine("利用可能なポートが見つかりませんでした。");
            return 0;
        }
        catch (NetworkInformationException ex)
        {
            return 0;
        }
        catch (Exception ex)
        {
            // Console.WriteLine("なんか知らんがエラー");
            return 0;
        }
    }
}
