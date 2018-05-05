using IOA.IOA_Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOA_Vict
{
    class Program
    {

       
        static void Main(string[] args)
        {
            Console.WriteLine("测试IOA");
            IOAKernel io_test = new IOAKernel();
            string hello_message = "Hello, I come from Miniboom";
            string test_recv = string.Empty;

            IOAKernel.SendToMatlabData test_send = new IOAKernel.SendToMatlabData();
            test_send.a = 1;
            test_send.b = 1;
            test_send.c = 1;
            test_send.d = 1;
            test_send.e = 1;
            test_send.f = 1;

            IOAKernel.RecvFromMatlabData test_recv_a = new IOAKernel.RecvFromMatlabData();

            Task sendStringToAdress = new Task(() =>
            {
                while (true)
                {
                    if (io_test == null)
                    {
                        break;
                    }
                    else
                    {
                        io_test.SendStructToAdress(test_send);
                        io_test.SendStringToAdress(hello_message,"192.168.1.44",10011);                        
                    }
                    Thread.Sleep(50);
                }
            });

            sendStringToAdress.Start();

            //激活监听程序，只需要启动一次.
            io_test.StartRecvNotify();

            Task recvStringFromAddress = new Task(() =>
            {
                while (true)
                {                    
                    if (io_test == null)
                    {
                        io_test = new IOAKernel();
                    }

                    test_recv_a = io_test.RecvNotifyToStruct();
                    Console.WriteLine("接收到{0},{1}",test_recv_a.a,test_recv_a.b);
                }
            });
            recvStringFromAddress.Start();
            Console.ReadKey();
        }
    }
}
