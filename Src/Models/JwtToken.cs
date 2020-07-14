#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展类库
* 类 名 称 ：JwtToken
* 类 描 述 ：Jwt令牌实体
* 所在的域 ：KK-HOME
* 命名空间 ：Kane.AspNetCore
* 机器名称 ：KK-HOME 
* CLR 版本 ：4.0.30319.42000
* 作　　者 ：Kane Leung
* 创建时间 ：2020/07/13 11:00:35
* 更新时间 ：2020/07/13 11:00:35
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Kane Leung 2020. All rights reserved.
*******************************************************************
-----------------------------------------------------------------*/
#endregion
#if NETCOREAPP3_1
using Kane.Extension.Json;
#else
using Kane.Extension.JsonNet;
#endif

namespace Kane.AspNetCore
{
    /// <summary>
    /// Jwt令牌实体
    /// </summary>
    public class JwtToken
    {
        internal JwtToken(string accessToken, string refreshToken, int expires)
        {
            Access_Token = accessToken;
            Refresh_Token = refreshToken;
            Expires = expires;
        }
        /// <summary>
        /// Jwt令牌
        /// </summary>
        public string Access_Token { get; }
        /// <summary>
        /// 刷新所需的令牌
        /// </summary>
        public string Refresh_Token { get; }
        /// <summary>
        /// 过期时间戳
        /// </summary>
        public int Expires { get; }
        /// <summary>
        /// 将本实体转成Json字符串
        /// </summary>
        /// <param name="lowerCase">是否全部小写</param>
        /// <returns></returns>
        public string ToJson(bool lowerCase = false)
        {
            if (lowerCase) return new { access_token = Access_Token, refresh_token = Refresh_Token, expires = Expires }.ToJson();
            else return new { Access_Token, Refresh_Token, Expires }.ToJson();
        }
    }
}