using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebFilePublishVSIX
{
    /// <summary>
    /// 用于判断指定KEY所代表的cshtml文件是否需要被Razor.Compile重新编译
    /// 逻辑:使用文件内容的md5值判断
    /// razor引擎每次编译时,记下这个cshtml文件的hash值和key,(cshtml文件的key是固定的,取值于其项目根目录起的相对路径)
    /// 每次编译时会先判断是否需要编译,通过传来key和文件内容.据此计算并比较上一次的hash值.
    /// </summary>
    static class RazorCacheHelpers
    {
        /// <summary>
        /// 存放key和上一次编译时的hash值
        /// </summary>
        private static readonly Dictionary<string, string> key_md5 = new Dictionary<string, string>();
        /// <summary>
        /// 比较指定key的文件,修改过返回true.
        /// 如果字典中无此key,则添加到字典中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cshtml"></param>
        public static bool IsChange(string key,string cshtml)
        {
            // 计算当前传入的cshtml的Hash值
            string currMd5 = FileHelpers.StringMd5(cshtml);

            // 如果字典中没有这个key,说明没编译过,将其添加到字典.返回true
            if (!key_md5.ContainsKey(key))
            {
                key_md5.Add(key, currMd5);
                return true;
            }
            //
            bool isChg= key_md5[key] != currMd5;
            // 更新为此次hash值
            key_md5[key] = currMd5;
            //
            return isChg;
        }
    }
}
