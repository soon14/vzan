
##发布教程
【由于腾讯对小程序的运营规范要求严格，现在发布小程序必须保证服务类目+小程序内容页面一致，才能确保发布成功】
个人主体小程序：只能绑定单页版/行业版
企业主体小程序可以绑定所有模板，但是针对特殊行业（下面链接里）需要提供相应的资质

（1）<a href="https://developers.weixin.qq.com/miniprogram/product/material.html" target="_blank">特殊行业所需要的资质材料</a>

（2）<a href="https://developers.weixin.qq.com/miniprogram/product/material.html" target="_blank">小程序企业开放的服务范围类目表</a> 

（3）<a href="https://developers.weixin.qq.com/miniprogram/product/material.html" target="_blank">代码审核指引</a>


##准备工作：
1，先登陆微信小程序管理后台：<a href="https://mp.weixin.qq.com" target="_blank">https://mp.weixin.qq.com</a> 拿到小程序的appid和AppSecret
2，打开：设置-开发设置，设置服务器域名（之前授权过的小程序账号不需要再设置）
![](/img/发布-1.png)

**request合法域名（4个）**

 `https://cityapi.vzan.com`

`https://wtApi.vzan.com`

`https://log.aldwx.com`

`https://i.vzan.cc`

**socket合法域名（1个）**

`wss://dzwss.xiaochengxu.com.cn`


**uploadFile合法域名（3个）**
 
`https://cityapi.vzan.com`

`https://wtApi.vzan.com`

`https://i.vzan.cc`


**downloadFile合法域名（5个）**

`https://cityapi.vzan.com`

`https://wtApi.vzan.com`

`https://j.vzan.cc`

`https://i.vzan.cc`

`https://vzan-img.oss-cn-hangzhou.aliyuncs.com`

**3.注意：第三方服务如果之前授权过的请停止授权**

![](/img/发布-2.png)

##开始配置
登陆小未平台后台

【由于腾讯对小程序的运营规范要求严格，现在发布小程序必须保证服务类目+小程序内容页面一致，才能确保发布成功】

设置小程序APPID/小程序密钥

![](/img/发布-3.png)

小程序编辑之后，发布流程如下：
1.下载小程序代码，不同的模板有不同的代码包，在小程序后台可以直接下载，把代码包下载保存到桌面之后，解压到【当前文件夹】

![](/img/发布-4.png)

![](/img/发布-5.png)

2.下载小程序开发工具

<a href="https://developers.weixin.qq.com/miniprogram/dev/devtools/download.html" target="_blank">https://developers.weixin.qq.com/miniprogram/dev/devtools/download.html</a>

根据系统下载对应版本

![](/img/发布-6.png)

查看电脑的系统，下载对应的开发者工具

![](/img/发布-7.png)

下载对应的开发者工具—安装到电脑—扫码登录（需小程序账号的开发者扫码）—选择【小程序项目】

![](/img/发布-8.png)


**注意：登录账号需要是开发者**

![](/img/发布-9-1.png)

![](/img/发布-9.png)

![](/img/发布-10.png)



3.选择好小程序的代码包/填写好小程序的APPID/项目名称之后选择进入开发者工具

![](/img/发布-11.png)

打开app.js文件

`专业版：找到第80行，将appid改为自己的小程序appid`

`单页版：找到第20行，将appid改为自己的小程序appid`

`行业版：找到第16行，将appid改为自己的小程序appid`

`智慧餐厅：找到174行，将appid填到引号内`

`餐饮版：找到第30行，将appid改为自己的小程序appid`

`智推版：找到第96行，将appid改为自己的小程序appid`

`企业版：找到第38行，将appid改为自己的小程序appid`

`电商版：找到第32行，将appid改为自己的小程序appid`

`平台小程序：找到第142行，将appid改为自己的小程序appid`

`台子小程序：找到第164行，将appid改为自己的小程序appid`

`同城小程序：找到第75行，将appid改为自己的小程序appid`


![](/img/发布-12.png)

4.点击编译--预览，页面正常显示之后再上传代码

![](/img/发布-13.png)

5.上传代码

![](/img/发布-上传.png)

6.登陆微信小程序管理后台，打开开发管理，找到刚才上传的小程序代码

![](/img/发布-14.png)

7.点击提交审核，勾选同意规则，点下一步

![](/img/发布-15.png)

服务类目选择自己的小程序对应的服务类目，标签写自己小程序的关键词

功能页面：选择路径：

`专业版：pages/index/index`

`行业版：pages/index/index`

`单页版：pages/shopcar/shopcar`

`智慧餐厅：pages/restaurant/restaurant-home/index`

`餐饮版：pages/home/home`

`智推版：pages/index/index`

`企业版：pages/launch/launch`

`电商版：pages/index/index`

`平台小程序：pages/home/home-index/index`

`子小程序：pages/home/shop-detail/index`

`同城小程序：pages/index`

![](/img/发布-16.png)


8.提交审核，等待微信审核

![](/img/发布-17.png)

8.审核通过，管理员微信会收到通知
进入微信公众平台--开发管理--发布审核通过的小程序

![](/img/发布-18.png)

【以上为发布一个小程序的流程，下次发布另外一个小程序的流程如下】
左上角--项目--新建项目

![](/img/发布-19.png)