# WebFilePublishVSIX
vs2022+ Community,razor页面生成插件
## 主要功能
编译Razor页面生成html,压缩,复制到输出目录.  
[Doc文档](https://mirrortom.github.io/doc/webvsix.html)  
## 插件开发库
#### [Community.VisualStudio.Toolkit](https://github.com/VsixCommunity)
插件新开发版本使用了这个库,比起使用原生的VSSDK开发,要简便很多.vsct文件有提示,直接和类关联了,修改xml时,类也对应改变.代码大幅简化了,很大程度的简化了插件开发.
## razor编译和其它工具
1. js,css,html的mini压缩输出依赖[NUglify](https://github.com/xoofx/NUglify)这个工具  
2. sass,stylus,less编译依赖[WebCompiler](https://github.com/madskristensen/WebCompiler)插件  
3. 编译razor使用了项目[RazorService](https://github.com/mirrortom/RazorService),以前使用的是RazorEngine但是没有找到.netcore版本.这个库使用自带的Razor库实现了一些简单的功能.
## 安装使用
1. 需要安装2个项目,插件和VSIXService服务.
2. 4个命令的快捷键,右件菜单,还加入在"扩展"->"PublishWeb"下.
   1. 发布编辑文件Alt+QQ
   2. 发布选中文件Alt+11
   3. 发布选中文件夹Alt+22
   4. 发布整个项目Alt+33
3. 首次运行时,会生成配置文件publish.josn,在项目根目录下,但可能没有包含到项目,需要显示项目所有文件查看.
## 项目说明
1. 有2个项目,WebFilePublishVSIX(插件项目)和VSIXService(windows服务),插件和服务通过socket通信.这类似客户端和服务端结构的程序.
2. 插件项目负责和VS交互,比如获取项目目录和文件信息,生成命令菜单等.服务项目是一个本地的windows服务,主要功能是实现命令任务,比如发布文件,编译razor文件,压缩js/css/html这些工作.
## 程序流程
1. 点击命令后,实例化Publisher.cs类,加载配置文件.
   1. 项目首次运行命令时,会生成配置文件,在根目录下.运行后会发布到项目根目录下的dist目录.通过修改配置文件,规定发布要求.
2. 查找当前活动的文件/目录/项目,根据配置文件设置和文件扩展名,生成请求数据.
   1. 根据命令类型,选择一个执行目标,比如"发布文件"命令,会寻找当前选中的文件.
   2. 根据配置文件,检查这个文件是否可以发布.例如:这个文件被加入了配置文件禁止发布,那么就不会发布这个文件.
   3. 计算文件发布路径,优先使用配置的发布路径.没有配置时,使用默认规则:例如"根目录下的/html/index.cshtml",会发布到"发布目录/html/index.html".简单来说,发布文件就是将文件从项目根目录copy到了发布目录,文件从根目录开始的目录结构不变化.
   4. 将文件路径和发布路径,还有其它参数,例如是否压缩等,打包成数据包.
3. 将请求数据发送到服务,服务执行任务后返回结果
   1. 与服务项目建立socket连接,将数据包发送到服务.
   2. 服务根据数据参数,分析要执行的任务.例如:razor文件会编译成html文件,css/js等文件会压缩,然后将处理后的文件输出到发布目录.
   3. 生成一个处理结果,返回给插件项目.
3. 在vs输出界面显示生成结果.
   1. 收到结果后,断开socket连接,将处理结果显示在vs"输出"窗口.
   2. 到此,文件发布结束.文件已经处理,生成在发布目录下了.
#### VSIX插件项目主要类
1. Publish/Publisher.cs 命令类,使用这个类实现命令.
2. Publish/CmdContext.cs 命令运行时的上下文数据类.
3. Publish/ConfigM.cs 配置文件的实体类
4. Commands目录下的类是4种命令实现,采用了Toolkit插件工具库提供的简洁接口,比起原生的接口,去掉了一些繁琐的代码,只需要写具体命令执行代码就可以了.
#### VSIXService Win服务项目
1. 使用.Net通用主机,承载一个socket监听服务程序.
2. 接受来自插件程序的请求,实现razor编译,css/js/html压缩等任务.
3. 每个功能是一个命令类,实现IFun接口,通过命令编号调用.
4. 主要类:
   1. Worker.cs连接对象,每个socket连接会建立一个对象.
5. 2. WorkContent.cs连接上下文数据对象
   3. MainService.cs监听服务
   4. Commander.cs命令注册管理
   5. cmds目录,含有所有命令类.
6. 通信约定:
   1. 请求数据:第1个字节是命令id,后面是参数,每个命令的参数形式自定义.
   2. 回应数据:第1个字节是命令是否成功,后面是数据或者结果信息.