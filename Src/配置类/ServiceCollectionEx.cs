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
* 创建时间 ：2020/07/13 11:48:34
* 更新时间 ：2020/07/13 11:48:34
* 版 本 号 ：v1.0.1.0
*******************************************************************
* Copyright @ Kane Leung 2020. All rights reserved.
*******************************************************************
-----------------------------------------------------------------*/
#endregion
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using Kane.Extension;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

        #region 在服务容器中注册Jwt认证 + AddJwtAuthentication(this IServiceCollection services, string sectionKey = "jwt")
        /// <summary>
        /// 在服务容器中注册Jwt认证
        /// </summary>
        /// <param name="services">当前服务容器</param>
        /// <param name="sectionKey">配置文件中的节点</param>
        /// <returns></returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, string sectionKey = "jwt")
        {
            services.Configure<JwtOptions>(services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection(sectionKey));
            var options = services.BuildServiceProvider().GetRequiredService<IOptions<JwtOptions>>()?.Value;
            if (options.Issuer.IsNullOrEmpty() || options.Audience.IsNullOrEmpty() || options.Secret.IsNullOrEmpty())
                throw new NullReferenceException("Jwt配置信息为空。");
            services.AddMemoryCache();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.SaveToken = true;
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,//是否验证Issuer
                    ValidateAudience = true,//是否验证Audience
                    ValidateIssuerSigningKey = true,//是否验证SecurityKey
                    ValidateLifetime = true,//是否验证失效时间
                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = options.Key,
                    ClockSkew = TimeSpan.Zero//将默认的时间偏移量300s，设置为0
                };
            });
            return services;
        }
        #endregion
    }
}