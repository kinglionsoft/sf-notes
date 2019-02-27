# 准备开发环境

## SDK 安装

* SDK： [https://docs.microsoft.com/zh-cn/azure/service-fabric/service-fabric-get-started](https://docs.microsoft.com/zh-cn/azure/service-fabric/service-fabric-get-started)
* Service Fabric CLI: 基于Python实现的兼容Windows和Linux的管理命令行工具。[https://docs.microsoft.com/zh-cn/azure/service-fabric/service-fabric-cli](https://docs.microsoft.com/zh-cn/azure/service-fabric/service-fabric-cli)


## 应用账户

* 默认情况下使用Network Service启动应用；
* 使用 指定账户 启动应用： https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-application-runas-security
* 使用 gMSA 

``` bash

New-ADServiceAccount gmsa-sf-app -DNSHostName gmsa-sf-app.yx.com -PrincipalsAllowedToRetrieveManagedPassword SFHosts -KerberosEncryptionType RC4,AES128,AES256  -ServicePrincipalNames http/gmsa-sf-app/yx

```

## 部署到生产环境

### 部署 
https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-visualstudio-configure-secure-connections

#### 指定节点

* 节点属性和放置约束： https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-cluster-resource-manager-cluster-description#placement-constraints-and-node-properties

* ServiceManifest.xml

```xml
<StatelessServiceType ServiceTypeName="GuestHelloType" UseImplicitHost="true">
   <PlacementConstraints>(NodeType==NodeType0)</PlacementConstraints>
</StatelessServiceType>

```

#### 一个节点运行多个实例

* 暂不支持 SingletonPartition 的 StatelessService 在单个节点上运行多个实例：https://github.com/Azure/service-fabric-issues/issues/325

#### VSTS-Agent

* 在使用TFS发布时，VSTS-Agent的服务账户（Windows 服务中设置）需要设置为SFAdmins中的成员。

### HTTP 反向代理

#### 代理后的地址

* https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reverseproxy

```
http://sf.yx.com:19081/GuestExeSample/GuestHello
```

#### Guest Executable
* 需要在Endpoint上添加**UriScheme="http"**，否则代理网关会返回：404 FABRIC_E_ENDPOINT_NOT_FOUND

``` xml
<Endpoints>
    <Endpoint Name="GuestHelloTypeEndpoint" Port="8088" Protocol="http" UriScheme="http" Type="Input" />
</Endpoints>
```

#### 404
By default, HTTP based services that return a 404 will actually kind of confuse the reverse proxy into thinking it can't locate the service unless you add the following header to the response:

```
X-ServiceFabric : ResourceNotFound
```

### TCP 反向代理

内置的反向代理不支持TCP

#### 解决方案一：部署到指定节点，再代理到外网

```xml
<StatelessServiceType ServiceTypeName="GrpcHelloType" UseImplicitHost="true" >
    <!-- 指定部署的节点类型 -->
    <PlacementConstraints>(NodeName==SFMaster)</PlacementConstraints>
</StatelessServiceType>
```

### GRPC 反向代理

http 反向代理后，访问 192.168.0.12:19081/GuestExeSample/GrpcHello： Name resolution failure

``` 
StatusCode=Unavailable, Detail="Name resolution failure"

dns resolution failed (will retry):
 "created":"@1551168011.207000000","description":"OS Error","file":"T:\src\github\grpc\workspace_csharp_ext_windows_x86\src\core\lib\iomgr\resolve_address_windows.cc","file_line":96,"os_error":"The specified class was not found.\r\n","syscall":"getaddrinfo","target_address":"192.168.0.12:19081/GuestExeSample/GrpcHello","wsa_error":10109}
```

#### 解决方案一：Nginx

``` 
server {
    listen 9001 http2;
    location / {
        grpc_pass grpc://192.168.0.12:19081/GuestExeSample/GrpcHello;
    }
}

```

### 读取配置

## Errors

#### 提供HTTP服务的 Guest Executable 部署后，访问时 504

* 修改坚挺地址 http://*:8088/ => http://+:8088/