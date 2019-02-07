using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

public static class OutputConsole
{
    public static TextWriter Writer { get; set; }
    public static bool IsWriterConsole { get; set; }

    private static readonly BlockingCollection<Tuple<string, ConsoleColor?, ConsoleColor?>> MQueue = new BlockingCollection<Tuple<string, ConsoleColor?, ConsoleColor?>>();

    static OutputConsole()
    {
        Writer = Console.Out;
        IsWriterConsole = true;

        try
        {
            var thread = new Thread(
                    () =>
                    {
                        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                        while (true)
                        {
                            var (message, foreColor, backColor) = MQueue.Take();
                            try
                            {
                                if (isWindows && IsWriterConsole)
                                {
                                    if (foreColor != null)
                                        Console.ForegroundColor = (ConsoleColor)foreColor;
                                    if (backColor != null)
                                        Console.BackgroundColor = (ConsoleColor)backColor;
                                }
                            }
                            catch
                            {
                                // ignore
                            }

                            Writer.WriteLine(message);
                            try
                            {
                                if (isWindows && IsWriterConsole)
                                    Console.ResetColor();
                            }
                            catch
                            {
                                // ignore
                            }
                        }
                    })
            { IsBackground = true };
            thread.Start();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public static void WriteLine(string value, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        MQueue.Add(new Tuple<string, ConsoleColor?, ConsoleColor?>(value, foregroundColor, backgroundColor));
    }

    public static Task WriteLineAsync(string value, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        return Task.Run(() => { MQueue.Add(new Tuple<string, ConsoleColor?, ConsoleColor?>(value, foregroundColor, backgroundColor)); });
    }
}
