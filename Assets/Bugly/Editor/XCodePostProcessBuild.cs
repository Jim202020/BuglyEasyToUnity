#if UNITY_IOS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public static class XCodePostProcessBuild
{
    [PostProcessBuild(int.MaxValue)]
	public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
	{
        if (target != BuildTarget.iOS)
		{
			Debug.LogWarning("Target is not iOS. XCodePostProcessBuild will not run");
			return;
		}

        string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var pbxProject = new PBXProject();
        pbxProject.ReadFromString(File.ReadAllText(projectPath));
        
#if UNITY_2019_3_OR_NEWER
        string mainTarget = pbxProject.GetUnityMainTargetGuid();  
        string frameworkTarget = pbxProject.GetUnityFrameworkTargetGuid();   
#else
        string mainTarget = pbxProject.TargetGuidByName(PBXProject.GetUnityTargetName());
        string frameworkTarget = mainTarget;
#endif

        DisableBitcode(pbxProject,mainTarget,frameworkTarget);

        pbxProject.SetBuildProperty(frameworkTarget, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
		pbxProject.AddBuildProperty(frameworkTarget, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks");

        SetBugly(frameworkTarget,pbxProject,pathToBuiltProject);

        pbxProject.WriteToFile (projectPath);
    }

    static void DisableBitcode(PBXProject pbxProject,string mainTarget,string frameworkTarget)
    {
        pbxProject.SetBuildProperty(mainTarget, "ENABLE_BITCODE", "NO");
        pbxProject.SetBuildProperty(frameworkTarget, "ENABLE_BITCODE", "NO");
    }

    static void SetBugly(string frameworkTarget,PBXProject pbxProject, string pathToBuiltProject)
    {
        var buglyPath = "Bugly";
        AddDirectory(pbxProject, pathToBuiltProject, $"{buglyPath}/Plugins/BuglyPlugins/iOS", "Bugly", null);
        pbxProject.AddFileToBuild(frameworkTarget, pbxProject.AddFile("Bugly/Bugly.framework", "Bugly/Bugly.framework", PBXSourceTree.Source));
        pbxProject.AddFileToBuild(frameworkTarget, pbxProject.AddFile("Bugly/BuglyBridge/libBuglyBridge.a", "Bugly/libBuglyBridge.a", PBXSourceTree.Source));
        pbxProject.AddFileToBuild(frameworkTarget, pbxProject.AddFile("Bugly/BuglyBridge/BuglyBridge.h", "Bugly/BuglyBridge.h", PBXSourceTree.Source));

        pbxProject.AddFileToBuild(frameworkTarget,pbxProject.AddFile("usr/lib/libz.dylib", "Bugly/libz.dylib", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(frameworkTarget,pbxProject.AddFile("usr/lib/libc++.dylib", "Bugly/libc++.dylib", PBXSourceTree.Sdk));

        pbxProject.AddFrameworkToProject(frameworkTarget, "SystemConfiguration.framework", false);
        pbxProject.AddFrameworkToProject(frameworkTarget, "Security.framework", false);
        pbxProject.AddFrameworkToProject(frameworkTarget, "JavaScriptCore.framework", true);

        pbxProject.AddBuildProperty(frameworkTarget, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Bugly");
        pbxProject.AddBuildProperty(frameworkTarget, "LIBRARY_SEARCH_PATHS", "$(SRCROOT)/Bugly/BuglyBridge");
    }

    public static void AddDirectory(PBXProject project, string pathToBuiltProject, string assetPath, 
    string xcodePath, Action<string> callback,bool recursiveDir = false,bool curDirFiles = false)
    {
        var path = Path.Combine(Application.dataPath, assetPath);
        var targetPath = Path.Combine(pathToBuiltProject, xcodePath);
        CopyDirectory(path, targetPath);
        var info = new DirectoryInfo(targetPath);

        if(recursiveDir)
        {
            var directories = info.GetDirectories();
            foreach (var dirInfo in directories)
            {
                string fileGuid = project.AddFile(xcodePath + "/" + dirInfo.Name, xcodePath + "/" + dirInfo.Name, PBXSourceTree.Source);

                if (callback != null)
                {
                    callback(fileGuid);
                }
            }
        }

        if (curDirFiles)
        {
            var filesList = info.GetFiles();
            foreach (var fileInfo in filesList)
            {
                string fileGuid = project.AddFile(xcodePath + "/" + fileInfo.Name, xcodePath + "/" + fileInfo.Name, PBXSourceTree.Source);

                if (callback != null)
                {
                    callback(fileGuid);
                }
            }
        }
        
    }

	public static void CopyDirectory(string sourcePath, string destinationPath)
	{
		if (destinationPath.EndsWith(".meta") || destinationPath.EndsWith(".DS_Store"))
			return;
		
		DirectoryInfo info = new DirectoryInfo(sourcePath);
		Directory.CreateDirectory(destinationPath);
		foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
		{
			string destName = Path.Combine(destinationPath, fsi.Name);

			if (destName.EndsWith(".meta") || destName.EndsWith(".DS_Store"))
				continue;

			if (fsi is System.IO.FileInfo)          //如果是文件，复制文件
				File.Copy(fsi.FullName, destName);
			else                                    //如果是文件夹，新建文件夹，递归
			{
				Directory.CreateDirectory(destName);
				CopyDirectory(fsi.FullName, destName);
			}
		}
	}
}
#endif