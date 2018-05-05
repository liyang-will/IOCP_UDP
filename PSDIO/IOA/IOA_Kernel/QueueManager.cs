/*
    基于生产消费者模式
    接收消息队列处理
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOA_Execute.IOA_Kernel
{
    public class QueueManager
    {
        private ConcurrentQueue<byte[]> _notificationCenter;
        private byte[] _carrier;
        public QueueManager()
        {
            _notificationCenter = new ConcurrentQueue<byte[]>();
        }

        /// <summary>
        /// 将消息压入队列
        /// </summary>
        /// <param name="newMessage"></param>
        /// <returns></returns>
        public bool PushMessageToQueue(byte[] newMessage)
        {
            try
            {
                _notificationCenter.Enqueue(newMessage);
            }
            catch
            {
                Console.WriteLine("there is a problem in pushing message into Concurrentqueue");
            }
            return true;
        }

        /// <summary>
        /// 从队列取出消息
        /// </summary>
        /// <returns></returns>
        public byte[] GetMessageFromQueue()
        {
            try
            {
                if (_notificationCenter.Count <= 0)
                {
                    return null;
                }
                else
                {
                    _notificationCenter.TryDequeue(out _carrier);
                }
            }
            catch
            {
                Console.WriteLine("there is a problem in getting message from Concurrentqueue");
            }
            return _carrier;
        }


    }
}
