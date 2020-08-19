#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展类库
* 类 名 称 ：ApplicationBuilderEx
* 类 描 述 ：管道扩展方法
* 所在的域 ：KK-HOME
* 命名空间 ：Kane.AspNetCore
* 机器名称 ：KK-HOME 
* CLR 版本 ：4.0.30319.42000
* 作　　者 ：Kane Leung
* 创建时间 ：2020/07/05 11:42:37
* 更新时间 ：2020/07/05 11:42:37
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Kane Leung 2020. All rights reserved.
*******************************************************************
-----------------------------------------------------------------*/
#endregion
using Microsoft.AspNetCore.Builder;

namespace Kane.AspNetCore
{
    /// <summary>
    /// 管道扩展类
    /// </summary>
    public static class ApplicationBuilderEx
    {
        /// <summary>
        /// 添加<see cref="SpaMiddleware"/>到管道中
        /// </summary>
        /// <param name="builder">管道</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSpa(this IApplicationBuilder builder) => builder.UseMiddleware<SpaMiddleware>();

#if NETCOREAPP3_1
        /// <summary>
        /// 添加<see cref="ApiLoggerMiddleware"/>到管道中
        /// </summary>
        /// <param name="builder">管道</param>
        /// <returns></returns>
        public static IApplicationBuilder UseApiLogger(this IApplicationBuilder builder) => builder.UseMiddleware<ApiLoggerMiddleware>();
#endif
    }
}