#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展类库
* 类 名 称 ：EventedStreamStringReader
* 类 描 述 ：内部方法
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
#if NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.Text;

namespace Kane.AspNetCore
{
    /// <summary>
    /// Captures the completed-line notifications from a <see cref="EventedStreamReader"/>,
    /// <para>combining the data into a single <see cref="string"/>.</para>
    /// <para>https://github.com/dotnet/aspnetcore/blob/3.0/src/Middleware/SpaServices.Extensions/src/Util/EventedStreamStringReader.cs</para>
    /// </summary>
    internal class EventedStreamStringReader : IDisposable
    {
        private EventedStreamReader _eventedStreamReader;
        private bool _isDisposed;
        private StringBuilder _stringBuilder = new StringBuilder();

        public EventedStreamStringReader(EventedStreamReader eventedStreamReader)
        {
            _eventedStreamReader = eventedStreamReader ?? throw new ArgumentNullException(nameof(eventedStreamReader));
            _eventedStreamReader.OnReceivedLine += OnReceivedLine;
        }

        public string ReadAsString() => _stringBuilder.ToString();

        private void OnReceivedLine(string line) => _stringBuilder.AppendLine(line);

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _eventedStreamReader.OnReceivedLine -= OnReceivedLine;
                _isDisposed = true;
            }
        }
    }
}
#endif