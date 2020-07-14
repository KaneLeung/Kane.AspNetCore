#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展类库
* 类 名 称 ：JwtOptions
* 类 描 述 ：Jwt配置项实体模型
* 所在的域 ：KK-HOME
* 命名空间 ：Kane.AspNetCore
* 机器名称 ：KK-HOME 
* CLR 版本 ：4.0.30319.42000
* 作　　者 ：Kane Leung
* 创建时间 ：2020/07/05 11:00:35
* 更新时间 ：2020/07/05 11:00:35
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Kane Leung 2020. All rights reserved.
*******************************************************************
-----------------------------------------------------------------*/
#endregion

using Microsoft.IdentityModel.Tokens;

using Kane.Extension;

#if NETCOREAPP3_1
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif

namespace Kane.AspNetCore
{
    /// <summary>
    /// JWT身份认证选项
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// 发行方
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 订阅方
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// AccessToken有效期分钟数
        /// </summary>
        public int Expires { get; set; } = 10;
        /// <summary>
        /// RefreshToken有效期分钟数
        /// </summary>
        public int Refresh { get; set; } = 12;
        /// <summary>
        /// Jwt密钥
        /// </summary>
        [JsonIgnore]
        internal SecurityKey Key
        {
            get
            {
                Secret.ThrowIfNull(nameof(Secret));
                var temp = Secret.ToBytes();
                if (temp.Length <= 16)//Jwt密钥不足16位会报错
                {
                    var hash = new HashHelper();
                    temp = hash.Md5Bytes(Secret);
                }
                return new SymmetricSecurityKey(temp);
            }
        }
    }
}