using System.Globalization;

namespace LinodeDDNS;

public static class Log
{
    public static void Information(string message) => Console.WriteLine($"[INF] [{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] " + message);

    public static void Error(string message) => Console.WriteLine($"[ERR] [{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] " + message);
    
    public static void Exception(Exception exception)
    {
        var str = exception.Message + "\n" + exception.StackTrace;
        var innerException = exception.InnerException;
        while (innerException != null)
        {
            str += "\n" + innerException.Message + "\n" + innerException.StackTrace;
            innerException = innerException.InnerException;
        }
        Console.WriteLine($"[ERR] [{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] " + str);
    }
}
