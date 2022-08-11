using System.IO;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;

public class PostProcessIOS : MonoBehaviour
{
    [PostProcessBuildAttribute(45)]//must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
    private static void PostProcessBuild_iOS(BuildTarget target, string buildPath)
    {
        string podInfo = "\ntarget 'Unity-iPhone' do\n\tpod 'UnityAds','4.2.1'\n\tpod 'KSAdSDK', '3.3.27'\n\tpod 'smaato-ios-sdk', '21.7.6'\n\tpod 'OgurySdk', '2.1.0'\n\tpod 'BaiduMobAdSDK','4.881'\n\tpod 'HyBid','2.14.0'\nend";
        if (target == BuildTarget.iOS && podInfo.Length > 0)
        {

            using (StreamWriter sw = File.AppendText(buildPath + "/Podfile"))
            {
                //in this example I'm adding an app extension
                sw.WriteLine(podInfo);
            }
        }

        //如需要在 info.plist 中添加相关配置可打开以下注释
        //AddSetting(buildPath);

        //快手SDK不支持 bitCode 入使用需要关闭 bitcode
        //CloseBitCode(buildPath);
    }

    private static void AddSetting(string buildPath)
    {
        string infoPlistPath = Path.Combine(buildPath, "./Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(infoPlistPath));
        PlistElementDict rootDict = plist.root;

        //http请求权限
        PlistElementDict transportSecurity  = rootDict.CreateDict("NSAppTransportSecurity");
        transportSecurity.SetBoolean("NSAllowsArbitraryLoads",true);

        //google广告配置
        rootDict.SetString("GADApplicationIdentifier", "ca-app-pub-4737436342233455~9722214995");

        //IDFA权限请求设置
        rootDict.SetString("NSUserTrackingUsageDescription", "点击\"允许\"以使用设备信息获得更加相关的广告内容，未经同意我们不会用于其他目的；开启后，您也可以前往系统\"设置-隐私\"中随时关闭");

        File.WriteAllText(infoPlistPath, plist.WriteToString());
    }

    private static void CloseBitCode(string buildPath)
    {
        string projectPath = Path.Combine(buildPath, "./Unity-iPhone.xcodeproj/project.pbxproj");

        PBXProject pbxProject = new PBXProject();
        pbxProject.ReadFromFile(projectPath);

        string target = pbxProject.GetUnityMainTargetGuid();
        pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

        target = pbxProject.GetUnityFrameworkTargetGuid();
        pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

        pbxProject.WriteToFile(projectPath);
    }
}