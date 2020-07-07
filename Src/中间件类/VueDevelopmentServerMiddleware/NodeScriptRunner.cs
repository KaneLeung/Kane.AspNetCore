#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展方法
* 类 名 称 ：NodeScriptRunner
* 类 描 述 ：内部方法
* 所在的域 ：KK-HOME
* 命名空间 ：Kane.AspNetCore
* 机器名称 ：KK-HOME 
* CLR 版本 ：4.0.30319.42000
* 作　　者 ：Kane Leung
* 创建时间 ：2020/07/05 11:48:34
* 更新时间 ：2020/07/05 11:48:34
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Kane Leung 2020. All rights reserved.
*******************************************************************
-----------------------------------------------------------------*/
#endregion
#if NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace Kane.AspNetCore
{
    /// <summary>
    /// <para>https://github.com/dotnet/aspnetcore/blob/3.0/src/Middleware/SpaServices.Extensions/src/Npm/NodeScriptRunner.cs</para>
    /// </summary>
    internal class NodeScriptRunner
    {
        public EventedStreamReader StdOut { get; }
        public EventedStreamReader StdErr { get; }
        private static Regex AnsiColorRegex = new Regex("\x001b\\[[0-9;]*m", RegexOptions.None, TimeSpan.FromSeconds(1));

        public NodeScriptRunner(string workingDirectory, string scriptName, string arguments, IDictionary<string, string> envVars, string pkgManagerCommand)
        {
            if (string.IsNullOrEmpty(workingDirectory)) throw new ArgumentNullException(nameof(workingDirectory), $"参数不能为空。");
            if (string.IsNullOrEmpty(scriptName)) throw new ArgumentNullException(nameof(scriptName), $"参数不能为空。");
            if (string.IsNullOrEmpty(pkgManagerCommand)) throw new ArgumentNullException(nameof(pkgManagerCommand), $"参数不能为空。");
            var exeToRun = pkgManagerCommand;
            var completeArguments = $"run {scriptName} -- {arguments ?? string.Empty}";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, the node executable is a .cmd file, so it can't be executed
                // directly (except with UseShellExecute=true, but that's no good, because
                // it prevents capturing stdio). So we need to invoke it via "cmd /c".
                exeToRun = "cmd";
                completeArguments = $"/c {pkgManagerCommand} {completeArguments}";
            }
            var processStartInfo = new ProcessStartInfo(exeToRun)
            {
                Arguments = completeArguments,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory
            };
            if (envVars != null)
            {
                foreach (var keyValuePair in envVars)
                {
                    processStartInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
                }
            }
            var process = LaunchNodeProcess(processStartInfo, pkgManagerCommand);
            StdOut = new EventedStreamReader(process.StandardOutput);
            StdErr = new EventedStreamReader(process.StandardError);
        }

        public void AttachToLogger(ILogger logger)
        {
            // When the node task emits complete lines, pass them through to the real logger
            StdOut.OnReceivedLine += line =>
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    // Node tasks commonly emit ANSI colors, but it wouldn't make sense to forward
                    // those to loggers (because a logger isn't necessarily any kind of terminal)
                    logger.LogInformation(StripAnsiColors(line));
                }
            };
            StdErr.OnReceivedLine += line =>
            {
                if (!string.IsNullOrWhiteSpace(line)) logger.LogError(StripAnsiColors(line));
            };
            // But when it emits incomplete lines, assume this is progress information and
            // hence just pass it through to StdOut regardless of logger config.
            StdErr.OnReceivedChunk += chunk =>
            {
                var containsNewline = Array.IndexOf(chunk.Array, '\n', chunk.Offset, chunk.Count) >= 0;
                if (!containsNewline) Console.Write(chunk.Array, chunk.Offset, chunk.Count);
            };
        }

        private static string StripAnsiColors(string line) => AnsiColorRegex.Replace(line, string.Empty);

        private static Process LaunchNodeProcess(ProcessStartInfo startInfo, string commandName)
        {
            try
            {
                var process = Process.Start(startInfo);
                // See equivalent comment in OutOfProcessNodeInstance.cs for why
                process.EnableRaisingEvents = true;
                return process;
            }
            catch (Exception ex)
            {
                var message = $"Failed to start '{commandName}'. To resolve this:.\n\n"
                            + $"[1] Ensure that '{commandName}' is installed and can be found in one of the PATH directories.\n"
                            + $"    Current PATH enviroment variable is: { Environment.GetEnvironmentVariable("PATH") }\n"
                            + "    Make sure the executable is in one of those directories, or update your PATH.\n\n"
                            + "[2] See the InnerException for further details of the cause.";
                throw new InvalidOperationException(message, ex);
            }
        }
    }
}
#endif