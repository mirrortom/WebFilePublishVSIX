using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace WebFilePublishVSIX
{
    /// <summary>
    /// 获取项目内资源
    /// </summary>
    public static class ProjectHelpers
    {
        private static DTE2 _dte = EnvVar._dte;

        /// <summary>
        /// vs当前活动文档
        /// </summary>
        /// <returns></returns>
        public static Document GetActiveDoc()
        {
            return _dte.ActiveDocument;
        }

        /// <summary>
        /// 获取当前项目中的所有ProjectItem的路径(它不含bin下的内容,含项目源文件).
        /// 用于发布项目中的文件时,获取源文件路径
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static List<string> GetItems(this Project project)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            List<string> files = new List<string>();
            void getItems(ProjectItems items)
            {
                foreach (ProjectItem item in items)
                {
                    files.Add(item.Properties.Item("FullPath").Value.ToString());
                    if (item.ProjectItems.Count > 0)
                    {
                        getItems(item.ProjectItems);
                    }
                }
            }
            getItems(project.ProjectItems);
            return files;
        }

        /// <summary>
        /// 当前项目选中的那个文件/目录(项目资源管理窗口中)
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ProjectItem> GetSelectedItems()
        {
            var items = (Array)_dte.ToolWindows.SolutionExplorer.SelectedItems;

            foreach (UIHierarchyItem selItem in items)
            {
                ProjectItem item = selItem.Object as ProjectItem;

                if (item != null)
                    yield return item;
            }
        }

        /// <summary>
        /// 当前项目选中的那个文件/目录的全(物理)路径 (项目资源管理窗口中)
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetSelectedItemPaths()
        {
            foreach (ProjectItem item in GetSelectedItems())
            {
                if (item != null && item.Properties != null)
                    yield return item.Properties.Item("FullPath").Value.ToString();
            }
        }

        /// <summary>
        /// 获取项目根目录路径.返回的目录路径最后不带/或\,且都是/(正斜杠)
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static string GetRootFolder(this Project project)
        {
            if (project == null || string.IsNullOrEmpty(project.FullName))
                return null;

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    // MFC projects don't have FullPath, and there seems to be no way to query existence
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    // Installer projects have a ProjectPath.
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName).Replace('\\', '/') : null;

            if (Directory.Exists(fullPath))
                return fullPath.Replace('\\', '/').TrimEnd('/');

            if (File.Exists(fullPath))
                return Path.GetDirectoryName(fullPath).Replace('\\', '/');

            return null;
        }

        /// <summary>
        /// 当前活动的项目(项目资源管理窗口中)
        /// </summary>
        /// <returns></returns>
        public static Project GetActiveProject()
        {
            try
            {
                Window2 window = _dte.ActiveWindow as Window2;
                Document doc = _dte.ActiveDocument;

                if (window != null && window.Type == vsWindowType.vsWindowTypeDocument)
                {
                    // if a document is active, use the document's containing directory
                    if (doc != null && !string.IsNullOrEmpty(doc.FullName))
                    {
                        ProjectItem docItem = _dte.Solution.FindProjectItem(doc.FullName);

                        if (docItem != null && docItem.ContainingProject != null)
                            return docItem.ContainingProject;
                    }
                }

                Array activeSolutionProjects = _dte.ActiveSolutionProjects as Array;

                if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
                    return activeSolutionProjects.GetValue(0) as Project;

                if (doc != null && !string.IsNullOrEmpty(doc.FullName))
                {
                    var item = _dte.Solution?.FindProjectItem(doc.FullName);

                    if (item != null)
                        return item.ContainingProject;
                }
            }
            catch (Exception ex)
            {
                //Logger.Log("Error getting the active project" + ex);
                OutPutInfo.Error(ex);
            }

            return null;
        }
    }
}
