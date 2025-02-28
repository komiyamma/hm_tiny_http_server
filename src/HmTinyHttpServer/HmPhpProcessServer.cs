using System.Diagnostics;

internal partial class HmTinyHttpServer
{
    public class HmPhpProcessServer
    {

        Process phpProcess;

        // PHPデーモンのスタート
        public HmPhpProcessServer()
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
        ~HmPhpProcessServer()
        {
            Destroy();
        }

        public int Launch()
        {
            return CreatePHPServerProcess();
        }



        // PHPプロセス生成
        private int CreatePHPServerProcess()
        {
            try
            {
                int port = getFreePort();

                phpProcess = new Process();
                ProcessStartInfo psi = phpProcess.StartInfo;
                psi.FileName = phpExeFullPath;
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

        public Process GetProcess()
        {
            return phpProcess;
        }
        public void Destroy()
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
}
