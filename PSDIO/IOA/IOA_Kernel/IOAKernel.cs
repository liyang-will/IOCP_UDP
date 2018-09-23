using IOA_Execute.IOA_Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IOA.IOA_Kernel
{
    public class IOAKernel
    {
        #region 套接字部分
        private Socket receiveSocket;

        private Socket sendSocket;

        private SocketAsyncEventArgs sendToRecvFromSocketArgs;

        private UdpClient recvUdpCleint;

        private IPEndPoint recvEndPoint;
        #endregion

        #region 发送接收IP端口定义
        private string recvIP = string.Empty;

        private string sendIP = string.Empty;

        private int recvPort = 0;

        private int sendPort = 0;
        #endregion

        #region IO设置参数
        /// <summary>
        /// 客户端连接数
        /// </summary>
        private int m_Collection = 12;

        /// <summary>
        /// 收发消息限定大小
        /// </summary>
        private int m_writeReadSize = 1024;
        #endregion

        /// <summary>
        /// 缓存池
        /// </summary>
        private BufferManager bfCabinManager;

        /// <summary>
        /// 消息队列
        /// </summary>
        private QueueManager queueManager;

        /// <summary>
        /// 线程池
        /// </summary>
        private SocketAsyncPool readWriteSocketArgsPool;

        private static readonly object thislock = new object();
        public IOAKernel()
        {
            //缓存池初始化
            bfCabinManager = new BufferManager(m_Collection * m_writeReadSize * 2, m_writeReadSize);
            bfCabinManager.InitBuffer();

            //读取XML配置
            LoadXMLConfig();

            //消息队列初始化
            queueManager = new QueueManager();

            readWriteSocketArgsPool = new SocketAsyncPool(m_Collection);

            //初始化receiveSocket属性
            receiveSocket = new Socket(AddressFamily.InterNetwork,
                                        SocketType.Dgram, ProtocolType.Udp);

            recvEndPoint = new IPEndPoint(IPAddress.Parse(Get_Local_IP()), recvPort);

            recvUdpCleint = new UdpClient(recvEndPoint);

            //初始化sendSocket属性
            sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //为线程池添加对象和缓存
            for (int i = 0; i < m_Collection; ++i)
            {
                sendToRecvFromSocketArgs = new SocketAsyncEventArgs();
                sendToRecvFromSocketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CabinCompleted);
                bfCabinManager.SetBuffer(sendToRecvFromSocketArgs);
                readWriteSocketArgsPool.Push(sendToRecvFromSocketArgs);
            }

        }

        private void LoadXMLConfig()
        {
            XmlDocument IOAConfig = new XmlDocument();

            IOAConfig.Load(@"IOA.xml");
            XmlNode startup_part = IOAConfig.SelectSingleNode("configuration");
            XmlNodeList xnlist = startup_part.ChildNodes;
            foreach (XmlElement chird_xnl_list in xnlist)
            {
                foreach (XmlElement chird_xml_list in chird_xnl_list)
                {
                    if (Equals(chird_xml_list.Name, "Recv"))
                    {
                        recvIP = chird_xml_list.GetAttribute("IP").ToString();
                        recvPort = int.Parse(chird_xml_list.GetAttribute("Port").ToString());
                    }
                    if (Equals(chird_xml_list.Name, "Send"))
                    {
                        sendIP = chird_xml_list.GetAttribute("IP").ToString();
                        sendPort = int.Parse(chird_xml_list.GetAttribute("Port").ToString());
                    }
                }
            }
        }

        private void CabinCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceived(e);
                    break;
                case SocketAsyncOperation.SendTo:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not success!");
            }
        }

        //将数据取出放入队列，然后回收套接字        
        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            //接收并将数据写入消息队列
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                queueManager.PushMessageToQueue(e.Buffer);
                readWriteSocketArgsPool.Push(e);

            }

        }

        /// <summary>
        /// 套接字没有接收到消息时，
        /// 需要把资源回收
        /// </summary>
        /// <param name="e"></param>
        private void RecvInvailSocketRecover(SocketAsyncEventArgs e)
        {
            readWriteSocketArgsPool.Push(e);
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                readWriteSocketArgsPool.Push(e);
            }
        }

        #region 发送方法
        /// <summary>
        /// 发送byte数组
        /// </summary>
        /// <param name="content"></param>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        public bool SendBytesToAdress(byte[] content, string IP, int port)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            sendToRecvFromSocketArgs = readWriteSocketArgsPool.Pop();
            sendToRecvFromSocketArgs.RemoteEndPoint = remoteEndPoint;

            //设置发送内容
            Array.Copy(content, sendToRecvFromSocketArgs.Buffer, content.Length);

            sendToRecvFromSocketArgs.SetBuffer(0, content.Length);

            sendSocket.SendToAsync(sendToRecvFromSocketArgs);
            return true;
        }

        /// <summary>
        /// 发送struct结构体
        /// </summary>
        /// <param name="content"></param>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        public bool SendStructToAdress(object structObj, string IP, int port)
        {
            byte[] content = StructToBytes(structObj);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            sendToRecvFromSocketArgs = readWriteSocketArgsPool.Pop();
            sendToRecvFromSocketArgs.RemoteEndPoint = remoteEndPoint;

            //设置发送内容
            Array.Copy(content, sendToRecvFromSocketArgs.Buffer, content.Length);

            sendToRecvFromSocketArgs.SetBuffer(0, content.Length);

            sendSocket.SendToAsync(sendToRecvFromSocketArgs);
            return true;
        }

        /// <summary>
        /// 发送string
        /// </summary>
        /// <param name="content"></param>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        public bool SendStringToAdress(string data_content, string IP, int port)
        {
            byte[] content = Encoding.Default.GetBytes(data_content);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            sendToRecvFromSocketArgs = readWriteSocketArgsPool.Pop();
            sendToRecvFromSocketArgs.RemoteEndPoint = remoteEndPoint;

            //设置发送内容
            Array.Copy(content, sendToRecvFromSocketArgs.Buffer, content.Length);

            sendToRecvFromSocketArgs.SetBuffer(0, content.Length);
            sendSocket.SendToAsync(sendToRecvFromSocketArgs);
            return true;
        }

        /// <summary>
        /// 发送byte数组
        /// </summary>
        /// <param name="content"></param>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        public bool SendBytesToAdress(byte[] content)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(sendIP), sendPort);
            sendToRecvFromSocketArgs = readWriteSocketArgsPool.Pop();
            sendToRecvFromSocketArgs.RemoteEndPoint = remoteEndPoint;

            //设置发送内容
            Array.Copy(content, sendToRecvFromSocketArgs.Buffer, content.Length);

            sendToRecvFromSocketArgs.SetBuffer(0, content.Length);

            sendSocket.SendToAsync(sendToRecvFromSocketArgs);
            return true;
        }

        /// <summary>
        /// 发送struct结构体
        /// </summary>
        /// <param name="content"></param>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        public bool SendStructToAdress(object structObj)
        {
            byte[] content = StructToBytes(structObj);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(sendIP), sendPort);
            sendToRecvFromSocketArgs = readWriteSocketArgsPool.Pop();
            sendToRecvFromSocketArgs.RemoteEndPoint = remoteEndPoint;

            //设置发送内容
            Array.Copy(content, sendToRecvFromSocketArgs.Buffer, content.Length);

            sendToRecvFromSocketArgs.SetBuffer(0, content.Length);

            sendSocket.SendToAsync(sendToRecvFromSocketArgs);
            return true;
        }

        /// <summary>
        /// 发送string
        /// </summary>
        /// <param name="content"></param>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        public bool SendStringToAdress(string data_content)
        {
            lock (thislock)
            {
                try
                {
                    byte[] content = Encoding.Default.GetBytes(data_content);
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(sendIP), sendPort);
                    sendToRecvFromSocketArgs = readWriteSocketArgsPool.Pop();
                    sendToRecvFromSocketArgs.RemoteEndPoint = remoteEndPoint;

                    //设置发送内容
                    Array.Copy(content, sendToRecvFromSocketArgs.Buffer, content.Length);

                    sendToRecvFromSocketArgs.SetBuffer(0, content.Length);
                    sendSocket.SendToAsync(sendToRecvFromSocketArgs);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0}", e);
                }
                return true;
            }
        }

        #endregion

        #region 接收方法
        /// <summary>
        /// 激活监听
        /// </summary>
        public void StartRecvNotify()
        {

            Task recv = new Task
                (() =>
                {
                    try
                    {
                        while (true)
                        {
                            IAsyncResult iar = recvUdpCleint.BeginReceive(null, null);
                            byte[] receiveData = recvUdpCleint.EndReceive(iar, ref recvEndPoint);
                            string message_ip = recvEndPoint.Address.ToString();
                            //是来自于接收端的消息
                            if (message_ip.Equals(recvIP))
                            {
                                queueManager.PushMessageToQueue(receiveData);
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("there is a problem in receive data");
                    }
                    finally
                    {
                        recvUdpCleint.Close();
                    }
                }
                );
            recv.Start();
        }

        public byte[] RecvNotifyToBytes()
        {
            byte[] notifyUnit = queueManager.GetMessageFromQueue();
            if (notifyUnit == null)
            {
                return null;
            }
            return notifyUnit;
        }

        public RecvFromMatlabData RecvNotifyToStruct()//从序列化反编译出结构体
        {
            byte[] rawdatas = queueManager.GetMessageFromQueue();
            if (rawdatas != null)
            {
                Type anytype = typeof(RecvFromMatlabData);
                int rawsize = Marshal.SizeOf(anytype);

                if (rawsize > rawdatas.Length)
                    return new RecvFromMatlabData();
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.Copy(rawdatas, 0, buffer, rawsize);
                object retobj = Marshal.PtrToStructure(buffer, anytype);
                Marshal.FreeHGlobal(buffer);
                return (RecvFromMatlabData)retobj;
            }

            return new RecvFromMatlabData();
        }

        public string RecvNotifyToString()
        {
            byte[] notifyUnit = queueManager.GetMessageFromQueue();
            if (notifyUnit == null)
            {
                return null;
            }
            string goalString = BytesToString(notifyUnit);
            return goalString;
        }

        #endregion

        /// <summary>
        /// 结构体转byte数组
        /// </summary>
        /// <param name="structObj"></param>
        /// <returns></returns>
        private static byte[] StructToBytes(object structObj)
        {
            int size = Marshal.SizeOf(structObj);
            byte[] bytes = new byte[size];
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structObj, structPtr, false);
            Marshal.Copy(structPtr, bytes, 0, size);
            Marshal.FreeHGlobal(structPtr);
            return bytes;
        }

        /// <summary>
        /// byte数组转结构体
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="structType"></param>
        /// <returns></returns>
        private static object BytesToStruct(byte[] bytes, Type structType)
        {
            int size = Marshal.SizeOf(structType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);

            }
            catch
            {
                Console.WriteLine("there is a problem in bytes transferto structure");
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return Marshal.PtrToStructure(buffer, structType);
        }

        /// <summary>
        /// byte数组转string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string BytesToString(byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }

        private static string Get_Local_IP()
        {
            IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;
            string my_ip = addressList[1].ToString();
            return my_ip;
        }

    }
}
