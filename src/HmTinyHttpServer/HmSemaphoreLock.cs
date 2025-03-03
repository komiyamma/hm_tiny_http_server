using System.Runtime.InteropServices;

internal partial class HmTinyHttpServer
{
    private const string SemaphoreName = @"Global\HmTinyHttpServerSemaphore";
    private static Semaphore semaphore;

    private static void ClearSemaphore()
    {
        semaphore?.Release();
        semaphore?.Close();
        semaphore?.Dispose();
    }
}
