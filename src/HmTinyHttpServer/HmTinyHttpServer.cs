/*
 * Copyright (c) 2025 Akitsugu Komiyama
 * under the MIT License
 */

using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;


internal class HmTinyHttpServer
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool IsWindow(nint hWnd);

    static nint hmWndHandle = 0;

    static HmTinyHttpServer server;

    static string phpExePath = Path.Combine(System.AppContext.BaseDirectory, "php.exe");

    const string phpHostName = "localhost";

    static string phpServerDocumentFolder = System.AppContext.BaseDirectory;
    // 秀丸の該当プロセスのウィンドウハンドルの値がもらいやすいので、これが存在しなくなっていたら、このプロセスも終了するようにする。
    static async Task Main(String[] args)
    {
        if (args.Length < 2)
        {
            return;
        }

        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

        try
        {
            phpServerDocumentFolder = args[0];
            hmWndHandle = (nint)long.Parse(args[1]);
        }
        catch (Exception) {
            return;
        }

        server = new HmTinyHttpServer();
        int port = server.Launch();
        Console.WriteLine("PORT:" + port);
        if (port == 0)
        {
            return;
        }

        while (true)
        {
            await Task.Delay(1000); // 1秒待つ
            if (!IsWindow(hmWndHandle))
            {
                break;
            }
            if (phpProcess == null)
            {
                break;
            }
            if (phpProcess.HasExited)
            {
                break;
            }
        }

        server?.Destroy();
        Console.WriteLine($"秀丸ウィンドウハンドル:{hmWndHandle}から呼ばれた{nameof(HmTinyHttpServer)}はクローズします。");
        // 何か外部からインプットがあれば終了し、このserverインスタンスが終われば、対応したphpサーバープロセスもkillされる。
    }

    private static void OnProcessExit(object sender, EventArgs e)
    {
        server?.Destroy();
        Console.WriteLine("OnProcessExit");
    }

    static Process phpProcess;

    // PHPデーモンのスタート
    HmTinyHttpServer()
    {
        try
        {
            Destroy();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString() + "\r\n");
        }
    }
    ~HmTinyHttpServer()
    {
        Destroy();
    }

    public int Launch()
    {
        return CreatePHPServerProcess();
    }


    private static int getFreePort()
    {
        try
        {
            var ipGP = IPGlobalProperties.GetIPGlobalProperties();
            var usedPorts = ipGP.GetActiveTcpListeners()
                .Concat(ipGP.GetActiveUdpListeners())
                .Select(endpoint => endpoint.Port)
                .Distinct();

            for (int port = 49152; port <= 65535; port++)
            {
                if (!usedPorts.Contains(port))
                {
                    return port; // 空いているポートを見つけた場合
                }
            }
        }
        catch (Exception)
        {
        }

        return 0; // 空きポートが見つからない場合
    }

    // PHPプロセス生成
    private int CreatePHPServerProcess()
    {
        try
        {
            int port = getFreePort();

            phpProcess = new Process();
            ProcessStartInfo psi = phpProcess.StartInfo;
            psi.FileName = Path.Combine(System.AppContext.BaseDirectory, phpExePath);
            psi.Arguments = $" -S {phpHostName}:{port} -t \"{phpServerDocumentFolder}\" ";

            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = false;
            psi.RedirectStandardError = false;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            phpProcess.Start();
            return port;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString() + "\r\n");
        }

        return 0;
    }


    private void Destroy()
    {
        try
        {
            phpProcess?.Kill();
        }
        catch (Exception)
        {
        }
    }
}
