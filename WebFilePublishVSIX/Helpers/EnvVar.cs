using EnvDTE80;

namespace WebFilePublishVSIX
{
    static class EnvVar
    {
        /// <summary>
        /// 服务是两个 Vspackage 之间的协定
        /// doc:https://docs.microsoft.com/zh-cn/visualstudio/extensibility/using-and-providing-services?view=vs-2022
        /// </summary>
        public static DTE2 _dte;
        /// <summary>
        /// 插件名字.
        /// </summary>
        public const string Name = "web发布插件";
        /// <summary>
        /// 发布配置文件名字.这个文件放在项目根目录下
        /// </summary>
        public const string PublishCfgName = "publish.json";
        /// <summary>
        /// 当前要发布的项目的根路径.例如"e:/vs2017/myproject"(正斜杠,后面无/)
        /// </summary>
        public static string ProjectDir;
        
    }
}
