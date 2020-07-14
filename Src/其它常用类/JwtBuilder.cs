#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展方法
* 类 名 称 ：JwtBuilder
* 类 描 述 ：Jwt构建器
* 所在的域 ：KK-HOME
* 命名空间 ：Kane.AspNetCore
* 机器名称 ：KK-HOME 
* CLR 版本 ：4.0.30319.42000
* 作　　者 ：Kane Leung
* 创建时间 ：2020/07/13 11:48:34
* 更新时间 ：2020/07/13 11:48:34
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Kane Leung 2020. All rights reserved.
*******************************************************************
-----------------------------------------------------------------*/
#endregion
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Kane.Extension;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kane.AspNetCore
{
    /// <summary>
    /// Jwt构建器
    /// </summary>
    public static class JwtBuilder
    {
        #region 创建JwtToken【Jwt令牌】 + BuildJwtToken(this IServiceProvider provider, IList<Claim> claims)
        /// <summary>
        /// 创建JwtToken【Jwt令牌】
        /// </summary>
        /// <param name="provider">当前服务容器</param>
        /// <param name="claims">声明集合</param>
        /// <returns></returns>
        private static JwtToken BuildJwtToken(this IServiceProvider provider, IList<Claim> claims)
        {
            var options = provider.GetService<IOptions<JwtOptions>>()?.Value;
            if (options.IsNull()) throw new NullReferenceException("Jwt配置信息为空。");
            if (claims.IsNullOrEmpty()) throw new ArgumentNullException(nameof(claims), "对象不能为空。");
            DateTime expires = DateTime.Now.AddMinutes(options.Expires);
            var credentials = new SigningCredentials(options.Key, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(options.Issuer, options.Audience, claims, expires: expires, signingCredentials: credentials);
            var token = new JwtToken(new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), RandomHelper.UUID(), expires.ToStamp());
            var cache = provider.GetService<IMemoryCache>();
            if (cache.IsNull()) throw new NullReferenceException($"{nameof(IMemoryCache)}未注入。");
            cache.Set($"_jwt_{token.Refresh_Token}", claims, TimeSpan.FromMinutes(options.Refresh));
            return token;
        }
        #endregion

        #region 创建JwtToken【Jwt令牌】 + BuildJwtToken(this IServiceProvider provider, string uniqueCode, IList<Claim> claims = null)
        /// <summary>
        /// 创建JwtToken【Jwt令牌】
        /// </summary>
        /// <param name="provider">当前服务容器</param>
        /// <param name="uniqueCode">唯一码</param>
        /// <param name="claims">声明集合</param>
        /// <returns></returns>
        private static JwtToken BuildJwtToken(this IServiceProvider provider, string uniqueCode, IList<Claim> claims = null)
        {
            claims ??= new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, uniqueCode));
            return provider.BuildJwtToken(claims);
        }
        #endregion

        #region 创建JwtToken【Jwt令牌】 + BuildJwtToken(this ControllerBase controller, string uniqueCode, IList<Claim> claims = null)
        /// <summary>
        /// 创建JwtToken【Jwt令牌】
        /// </summary>
        /// <param name="controller">当前控制器</param>
        /// <param name="uniqueCode">唯一码</param>
        /// <param name="claims">声明集合</param>
        /// <returns></returns>
        public static JwtToken BuildJwtToken(this ControllerBase controller, string uniqueCode, IList<Claim> claims = null)
            => controller.HttpContext.RequestServices.BuildJwtToken(uniqueCode, claims);
        #endregion

        #region 创建JwtToken【Jwt令牌】 + BuildJwtToken(this ControllerBase controller, IList<Claim> claims)
        /// <summary>
        /// 创建JwtToken【Jwt令牌】
        /// </summary>
        /// <param name="controller">当前控制器</param>
        /// <param name="claims">声明集合</param>
        /// <returns></returns>
        public static JwtToken BuildJwtToken(this ControllerBase controller, IList<Claim> claims)
            => controller.HttpContext.RequestServices.BuildJwtToken(claims);
        #endregion

        #region 创建JwtToken【Jwt令牌】 + BuildJwtToken(this HttpContext httpContext, string uniqueCode, IList<Claim> claims = null)
        /// <summary>
        /// 创建JwtToken【Jwt令牌】
        /// </summary>
        /// <param name="httpContext">当前上下文</param>
        /// <param name="uniqueCode">唯一码</param>
        /// <param name="claims">声明集合</param>
        /// <returns></returns>
        public static JwtToken BuildJwtToken(this HttpContext httpContext, string uniqueCode, IList<Claim> claims = null)
            => httpContext.RequestServices.BuildJwtToken(uniqueCode, claims);
        #endregion

        #region 创建JwtToken【Jwt令牌】 + BuildJwtToken(this HttpContext httpContext, IList<Claim> claims)
        /// <summary>
        /// 创建JwtToken【Jwt令牌】
        /// </summary>
        /// <param name="httpContext">当前上下文</param>
        /// <param name="claims">声明集合</param>
        /// <returns></returns>
        public static JwtToken BuildJwtToken(this HttpContext httpContext, IList<Claim> claims)
            => httpContext.RequestServices.BuildJwtToken(claims);
        #endregion

        #region 根据Refresh Token【刷新令牌】，获取新的JwtToken【Jwt令牌】 + RefreshJwtToken(this IServiceProvider provider, string refreshToken)
        /// <summary>
        /// 根据Refresh Token【刷新令牌】，获取新的JwtToken【Jwt令牌】
        /// </summary>
        /// <param name="provider">当前服务容器</param>
        /// <param name="refreshToken">Refresh Token【刷新令牌】</param>
        /// <returns></returns>
        public static JwtToken RefreshJwtToken(this IServiceProvider provider, string refreshToken)
        {
            var cache = provider.GetService<IMemoryCache>();
            if (cache.IsNull()) throw new NullReferenceException($"{nameof(IMemoryCache)}未注入。");
            var cacheKey = $"_jwt_{refreshToken}";
            if (cache.TryGetValue(cacheKey, out IList<Claim> claims))
            {
                var token = provider.BuildJwtToken(claims);
                cache.Remove(cacheKey);
                return token;
            }
            else return null;
        }
        #endregion

        #region 根据Refresh Token【刷新令牌】，获取新的JwtToken【Jwt令牌】 + RefreshJwtToken(this ControllerBase controller, string refreshToken)
        /// <summary>
        /// 根据Refresh Token【刷新令牌】，获取新的JwtToken【Jwt令牌】
        /// </summary>
        /// <param name="controller">当前控制器</param>
        /// <param name="refreshToken">Refresh Token【刷新令牌】</param>
        /// <returns></returns>
        public static JwtToken RefreshJwtToken(this ControllerBase controller, string refreshToken)
            => controller.HttpContext.RequestServices.RefreshJwtToken(refreshToken);
        #endregion

        #region 根据Refresh Token【刷新令牌】，获取新的JwtToken【Jwt令牌】 + RefreshJwtToken(this HttpContext httpContext, string refreshToken)
        /// <summary>
        /// 根据Refresh Token【刷新令牌】，获取新的JwtToken【Jwt令牌】
        /// </summary>
        /// <param name="httpContext">当前上下文</param>
        /// <param name="refreshToken">Refresh Token【刷新令牌】</param>
        /// <returns></returns>
        public static JwtToken RefreshJwtToken(this HttpContext httpContext, string refreshToken)
            => httpContext.RequestServices.RefreshJwtToken(refreshToken);
        #endregion
    }
}