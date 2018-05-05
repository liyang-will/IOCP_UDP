/*
=================================
    * @Author：Miniboom
    * @StartTime：2018-4-19
    * Version: v3.0
=================================
模块介绍：
    基于UDP通讯的IOCP模块，对每个异
步套接字提供了端口自动完成功能，开发
者不需要关注线程池的操作、套接字的回
收和碎片化缓存的管理，只需要调用此处
的接口，模块自动完成这些操作。
=================================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IOA
{

    public class BufferManager
    {
        int numBytes;
        byte[] buffer;
        Stack<int> freeIndexPool;
        int currentIndex;
        int bufferSize;
        Random random;

        /// <summary>
        /// 缓存管理，创建非零散缓存块
        /// </summary>
        /// <param name="totalBytes">缓存的总大小</param>
        /// <param name="bufferSize">每个缓存的大小</param>
        public BufferManager(int totalBytes, int bufferSize)
        {
            numBytes = totalBytes;
            currentIndex = 0;
            this.bufferSize = bufferSize;
            freeIndexPool = new Stack<int>();
            random = new Random();

        }

        /// <summary>
        /// 初始化缓存
        /// </summary>
        public void InitBuffer()
        {
            buffer = new byte[numBytes];
            //随机填写消息
            random.NextBytes(buffer);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="args">增强型异步缓存上下文类</param>
        /// <returns></returns>
        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (freeIndexPool.Count > 0)
            {
                args.SetBuffer(buffer, freeIndexPool.Pop(), bufferSize);
            }
            else
            {
                if ((numBytes - bufferSize) < currentIndex)
                {
                    return false;
                }
                args.UserToken = currentIndex;
                args.SetBuffer(buffer, currentIndex, bufferSize);
                currentIndex += bufferSize;
            }
            return true;
        }

        public void SetBufferValue(SocketAsyncEventArgs args, byte[] value)
        {
            int offsize = (int)args.UserToken;
            for (int i = offsize; i < bufferSize + offsize; i++)
            {
                if (i >= value.Length)
                {
                    break;
                }
                buffer[i] = value[i - offsize];
            }
        }

        /// <summary>
        /// 释放缓存
        /// </summary>
        /// <param name="args">增强型异步缓存上下文类</param>
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            freeIndexPool.Push(args.Offset);

            args.SetBuffer(null, 0, 0);
        }
    }
}
