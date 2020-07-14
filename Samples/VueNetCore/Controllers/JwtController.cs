using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Kane.AspNetCore;
using Kane.Extension;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VueNetCore.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class JwtController : ControllerBase
    {
        /// <summary>
        /// 获取JwtToken
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public object Get() => this.BuildJwtToken(RandomHelper.UUID());

        /// <summary>
        /// 校验Jwt
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("Check")]
        public object Check() => new { message = "ok" };

        /// <summary>
        /// 根据刷新Token重新获取JwtToken
        /// </summary>
        /// <param name="token">刷新Token</param>
        /// <returns></returns>
        [HttpGet("Refresh")]
        public object Refresh(string token)
        {
            var temp = this.RefreshJwtToken(token);
            if (temp.IsNotNull()) return temp;
            return new { message = "刷新令牌已失效。" };
        }
    }
}
