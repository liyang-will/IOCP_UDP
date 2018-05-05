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

namespace IOA.IOA_Kernel
{
    class SocketAsyncPool
    {
        private Stack<SocketAsyncEventArgs> pool;
        private static readonly object thislock = new object();

        public SocketAsyncPool(int capacity)
        {
            pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        public void Push(SocketAsyncEventArgs item)
        {
            if (item != null)
            {
                lock (thislock)
                {
                    pool.Push(item);
                }
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            lock (thislock)
            {
                return pool.Pop();
            }
        }

        public void AddBack()
        {
            SocketAsyncEventArgs item = new SocketAsyncEventArgs();
            pool.Push(item);
        }

        public int Count
        {
            get { return pool.Count; }
        }
    }
}
