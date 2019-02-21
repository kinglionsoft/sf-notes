# 集群规划

## 节点 Node
* 包含多个节点类型的群集有一个主节点类型，剩余的是非主节点类型。
* 群集的耐久性（Bronze\Silver\Gold\Platinum）描述了节点的可靠性权限，以及对应的VM集群数量的下限。
* 在生产环境中，对于每个物理机或虚拟机，Service Fabric 只支持一个节点。
## 容错域 Fauld Domain
* 故障的物理单元
* 暂时没有理解
* 生产环境中，至少3个FD，建议在群集中至少跨 5 个 FD 部署 5 个节点。
## 升级域 Upgrade Domain
* 节点的逻辑单元
* 在 Service Fabric 协调式升级（应用程序升级或群集升级）期间，将关闭 UD 中的所有节点以执行升级，而其他 UD 中的节点仍可用来为请求提供服务。
## 硬件要求
* 至少 16 GB RAM
* 至少 40 GB 可用磁盘空间
* 4 核或更高规格的 CPU
* 所有计算机与安全网络连接
* 已安装 Windows Server OS（有效版本：2012 R2、2016、1709 或 1803）
* .NET Framework 4.5.1 或更高版本的完整安装版
* Windows PowerShell 3.0
* 应在所有计算机上运行 **RemoteRegistry** 服务
* 群集管理员必须拥有每台计算机的**管理员权限**
* **不能在域控制器上安装 Service Fabric**