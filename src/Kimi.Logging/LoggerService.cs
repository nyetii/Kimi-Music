using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Kimi.Logging
{
    public class LoggerService
    {
        public static ILogger LoggerConfiguration(string? path)
        {
            path ??= Environment.CurrentDirectory;
            return Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(@$"{path}\logs\kimi.log", rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd - HH:mm:ss.fff}|{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .Filter.ByExcluding(log => log.Exception is GatewayReconnectException)
                .Filter.ByExcluding(log => log.Exception is WebSocketException)
                .Filter.ByExcluding(log => log.Exception is SocketException)
                .CreateLogger();
        }
    }
}
