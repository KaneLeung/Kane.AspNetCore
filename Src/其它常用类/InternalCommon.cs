#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展方法
* 类 名 称 ：InternalCommon
* 类 描 述 ：内部公有方法
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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using Kane.Extension;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kane.AspNetCore
{
    internal static class InternalCommon
    {
        #region 根据日志类别，获取或创建ILogger + GetOrCreateLogger(this IApplicationBuilder appBuilder, string categoryName)
        /// <summary>
        /// 根据日志类别，获取或创建ILogger
        /// </summary>
        /// <param name="appBuilder">请求管道构建器</param>
        /// <param name="categoryName">日志类别</param>
        /// <returns></returns>
        public static ILogger GetOrCreateLogger(this IApplicationBuilder appBuilder, string categoryName)
        {
            var loggerFactory = appBuilder.ApplicationServices.GetService<ILoggerFactory>();
            return loggerFactory != null ? loggerFactory.CreateLogger(categoryName) : NullLogger.Instance;
        }
        #endregion

        #region 判断端口是否占用，若占用，则返回一个空闲的端口，否则返回原来的端口 + CheckAndGetAvailablePort(int port)
        /// <summary>
        /// 判断端口是否占用，若占用，则返回一个空闲的端口，否则返回原来的端口
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="foreKill">是否强制终止</param>
        /// <returns></returns>
        public static int CheckAndGetAvailablePort(int port, bool foreKill)
        {
            var ipEndPoints = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            if (ipEndPoints.Any(k => k.Port == port)) //已经占有，则获取一个空闲的端口
            {
                if (foreKill)
                {
                    var helper = new ProcessHelper();
                    if (helper.KillProcess(helper.GetPortPid(port), true) == true) return port;
                }
                var listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                try
                {
                    return ((IPEndPoint)listener.LocalEndpoint).Port;
                }
                finally
                {
                    listener.Stop();
                }
            }
            else return port;
        }
        #endregion
    }
}