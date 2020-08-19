using System;

namespace Kane.AspNetCore
{
    /// <summary>
    /// Api请求日志特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ApiLoggerAttribute : Attribute
    {
        /// <summary>
        /// 日志名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否忽略，默认为【否】
        /// </summary>
        public bool Ignore { get; set; } = false;
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
    }
}