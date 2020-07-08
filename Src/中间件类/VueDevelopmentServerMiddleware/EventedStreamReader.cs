#region << 版 本 注 释 >>
/*-----------------------------------------------------------------
* 项目名称 ：Kane.AspNetCore
* 项目描述 ：Asp.NetCore通用扩展类库
* 类 名 称 ：EventedStreamReader
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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kane.AspNetCore
{
    /// <summary>
    /// Wraps a <see cref="StreamReader"/> to expose an evented API, issuing notifications
    /// <para>when the stream emits partial lines, completed lines, or finally closes.</para>
    /// <para>https://github.com/dotnet/aspnetcore/blob/3.0/src/Middleware/SpaServices.Extensions/src/Util/EventedStreamReader.cs</para>
    /// </summary>
    internal class EventedStreamReader
    {
        public delegate void OnReceivedChunkHandler(ArraySegment<char> chunk);
        public delegate void OnReceivedLineHandler(string line);
        public delegate void OnStreamClosedHandler();

        public event OnReceivedChunkHandler OnReceivedChunk;
        public event OnReceivedLineHandler OnReceivedLine;
        public event OnStreamClosedHandler OnStreamClosed;

        private readonly StreamReader _streamReader;
        private readonly StringBuilder _linesBuffer;

        public EventedStreamReader(StreamReader streamReader)
        {
            _streamReader = streamReader ?? throw new ArgumentNullException(nameof(streamReader));
            _linesBuffer = new StringBuilder();
            Task.Factory.StartNew(Run);
        }

        public Task<Match> WaitForMatch(Regex regex)
        {
            var tcs = new TaskCompletionSource<Match>();
            var completionLock = new object();
            OnReceivedLineHandler onReceivedLineHandler = null;
            OnStreamClosedHandler onStreamClosedHandler = null;
            void ResolveIfStillPending(Action applyResolution)
            {
                lock (completionLock)
                {
                    if (!tcs.Task.IsCompleted)
                    {
                        OnReceivedLine -= onReceivedLineHandler;
                        OnStreamClosed -= onStreamClosedHandler;
                        applyResolution();
                    }
                }
            }
            onReceivedLineHandler = line =>
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    ResolveIfStillPending(() => tcs.SetResult(match));
                }
            };
            onStreamClosedHandler = () =>
            {
                ResolveIfStillPending(() => tcs.SetException(new EndOfStreamException()));
            };
            OnReceivedLine += onReceivedLineHandler;
            OnStreamClosed += onStreamClosedHandler;
            return tcs.Task;
        }

        private async Task Run()
        {
            var buf = new char[8 * 1024];
            while (true)
            {
                var chunkLength = await _streamReader.ReadAsync(buf, 0, buf.Length);
                if (chunkLength == 0)
                {
                    if (_linesBuffer.Length > 0)
                    {
                        OnCompleteLine(_linesBuffer.ToString());
                        _linesBuffer.Clear();
                    }
                    OnClosed();
                    break;
                }
                OnChunk(new ArraySegment<char>(buf, 0, chunkLength));
                int startPos = 0;
                int lineBreakPos;
                // get all the newlines
                while ((lineBreakPos = Array.IndexOf(buf, '\n', startPos, chunkLength - startPos)) >= 0 && startPos < chunkLength)
                {
                    var length = (lineBreakPos + 1) - startPos;
                    _linesBuffer.Append(buf, startPos, length);
                    OnCompleteLine(_linesBuffer.ToString());
                    _linesBuffer.Clear();
                    startPos = lineBreakPos + 1;
                }
                // get the rest
                if (lineBreakPos < 0 && startPos < chunkLength) _linesBuffer.Append(buf, startPos, chunkLength);
            }
        }

        private void OnChunk(ArraySegment<char> chunk) => OnReceivedChunk?.Invoke(chunk);
        private void OnCompleteLine(string line) => OnReceivedLine?.Invoke(line);
        private void OnClosed() => OnStreamClosed?.Invoke();
    }
}
#endif