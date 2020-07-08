#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展类库
* 类 名 称 ：ServiceCollectionEx
* 类 描 述 ：服务容器扩展方法
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
using System.Text.Encodings.Web;
using System.Text.Unicode;

using Microsoft.Extensions.DependencyInjection;

namespace Kane.AspNetCore
{
    /// <summary>
    /// AspNetCore服务容器扩展类
    /// </summary>
    public static class ServiceCollectionEx
    {
        #region 解决MVC视图中的中文被Html编码的问题 + IServiceCollection AddHtmlEncoder(this IServiceCollection services)
        /// <summary>
        /// 解决MVC视图中的中文被Html编码的问题
        /// </summary>
        /// <param name="services">当前服务容器</param>
        /// <returns></returns>
        public static IServiceCollection AddHtmlEncoder(this IServiceCollection services)
            => services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
        #endregion

        #region 在服务容器中注册单页Spa页面中间件，使用默认配置 + AddSpa(this IServiceCollection services)
        /// <summary>
        /// 在服务容器中注册单页Spa页面中间件，使用默认配置
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSpa(this IServiceCollection services)
            => services.AddSingleton<SpaMiddleware>();
        #endregion

        #region 在服务容器中注册单页Spa页面中间件，自定义配置 + AddSpa(this IServiceCollection services, Action<SpaOptions> options)
        /// <summary>
        /// 在服务容器中注册单页Spa页面中间件，自定义配置
        /// </summary>
        /// <param name="services">当前服务容器</param>
        /// <param name="options">配置选项</param>
        /// <returns></returns>
        public static IServiceCollection AddSpa(this IServiceCollection services, Action<SpaOptions> options)
        {
            services.Configure(options);
            return services.AddSpa();
        }
        #endregion
    }
}