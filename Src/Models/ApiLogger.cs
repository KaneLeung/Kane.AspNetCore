using System;

namespace Kane.AspNetCore
{
    /// <summary>
    /// Api日志模型
    /// </summary>
    public class ApiLogger
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 请求来源Host
        /// </summary>
        public string RequestHost { get; set; }
        /// <summary>
        /// 请求路径
        /// </summary>
        public string RequestUrl { get; set; }
        /// <summary>
        /// 请求方法类型：GET/POST
        /// </summary>
        public string HttpMethod { get; set; }
        /// <summary>
        /// 请求参数字符串
        /// </summary>
        public string QueryString { get; set; }
        /// <summary>
        /// 请求报文，POST专用
        /// </summary>
        public string RequestBody { get; set; }
        /// <summary>
        /// 请问时间
        /// </summary>
        public DateTime RequestTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 响应报文
        /// </summary>
        public string ResponseBody { get; set; }
        /// <summary>
        /// 耗时，单位【毫秒】
        /// </summary>
        public long ElapsedTime { get; set; }
        /// <summary>
        /// LocalIP
        /// </summary>
        public string LocalIP { get; set; }
        /// <summary>
        /// RemoteIP
        /// </summary>
        public string RemoteIP { get; set; }
        /// <summary>
        /// 低层使用的协议(例如：http, https, ftp)
        /// </summary>
        public string Scheme { get; set; }
        /// <summary>
        /// 控制器名称
        /// </summary>
        public string Controller { get; set; }
        /// <summary>
        /// Action名称
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}