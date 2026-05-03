using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace lingui;

public static class Logger {
    private const string RED = "\x1b[31m";
    private const string GREEN = "\x1b[32m";
    private const string BLUE = "\x1b[34m";
    private const string YELLOW = "\x1b[93m";
    private const string RESET = "\x1b[0m";
    
    public static void AssertIsNotNull([NotNull] object? obj,string? message = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) {
        if (obj != null) return;
        Error($"ASSERT NOT NULL FAILED: '{message}' at {filePath}:{lineNumber}", fail:true);
        ArgumentNullException.ThrowIfNull(obj);
    }
    
    
    public static void Info(string message, params object?[] args) {
        Console.WriteLine("[{0}INF{1}] : '{2}'", GREEN, RESET, message);
    }

    public static void Warning(string message, params object?[] args) {
        Console.WriteLine("[{0}WRN{1}] : '{2}'", YELLOW, RESET, message);
    }


    public static void Error(string message, bool fail = false) {
        Console.WriteLine("[{0}ERR{1}] : '{2}'", RED, RESET, message);
        if (!fail) return;
        System.Environment.Exit(-1);
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        throw new Exception(message);
    }
}