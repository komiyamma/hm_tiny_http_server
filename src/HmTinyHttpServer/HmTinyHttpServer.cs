/*
 * Copyright (c) 2025 Akitsugu Komiyama
 * under the MIT License
 */

using System.Runtime.InteropServices;

internal partial class HmTinyHttpServer
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool IsWindow(nint hWnd);

    static nint hmWndHandle = 0;

    static HmPhpProcessServer server;

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

        try
        {
            phpServerDocumentFolder = args[0];
            hmWndHandle = (nint)long.Parse(args[1]);
        }
        catch (Exception) {
            return;
        }

        server = new HmPhpProcessServer();
        int port = server.Launch();
        Console.WriteLine(port); // ポート番号の出力
        if (port == 0)
        {
            return;
        }

        // 秀丸側のrunProcessのstdioAliveのprocessの終わらせ方が判然としない
        // ProcessExitにも反応する
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

        // 秀丸側のrunProcessのstdioAliveのprocessの終わらせ方が判然としない
        // Win32レベルで、シャットダウンやログアウトも含めこのコンソールへの終了のあらゆる促しに感知するようにしておく
        SetConsoleCtrlHandler(ConsoleCtrlCheck, true);

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

            var phpProcess = server.GetProcess();
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


    // DLLインポート
    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

    // デリゲート
    private delegate bool HandlerRoutine(CtrlTypes CtrlType);

    // コンソール制御イベントの種類
    private enum CtrlTypes
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2, // ログオフ、シャットダウン
        CTRL_LOGOFF_EVENT = 5, // ログオフ
        CTRL_SHUTDOWN_EVENT = 6 // シャットダウン
    }

    // コールバック関数
    private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
    {
        switch (ctrlType)
        {
            case CtrlTypes.CTRL_C_EVENT:
            case CtrlTypes.CTRL_BREAK_EVENT:
            case CtrlTypes.CTRL_CLOSE_EVENT:
            case CtrlTypes.CTRL_LOGOFF_EVENT:
            case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                server?.Destroy();
                // Console.WriteLine("ログアウトまたはシャットダウンを検出しました。アプリケーションを終了します。");
                // 終了処理をここに記述
                Environment.Exit(0); // アプリケーションを終了
                return true; // 他のイベントは処理しない
            default:
                return false; // 他のイベントは処理しない
        }
    }

}
