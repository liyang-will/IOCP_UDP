# 接口说明



## 一、接收接口说明：

**在使用IOA的接收功能之前，有且需要调用一次StartRecvNotify()接口，进而接收到来自于填在IOA.xml中的Recv IP和端口的消息。**

#### 1、RecvNotifyToString();

是以string类型的方式得到传来的数据

#### 2、RecvNotifyToStruct(Type structType);

是以定义好的结构体得到传来的数据

#### 3、RecvNotifyToBytes();

是以byte数组得到传来的数据



```c#
    public void StartRecvNotify();
	public string RecvNotifyToString();
    public object RecvNotifyToStruct(Type structType);
    public byte[] RecvNotifyToBytes();
```


## 二、发送接口说明：

主要分为两组发送模式，一组是直接传入参数，数据会发到填在IOA.xml的Send IP和端口；另一种是需要填入传入的IP和端口。



~~~c#
 	//发送byte[]数组
	public bool SendBytesToAdress(byte[] content);
	//发送定义的结构体
    public bool SendStructToAdress(object structObj);
	//发送string字符串
    public bool SendStringToAdress(string data_content);
	
	//发送byte[]数组到指定地址
  	public bool SendBytesToAdress(byte[] content, string IP, int port);
	//发送定义的结构体到指定地址
 	public bool SendStructToAdress(object structObj, string IP, int port);
	//发送string字符串到指定地址
    public bool SendStringToAdress(string data_content, string IP, int port);

   
~~~

