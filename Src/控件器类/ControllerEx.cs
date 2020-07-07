#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展方法
* 类 名 称 ：ControllerEx
* 类 描 述 ：控件器扩展方法
* 所在的域 ：KK-HOME
* 命名空间 ：Kane.AspNetCore
* 机器名称 ：KK-HOME 
* CLR 版本 ：4.0.30319.42000
* 作　　者 ：Kane Leung
* 创建时间 ：2020/07/05 11:25:26
* 更新时间 ：2020/07/05 11:25:26
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Kane Leung 2020. All rights reserved.
*******************************************************************
-----------------------------------------------------------------*/
#endregion
using Kane.Extension;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Kane.AspNetCore
{
    /// <summary>
    /// 控制器扩展类
    /// </summary>
    public static class ControllerEx
    {
        #region 根据UserAgent，判断是否为手机或移动端 + IsMobileAgent(this ControllerBase controller)
        /// <summary>
        /// 根据UserAgent，判断是否为手机或移动端
        /// </summary>
        /// <param name="controller">当前控制器</param>
        /// <returns></returns>
        public static bool IsMobileAgent(this ControllerBase controller)
        {
            var userAgent = controller.HttpContext.Request.Headers[HeaderNames.UserAgent].ToString();
            if (string.IsNullOrEmpty(userAgent)) return false;
            else return WebHelper.IsMobileAgent(userAgent);
        }
        #endregion
    }
}