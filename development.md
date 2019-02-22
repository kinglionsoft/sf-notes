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


### 反向代理

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

## Errors

#### 提供HTTP服务的 Guest Executable 部署后，访问时 504

* 修改坚挺地址 http://*:8088/ => http://+:8088/