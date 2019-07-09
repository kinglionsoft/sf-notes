# 本地集群

## 集群安全

> 集群安全配置不可更改，除非重建整个集群。
采用gMSA（组托管服务）配置集群安全。

### gMSA 配置

> gMSA每次改密码是由DC上面的KDS服务管理的，每次通过一个root key id，时间戳以及gMSA的SID通过某个复杂的算法生成一个随机的密码。注意这里的gMSA中的g代表的是group，也是说我们需要分配一个安全组给这个托管账户，安全组里面的所有计算机账户都可以去使用这个托管账户。
https://docs.microsoft.com/zh-cn/previous-versions/windows/server/jj128431(v=ws.11)
域控制器上配置gMSA：

#### DC 上创建gMSA
* 将集群中的主机加入到安全组：SFHosts
* 设置组托管服务帐户

``` bash

if(!(Get-KdsRootKey)) {
    Add-KdsRootKey -EffectiveTime ((get-date).addhours(-10))
}

New-ADServiceAccount gmsa-sf -DNSHostName gmsa-sf.yx.com -PrincipalsAllowedToRetrieveManagedPassword SFHosts -KerberosEncryptionType RC4,AES128,AES256  -ServicePrincipalNames ServiceFabric/gmsa-sf/yx

```
* 创建用户组SFAdmins，组内用户可以管理Service Fabric

#### 在成员服务器上配置并验证gMSA服务帐户
> 确保集群中的主机的【Remtoe Registry】服务已启动。

```bash
Add-WindowsFeature RSAT-AD-PowerShell
if(!(Test-ADServiceAccount gmsa-sf)) {
    Install-ADServiceAccount gmsa-sf
}
```

## 安装集群

以下操作都在操作机上进行。
### 准备

* 操作机：
    - 能够访问集群中所有主机
    - 登录操作机的用户有集群主机的管理员权限
    - 不能安装过Service Fabric

* 下载：https://docs.microsoft.com/zh-cn/azure/service-fabric/service-fabric-windows-cluster-windows-security
    - Microsoft.Azure.ServiceFabric.WindowsServer.xxx.zip
    - MicrosoftAzureServiceFabric.xxx.cab
> 配置文件模板和脚本都在 Microsoft.Azure.ServiceFabric.WindowsServer.xxx.zip 中。

### 集群配置

* 一个集群有多个NodeType，包括一个主要（Primary）的NodeType和多个非主要（Non-Primary）NodeType。节点归属于NodeType。NodeType 可以描述节点的特性，如：前端节点、OCR节点、物理机带显卡的节点等。

* 服务部署是通过placement constraints限定布放的节点。

* 使用  ClusterConfig.gMSA.Windows.MultiMachine.JSON 模板，复制到 ClusterConfig.json，修改 nodes 和 security 配置。

* 创建

``` bash
# 校验配置
.\TestConfiguration.ps1 -ClusterConfigFilePath .\ClusterConfig.json

# 创建
.\CreateServiceFabricCluster.ps1 -ClusterConfigFilePath .\ClusterConfig.json -FabricRuntimePackagePath ..\MicrosoftAzureServiceFabric.6.5.639.9590.cab

# 查看
Get-ServiceFabricNode |Format-Table

# 删除
.\RemoveServiceFabricCluster.ps1 -ClusterConfigFilePath .\ClusterConfig.json 

# 添加节点
 .\AddNode.ps1 -NodeName CTC -NodeType Aliyun -NodeIPAddressorFQDN 172.18.181.177 -ExistingClientConnectionEndpoint 172.16.0.3:19000 -UpgradeDomain UD5 -FaultDomain fd:/dc2/r1 -FabricRuntimePackagePath ..\MicrosoftAzureServiceFabric.6.4.637.9590.cab -AcceptEULA  -WindowsCredential
```

* 修改配置
任意节点中

```bash
Connect-ServiceFabricCluster
Get-ServiceFabricClusterConfiguration > 1.0.1.json 
# 修改1.0.1.json
Start-ServiceFabricClusterConfigurationUpgrade -ClusterConfigPath 1.0.1.json
```

* 管理页面
 http://sf.yx.com:19080/Explorer


* 辅助

``` bash
# Win10 需要安装 https://www.microsoft.com/zh-CN/download/details.aspx?id=45520
Import-Module ActiveDirectory

$group="CN=SFHosts,CN=Computers,DC=yx,DC=com"

$computers=(Get-ADComputer -Filter 'MemberOf -eq $group')

foreach($c in $computers) {
    Write-Host $c.Name "starting"
    Invoke-Command -ComputerName $c.Name -FilePath ./CleanFabric.ps1
    Invoke-Command -ComputerName $c.Name -ScriptBlock {Remove-Item C:/SF/* -recurse}
}

# Invoke-Command -ComputerName CTCT5 -ScriptBlock {ipconfig /flushdns}  

# 重启
Invoke-Command -ComputerName SF2 -ScriptBlock {Remove-Item C:/SF/* -recurse}
```

* 掉电后节点离线

``` bash
 Invoke-Command -ComputerName CTC3 -ScriptBlock {Restart-Service FabricHostSvc ; Restart-Service FabricInstallerSvc} 
```


* Node down
https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-cluster-windows-server-add-remove-nodes#remove-nodes-from-your-cluster
```bash
Connect-ServiceFabricCluster -ConnectionEndpoint  sfmaster:19000 -WindowsCredential
Get-ServiceFabricClusterConfiguration > 17.json
# 删除 Certificate
#  "$id": "1",
#      "CertificateInformation": {
#       "$id": "2"
#      },
Start-ServiceFabricClusterConfigurationUpgrade -ClusterConfigPath 17.json
Get-ServiceFabricClusterUpgrade

RemoveNode.ps1 -ExistingClientConnectionEndpoint sfmaster:19000
```

### 本地集群

#### 修改NodeType
https://stackoverflow.com/questions/37881422/how-do-i-configure-local-cluster-for-addtional-node-types
