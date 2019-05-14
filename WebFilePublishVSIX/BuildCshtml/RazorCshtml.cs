using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebFilePublishVSIX
{
    /*==========================================================================================================
        Mirror始建于2018年6月16日 16:53:59

        1.类功能:使用RazorEngine开源库编译cshtml页面,得到静态页.
        2.设计功能:使用cshtml页面模块化编写HTML文件,语法习惯与razor相同.暂不支持@model数据,因为主要目标是模块化html.之前使用字符串替换的方式,实现过一个版本,模块化可以实现,但功能弱,只能简单替换字符串变量,不能运行代码.现在这版使用真正的Razor页面,利用RazorEngine编译,可以使用Razor页面的多种基本功能.
        3.功能示例:一个a.cshtml页面如下:
        @{
            // 使用母板页.(母板页只能引用一个,并且写在页面开始处)
            Layout="/src/layout/layout.cshtml";
        }
        <div>
            // 引入了一个分部页.(可以引用多个)
            @Include("/src/a_1.part.cshtml"); 
            // 递归的 : a_1.part.cshtml中也支持再引用layout和Include.
        </div>
        <table>
            // razor语法.和asp.net razor的一样.在Cshtml页面中使用c#语言生成html
            @{
                List<string> names=new List<string>();
            }
            @foreach (string item in names)
            {
                <tr><td>item</td></tr>
            }
        </table>
        4.编译过程:
            找出a.cshtml引用的母板页和所有分部页,并且递归找出其分部页所引用的母板和分部页.
            使用RazorEngine编译,得到最后的html页面.
        5.母板页和分部页路径规则:
            使用项目根路径开始的相对路径.
            例如:Layout="/src/layout/layout.cshtml"
                @Include("/src/home/home1.part.cshtml")
        6.文件名约定
          母版页名字约定后缀:layout.cshtml,片段页约定:part.cshtml 例如:
          demo_layout.cshtml 
          head_part.cshtml
          foot.part.cshtml
        7.[razorengine参考文档]https://antaris.github.io/RazorEngine/index.html
    ============================================================================================================*/

    /// <summary>
    /// razor编译CSHTML
    /// </summary>
    public static class RazorCshtml
    {
        /// <summary>
        /// 引擎配置
        /// </summary>
        private static TemplateServiceConfiguration config;
        private static IRazorEngineService serveice;

        /// <summary>
        /// 静态构造时初始化引擎配置
        /// </summary>
        static RazorCshtml()
        {
            config = new TemplateServiceConfiguration();
            // 使用RoslynCompiler编译,不然运行插件运行时会卡死
            config.CompilerServiceFactory = new RazorEngine.Roslyn.RoslynCompilerServiceFactory();
            config.CompilerServiceFactory.CreateCompilerService(Language.CSharp);
            config.CachingProvider = new DefaultCachingProvider(t => { });
            config.Language = Language.CSharp;
            config.AllowMissingPropertiesOnDynamic = true;
            config.DisableTempFileLocking = true;
            // 静态构造只执行一次,这里初始化引擎服务.然后一直使用这个服务,并不释放它.因为服务建立耗时较长,大概1秒多.之前在using里编译完就释放,每次都要初始化,结果太慢.在控制台程序里并不慢,但一放在插件里,就不行.故这里初始化之后,不再释放.
            serveice = RazorEngineService.Create(config);
        }
        /// <summary>
        /// 编译cshtml后返回静态html文件内容.元组1位表示成功,2位成功时html内容,失败时出错信息
        /// </summary>
        /// <param name="cshtmlPath">cshtml文件全路径</param>
        /// <returns></returns>
        public static Tuple<bool, string> BuildCshtml(string cshtmlPath)
        {
            try
            {
                Dictionary<string, string> allTemps = FindTemps(cshtmlPath);
                //
                return new Tuple<bool, string>(true, RazorRun(allTemps, cshtmlPath));
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"编译cshtml时发生异常:{Environment.NewLine}cshtml文件地址:{cshtmlPath}{Environment.NewLine}详情{e.ToString()}{Environment.NewLine}");
            }
        }

        /// <summary>
        /// 使用RazorEngine编译cshtml页面.返回静态内容
        /// </summary>
        /// <param name="temps">temps是一个以路径为键,cshtml文件内容为值的字典.它包含了一个要被编译的cshtml主文件以及其引用的0~多个母板页和片段页.</param>
        /// <param name="mainCshtmlKey">要编译的主cshtml模板文件在temps中的key</param>
        /// <returns></returns>
        private static string RazorRun(Dictionary<string, string> temps, string mainCshtmlKey)
        {
            // 添加要编译的cshtml文件,包括其引用的母板页和片段页
            foreach (string key in temps.Keys)
            {
                // 如果文件有变化才重新添加模板和编译,因为编译时间比较长
                if (!RazorCacheHelpers.IsChange(key, temps[key]))
                    continue;

                // 键是文件的相对路径名,是固定的,每个cshtml的路径是唯一的.
                ITemplateKey k = serveice.GetKey(key);

                // 先删除这个键,再添加之.由于键固定,不删除的话,会报键重复.
                ((DelegateTemplateManager)config.TemplateManager).RemoveDynamic(k);

                // 添加模板,以键为模板名,以cshtml内容为模板内容
                serveice.AddTemplate(k, temps[key]);

                // 编译之.由于键固定,如果不编译的话,就会使用上一次编译后的缓存(与这固定键对应的缓存).所以添加模板后,一定要重新编译.
                serveice.Compile(k);
            }
            // MainCshtmlKey是cshtml主页面,生成html时,必须以主cshtml模板的名.
            return serveice.Run(serveice.GetKey(mainCshtmlKey));
        }

        /// <summary>
        /// 根据cshtml文件相对路径,返回其在项目中的全路径
        /// </summary>
        /// <param name="relPath">cshtml相对路径</param>
        private static string FullPath(string relPath)
        {

            string fullPath = Path.Combine(EnvVar.ProjectDir,
                            (relPath.StartsWith("/") || relPath.StartsWith("\\"))
                            ? relPath.Substring(1)
                            : relPath).Replace('\\', '/');

            if (File.Exists(fullPath))
                return fullPath;

            throw new Exception($"cshtml编译失败,找不到cshtml文件:{relPath}");
        }


        /// <summary>
        /// 找出指定路径的CSHMTL页面中的引用的母板layout和片段Include.(递归,如果模板片段中也引用了片段,亦将找出.)
        /// 返回字典:键为cshtml页面相对路径,值为cshtml页面内容.(cshtml主模板的键是全路径)
        /// </summary>
        /// <param name="masterCshtmlPath">cshtml主模板全路径</param>
        /// <returns></returns>
        private static Dictionary<string, string> FindTemps(string masterCshtmlPath)
        {
            Dictionary<string, string> temps = new Dictionary<string, string>();
            // 添加cshtml主模板,这里直接使用全路径做键,不影响逻辑.
            string masterCshtml = File.ReadAllText(masterCshtmlPath);
            temps.Add(masterCshtmlPath, masterCshtml);

            // 递归查找其引用的母板页以及部分页
            void findTemps(string cshtmlCont)
            {
                // 匹配母板和片段页正则
                Regex layoutRege = new Regex("Layout\\s*=\\s*@?\"(\\S+)\";");
                Regex partialRege = new Regex("@Include\\(@?\"(\\S+)\"\\)");

                // 匹配layout(只匹配第1个Layout)
                var layoutMatch = layoutRege.Match(cshtmlCont);
                // 匹配部分页(所有的)
                var partialMatches = partialRege.Matches(cshtmlCont);

                // 模板地址键值对结果值
                Dictionary<string, string> tempDict = new Dictionary<string, string>();

                // 添加layout模板,如果找到
                string layoutPath = layoutMatch?.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(layoutPath))
                    tempDict.Add(layoutPath, File.ReadAllText(FullPath(layoutPath)));

                // 添加片段模板,如果找到
                foreach (Match mpartpath in partialMatches)
                {
                    string partialPath = mpartpath.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(partialPath))
                        tempDict.Add(partialPath, File.ReadAllText(FullPath(partialPath)));
                }
                //
                if (tempDict.Count == 0) return;
                foreach (string key in tempDict.Keys)
                {
                    // 找到的模板保存到列表.如果有相同路径的模板,不再添加.视为同一个.
                    if (!temps.ContainsKey(key))
                        temps.Add(key, tempDict[key]);
                    findTemps(tempDict[key]);
                }
            }
            //
            findTemps(masterCshtml);
            return temps;
        }

    }
}
