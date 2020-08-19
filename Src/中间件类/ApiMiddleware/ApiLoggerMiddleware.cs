#if NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kane.Extension;
using Kane.Extension.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Kane.AspNetCore
{
    /// <summary>
    /// 请求日志中间件
    /// </summary>
    public class ApiLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// 构造 Http 请求中间件
        /// </summary>
        /// <param name="next"></param>
        /// <param name="loggerFactory"></param>
        public ApiLoggerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _stopwatch = new Stopwatch();
            _logger = loggerFactory.CreateLogger<ApiLoggerMiddleware>();
        }


        /// <summary>
        /// 执行响应流指向新对象
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var api = new ApiLogger();
            var endpoints = context.GetEndpoint();
            if (endpoints.IsNotNull())
            {
                var attrs = endpoints.Metadata.GetOrderedMetadata<ApiLoggerAttribute>();
                if (attrs.Count > 0 && attrs.LastOrDefault().Ignore != true)//检测是否包含忽略
                {
                    api.Name = attrs.LastOrDefault().Name;
                    api.Remark = attrs.LastOrDefault().Remark;
                    context.Request.EnableBuffering();//开启后，可多次读取请求的Body
                    _stopwatch.Restart();
                    api.HttpMethod = context.Request.Method;
                    api.QueryString = context.Request.QueryString.Value;
                    api.RequestUrl = context.Request.Path;
                    api.RequestHost = context.Request.Host.Value;
                    api.LocalIP = $"{context.Connection.LocalIpAddress.MapToIPv4()}:{context.Connection.LocalPort}";
                    api.RemoteIP = $"{context.Connection.RemoteIpAddress.MapToIPv4()}:{context.Connection.RemotePort}";

                    var stachRequestBody = context.Request.Body;//暂存的Request流
                    var stachResponseBody = context.Response.Body;//暂存的Response流
                    try
                    {
                        using var newRequestBody = new MemoryStream();
                        using var newResponseBody = new MemoryStream();
                        context.Request.Body = newRequestBody;//替换Request流
                        context.Response.Body = newResponseBody;//替换Response流
                        using (var reader = new StreamReader(stachRequestBody))
                        {
                            api.RequestBody = await reader.ReadToEndAsync(); //读取原始请求流的内容
                        }
                        using (var writer = new StreamWriter(newRequestBody))//将原内容写回替换的Request流
                        {
                            await writer.WriteAsync(api.RequestBody);
                            writer.Flush();
                            newRequestBody.Position = 0;
                            await _next(context);
                        }
                        using (var reader = new StreamReader(newResponseBody))
                        {
                            newResponseBody.Position = 0;
                            api.ResponseBody = await reader.ReadToEndAsync();//读取替换Response流内的信息
                        }
                        await stachResponseBody.WriteAsync(api.ResponseBody.ToBytes());//将读取的信息回写到原暂存的Response流
                    }
                    finally
                    {
                        context.Request.Body = stachRequestBody;//替换原来的Request流
                        context.Response.Body = stachResponseBody;//替换原来的Response流
                    }
                    context.Response.OnCompleted(() => // 响应完成时存入缓存或日志输出
                    {
                        _stopwatch.Stop();
                        api.ElapsedTime = _stopwatch.ElapsedMilliseconds;
                        _logger.LogInformation($"ApiInfo:{api.ToJson()}");
                        return Task.CompletedTask;
                    });
                    return;
                }
            }
            await _next(context);
        }
    }
}
#endif