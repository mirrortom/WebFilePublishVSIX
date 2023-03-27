# WebFilePublishVSIX
vs2022 Community,razor页面生成插件
## 主要功能
编译Razor页面生成html,压缩,复制到输出目录.  
js,css,html的mini压缩输出依赖[NUglify](https://github.com/xoofx/NUglify)这个工具  
sass,stylus,less编译依赖[WebCompiler](https://github.com/madskristensen/WebCompiler)插件  

[Doc文档](https://mirrortom.github.io/doc/webvsix.html)  

#### [Community.VisualStudio.Toolkit](https://github.com/VsixCommunity)
插件新开发版本使用了这个库,比起使用原生的VSSDK开发,要简便很多.vsct文件有提示,直接和类关联了,修改xml时,类也对应改变.代码大幅简化了,很大程度的简化了插件开发.
#### 新开发版本
1. 项目分为2部分,WebFilePublishVSIX(插件项目)和VSIXService(windows服务),由服务提供编译razor等功能,插件和服务通过socket通信.
2. 发布项目命令,只发布项目内文件,去掉了编译项目.
3. 编译razor使用了项目[RazorService](https://github.com/mirrortom/RazorService)
4. 安装时,除了插件,还要安装VSIXService服务.将VSIXService发布后,使用sc.exe安装.
5. 还是4个命令,除了右件菜单,还加入在"扩展"->"PublishWeb"下.
   1. 发布编辑文件Alt+QQ
   2. 发布选中文件Alt+11
   3. 发布选中文件夹Alt+22
   4. 发布整个项目Alt+33
