
using Kane.AspNetCore;
using Kane.Extension;
using Kane.Extension.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

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
        public string Get()
        {
            var temp = this.BuildJwtToken(RandomHelper.UUID());
            return new { code = 0, message = "ok", data = temp }.ToJson();
        }

        /// <summary>
        /// 校验Jwt
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("Check")]
        public string Check() => new { code = 0, message = "ok" }.ToJson();

        /// <summary>
        /// 根据刷新Token重新获取JwtToken
        /// </summary>
        /// <param name="token">刷新Token</param>
        /// <returns></returns>
        [HttpPost("Refresh")]
        public string Refresh(RefreshData data)
        {
            var temp = this.RefreshJwtToken(data.Token);
            if (temp.IsNotNull()) return new { code = 0, message = "ok", data = temp }.ToJson();
            return new { code = 403, message = "刷新令牌已失效" }.ToJson();//刷新令牌失效，返回403
        }

        public class RefreshData
        {
            [JsonProperty("token")]
            public string Token { get; set; }
        }
    }
}