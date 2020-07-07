#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展方法
* 类 名 称 ：SpaOptions
* 类 描 述 ：Spa配置项实体模型
* 所在的域 ：KK-HOME
* 命名空间 ：Kane.AspNetCore
* 机器名称 ：KK-HOME 
* CLR 版本 ：4.0.30319.42000
* 作　　者 ：Kane Leung
* 创建时间 ：2020/07/05 11:05:25
* 更新时间 ：2020/07/05 11:05:25
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Kane Leung 2020. All rights reserved.
*******************************************************************
-----------------------------------------------------------------*/
#endregion
using System.Collections.Generic;

namespace Kane.AspNetCore
{
    /// <summary>
    /// Spa中间件配置模型
    /// </summary>
    public class SpaOptions
    {
        /// <summary>
        /// 忽略路径集合
        /// </summary>
        internal List<string> IgnorePaths = new List<string>();
        /// <summary>
        /// 映射路径集合
        /// </summary>
        internal List<string> MapPaths = new List<string>();
        /// <summary>
        /// 是否启用本中间件
        /// </summary>
        public bool Enable { get; set; } = true;
        /// <summary>
        /// 映射的Spa页面文件，默认为【index.html】
        /// </summary>
        public string StaticFiles { get; set; } = "index.html";
        /// <summary>
        /// 添加单个忽略路径，如【/api】,忽略大小写
        /// </summary>
        /// <param name="path">要忽略的路径</param>
        public void AddIgnore(string path) => IgnorePaths.Add(path);
        /// <summary>
        /// 添加多个忽略路径，如【/api】,忽略大小写
        /// </summary>
        /// <param name="paths">要忽略的路径</param>
        public void AddIgnore(params string[] paths) => IgnorePaths.AddRange(paths);
        /// <summary>
        /// 添加单个映射路径，如【/pages】,忽略大小写
        /// </summary>
        /// <param name="path">要映射路径</param>
        public void AddMap(string path) => MapPaths.Add(path);
        /// <summary>
        /// 添加多个映射路径，如【/pages】,忽略大小写
        /// </summary>
        /// <param name="paths">要映射路径</param>
        public void AddMap(params string[] paths) => MapPaths.AddRange(paths);
    }
}