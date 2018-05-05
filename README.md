# PSDIO简介

+ 简介
+ 研发规划
+ 团队维护及加入

## 简介

**PSDIO（Publish-Pubscribe Distribute I/O）**是基于发布订阅模式，进行高并发I/O处理及数据分发的简称。是一款**好用的消息通讯中间件**。基于windows系统底层使用c#开发。使用IOCP充分调度系统内核进行高频率的I/O操作。支持UDP、TCP和Http。消息通知机制基于发布订阅模式，方便对业务逻辑进行解耦；使用生产消费模式处理队列缓存，平衡数据的产生和消耗；创造缓存池为异步对象池中的每一个异步套接字分配合适的缓存区大小，管理零碎缓存，避免不必要的内存开销。

**PSDIO**致力于成为**随调随用，灵活可插拔式**的高可用性能强劲的消息通讯中间件。基于高解耦的发布订阅设计，使得开发者只需要关心业务逻辑的开发和设计，不必担心逻辑耦合的情况，通讯则放心交给**PSDIO**来处理。



## 研发规划

有两个大的阶段：**个人开发阶段**和**团队开发阶段**

**个人开发阶段**：由于目前网上没有详细针对UDP部分的IOCP技术，所以PSDIO开发者首先针对UDP部分进行了开发。大的方向是先将中间件的模式特点实现，比如发布订阅，qos策略，生产消费处理消息队列缓存等等。更进一步是支持更多的通讯协议，比如TCP、HTTP。最后是向网络集群发展，采用共享内存等方式实现分布式通讯。

**团队开发阶段**：后来会有小伙伴陆续加入PSDIO，会考虑到往框架方向发展。陆续加入数据库模块、日志模块、以及其他的框架必备模块。**但PSDIO的核心特点以及设计初衷始终保持不变：坚持高解耦的设计模式，永远使得使用者只需要简单地关心核心业务逻辑。**



## 团队维护及加入

由于目前是个人开发阶段，此中间件将在一定时间内处于封闭测试以及试用阶段。所以此项目会阶段性的出现大的更新，后期系统稳定并可广泛用于商业项目中后将会建立QQ群，进行技术探讨。开源的成功一定是来自于团队的共同价值观和对技术的热爱追求。欢迎各位加入，只要你觉得能使PSDIO变得更好用，性能更强，PSDIO最初开发者会考虑您的加入，也会将所有代码贡献者加入PSDIO开发感谢者名录中，为您的技术形象加分。











