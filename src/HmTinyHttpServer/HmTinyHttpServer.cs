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

    static void InputTask()
    {
        while(true)
        {
            // ReadLineがあっても終了
            var line = Console.ReadLine();
            if (line.Contains("exit"))
            {
                server?.Destroy();
                break;
            }
        }
    }

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
        Console.WriteLine(port); // ポート番号の出力
        if (port == 0)
        {
            return;
        }

        // 標準入力監視タスクを開始
        Task inputTask = Task.Run(InputTask);

        while (true)
        {
            await Task.Delay(1000); // 1秒待つ
            if (!IsWindow(hmWndHandle))
            {
                // Console.WriteLine("hmWndHandleが存在しなくなったので終了します。");  
                break;
            }
            if (phpProcess == null)
            {
                // Console.WriteLine("phpProcessが存在しないので終了します。");
                break;
            }
            if (phpProcess.HasExited)
            {
                // Console.WriteLine("phpProcessが終了したので終了します。");
                break;
            }
            if (inputTask.IsCompleted) // 何か標準入力があっても終了
            {
                // Console.WriteLine("標準入力監視タスクが完了したので終了します。");
                break;
            }
            if (Environment.HasShutdownStarted)
            {
                // Console.WriteLine("Environment.HasShutdownStartedがtrueになったので終了します。");
                break;
            }
        }

        server?.Destroy();
        // Console.WriteLine($"秀丸ウィンドウハンドル:{hmWndHandle}から呼ばれた{nameof(HmTinyHttpServer)}はクローズします。");
        // 何か外部からインプットがあれば終了し、このserverインスタンスが終われば、対応したphpサーバープロセスもkillされる。
    }

    private static void OnProcessExit(object sender, EventArgs e)
    {
        server?.Destroy();
        // Console.WriteLine("OnProcessExit");
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
            // Console.WriteLine(psi.FileName);
            // Console.WriteLine(psi.Arguments);
            psi.WorkingDirectory = (System.AppContext.BaseDirectory);

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
