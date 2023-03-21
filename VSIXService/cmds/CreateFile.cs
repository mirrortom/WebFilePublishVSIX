using System.Text.RegularExpressions;
using VSIXService.Helpers;

namespace VSIXService.cmds;

internal class CreateFile : IFun
{
    public int Id => 1;

    public string Desc => "新建文件.json参数:{path:\"新文件全路径名字(必须)\",code:\"代码片段名\",replace:{替换值对象}}";

    public void Run(WorkContent content)
    {
        if (string.IsNullOrWhiteSpace(content.ParaString))
            throw new VSIXServiceException("参数缺少!");
        //
        content.ParaDynamic = Helps.JSONToDynamic(content.ParaString);
        string path = content.ParaDynamic.path;
        if (string.IsNullOrWhiteSpace(path))
            throw new VSIXServiceException("path参数缺少!");
        path = path.Trim();
        if (!Path.IsPathFullyQualified(path))
            throw new VSIXServiceException("path参数错误,必须是绝对路径!");

        // 含有片段类型
        if (content.ParaDynamic.ContainsKey("code"))
        {
            Helps.CreateFile(path, GetCodeSnippet(content));
            content.ResultCode = 1;
            return;
        }

        // 无片段类型,是空文件
        Helps.CreateFile(path);
        content.ResultCode = 1;
    }

    /// <summary>
    /// 新建文件时,插入代码片段
    /// </summary>
    private string GetCodeSnippet(WorkContent content)
    {
        string codeName = content.ParaDynamic.code;
        string path = $"{Config.snippetDir}/{codeName.Trim()}.txt";
        // 片段文件不存在时,返回的是空文件
        if (!File.Exists(path))
            return null;
        // 取出片段
        string codeSnippet = File.ReadAllText(path);
        // 检查替换占位符 ${val}
        Regex replaceTagsRege = new(@"\$\{[a-zA-Z0-9-_]+?\}");
        var tagsMatchs = replaceTagsRege.Matches(codeSnippet);
        // 没有要替换的,返回
        int count = tagsMatchs.Count;
        if (count == 0)
        {
            return codeSnippet;
        }
        // 检查替换值
        dynamic replace = content.ParaDynamic.replace;
        if (replace == null)
            return codeSnippet;
        // 替换,替换值在参数中
        for (int i = 0; i < count; i++)
        {
            var matchval = tagsMatchs[i].Value;
            var key = matchval.Replace("${", "").Replace("}", "");
            // 如果提供了替换值,才替换
            if (replace.ContainsKey(key))
            {
                codeSnippet = codeSnippet.Replace(matchval, replace[key].ToString().Trim());
            }
        }
        return codeSnippet;
    }
}