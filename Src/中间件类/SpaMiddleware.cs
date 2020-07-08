#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展类库
* 类 名 称 ：SpaMiddleware
* 类 描 述 ：单页Web应用中间件
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
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Kane.AspNetCore
{
    /// <summary>
    /// 单页Web应用中间件
    /// </summary>
    public class SpaMiddleware : IMiddleware
    {
        private readonly SpaOptions options;
#if NETCOREAPP2_1
        private readonly IHostingEnvironment env;
        /// <summary>
        /// AspNetCore2.1单页Web应用中间件构造方法
        /// </summary>
        /// <param name="env">程序环境</param>
        /// <param name="options">配置选项</param>
        public SpaMiddleware(IHostingEnvironment env, IOptions<SpaOptions> options)
#else
        private readonly IWebHostEnvironment env;
        /// <summary>
        /// 单页Web应用中间件构造方法
        /// </summary>
        /// <param name="env">程序环境</param>
        /// <param name="options">配置选项</param>
        public SpaMiddleware(IWebHostEnvironment env, IOptions<SpaOptions> options)
#endif
        {
            this.env = env;
            this.options = options?.Value ?? new SpaOptions();
        }

        #region 中间件方法 + InvokeAsync(HttpContext context, RequestDelegate next)
        /// <summary>
        /// 中间件方法
        /// </summary>
        /// <param name="context">当前请求上下文</param>
        /// <param name="next">请求委托</param>
        /// <returns></returns>
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (options.Enable)
            {
                var path = context.Request.Path.Value.ToLower();
                if (options.IgnorePaths.Any(k => k.StartsWith(path, StringComparison.OrdinalIgnoreCase))) next(context);
                if (options.MapPaths.Any(k => k.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                {
                    var file = Path.Combine(env.WebRootPath, options.StaticFiles);
                    if (File.Exists(file)) return context.Response.SendFileAsync(file);
                }
            }
            return next(context);
        }
        #endregion
    }
}