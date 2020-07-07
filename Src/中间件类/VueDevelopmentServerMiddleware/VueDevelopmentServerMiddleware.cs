#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展方法
* 类 名 称 ：VueDevelopmentServerMiddleware
* 类 描 述 ：Vue开发环境中间件
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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Kane.Extension;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.Logging;

namespace Kane.AspNetCore
{
    /// <summary>
    /// Vue开发环境中间件，参考自<see cref="Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer"/>
    /// <para>https://github.com/dotnet/aspnetcore/blob/3.0/src/Middleware/SpaServices.Extensions/src/ReactDevelopmentServer/ReactDevelopmentServerMiddleware.cs</para>
    /// </summary>
    public static class VueDevelopmentServerMiddleware
    {
        #region VueDevelopmentServerMiddleware中间器扩展方法 + UseVueDevelopmentServer(this ISpaBuilder spaBuilder, string script, int port = 8080, string pkgManagerCommand = "npm")
        /// <summary>
        /// VueDevelopmentServerMiddleware中间器扩展方法
        /// </summary>
        /// <param name="spaBuilder">当前Spa创建器</param>
        /// <param name="script">运行的脚本命令</param>
        /// <param name="port">默认端口【8080】，如果端口被占用，则自动获取一个新的端口</param>
        /// <param name="pkgManagerCommand">使用包管理器命令，默认为【npm】</param>
        public static void UseVueDevelopmentServer(this ISpaBuilder spaBuilder, string script, int port = 8080, string pkgManagerCommand = "npm")
        {
            if (spaBuilder == null) throw new ArgumentNullException("spaBuilder", "参数不能为空。");
            if (string.IsNullOrEmpty(spaBuilder.Options.SourcePath))
                throw new InvalidOperationException($"调用{nameof(UseVueDevelopmentServer)},必须为【UseSpa】提供SpaOptions的SourcePath值");
            Attach(spaBuilder, script, port, pkgManagerCommand);
        }
        #endregion

        #region 将Vue Server附加到当前程序 + Attach(ISpaBuilder spaBuilder, string script, int port, string pkgManagerCommand)
        /// <summary>
        /// 将Vue Server附加到当前程序
        /// </summary>
        /// <param name="spaBuilder">当前Spa创建器</param>
        /// <param name="script">运行的脚本命令</param>
        /// <param name="port">端口，如果端口被占用，则自动获取一个新的端口</param>
        /// <param name="pkgManagerCommand">使用包管理器命令</param>
        private static void Attach(ISpaBuilder spaBuilder, string script, int port, string pkgManagerCommand)
        {
            string sourcePath = spaBuilder.Options.SourcePath;
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException(nameof(sourcePath), $"参数不能为空。");
            if (string.IsNullOrEmpty(script)) throw new ArgumentNullException(nameof(script), $"参数不能为空。");


            var appBuilder = spaBuilder.ApplicationBuilder;
            var logger = InternalCommon.GetOrCreateLogger(appBuilder, nameof(VueDevelopmentServerMiddleware));
            var portTask = CreateVueAppServerAsync(sourcePath, script, port, pkgManagerCommand, logger);//创建Vue服务

            // Everything we proxy is hardcoded to target http://localhost because:
            // - the requests are always from the local machine (we're not accepting remote
            //   requests that go directly to the create-react-app server)
            // - given that, there's no reason to use https, and we couldn't even if we
            //   wanted to, because in general the create-react-app server has no certificate
            var targetUriTask = portTask.ContinueWith(task => new UriBuilder("http", "localhost", task.Result).Uri);
            SpaProxyingExtensions.UseProxyToSpaDevelopmentServer(spaBuilder, () =>
            {
                // On each request, we create a separate startup task with its own timeout. That way, even if
                // the first request times out, subsequent requests could still work.
                var timeout = spaBuilder.Options.StartupTimeout;
                return targetUriTask.TimeoutCancel(timeout, $"程序在【{timeout}】秒内无法侦听到Vue server，请检查日志输出中的错误信息，或加长侦听时长。");
            });
        }
        #endregion

        #region 创建Vue Server，返回成功的服务器端口 + CreateVueAppServerAsync(string sourcePath, string script, int port, string pkgManagerCommand, ILogger logger)
        /// <summary>
        /// 创建Vue Server，返回成功的服务器端口
        /// </summary>
        /// <param name="sourcePath">源代码路径</param>
        /// <param name="script">运行的脚本命令</param>
        /// <param name="port">端口</param>
        /// <param name="pkgManagerCommand">使用包管理器命令</param>
        /// <param name="logger">日志入口</param>
        /// <returns></returns>
        private static async Task<int> CreateVueAppServerAsync(string sourcePath, string script, int port, string pkgManagerCommand, ILogger logger)
        {
            int portNumber = InternalCommon.CheckAndGetAvailablePort(port);
            if (portNumber != port) logger.LogInformation($"原端口【{port}】已被占有，重新获取新端口【{portNumber}】。");
            logger.LogInformation($"在端口【{portNumber}】上运行Vue server。");
            var envVars = new Dictionary<string, string>
            {
                { "PORT", port.ToString() },
                { "DEV_SERVER_PORT", port.ToString() },
                { "BROWSER", "none" }
            };
            var runner = new NodeScriptRunner(sourcePath, script, $"--port {port:0}", envVars, pkgManagerCommand);
            runner.AttachToLogger(logger);
            using (var reader = new EventedStreamStringReader(runner.StdErr))
            {
                try
                {
                    // Although the React dev server may eventually tell us the URL it's listening on,
                    // it doesn't do so until it's finished compiling, and even then only if there were
                    // no compiler warnings. So instead of waiting for that, consider it ready as soon
                    // as it starts listening for requests.
                    await runner.StdOut.WaitForMatch(new Regex("running at", RegexOptions.None, TimeSpan.FromSeconds(5.0)));
                }
                catch (EndOfStreamException ex)
                {
                    throw new InvalidOperationException($"运行的脚本【{script}】已退出，程序没有获取到Vue Server是否成功，请检查输出中的错误信息：{reader.ReadAsString()}", ex);
                }
            }
            return portNumber;
        }
        #endregion
    }
}
#endif