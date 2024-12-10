#if UNITY_IOS
using System.IO;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.Collections;
using UnityEditor.iOS.Xcode.Extensions;

public class PostProcessIOS : MonoBehaviour
{
    [PostProcessBuildAttribute(45)]//must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
    private static void PostProcessBuild_iOS(BuildTarget target, string buildPath)
    {
        string podInfo = "";
        if (target == BuildTarget.iOS && podInfo.Length > 0)
        {
            string path = buildPath + "/Podfile";
            string Podfile = File.ReadAllText(path);
            string keyStr = "target 'Unity-iPhone' do";
            if (Podfile.Contains(keyStr))
            {
                Podfile = Podfile.Replace(keyStr, podInfo);
            }
            else
            {
                Podfile += podInfo + "end";
            }
            File.WriteAllText(path,Podfile);
        }

        //以下功能可根据实际情况选择使用
        
        //info.plist 中添加相关配置  http请求权限，google广告配置，IDFA权限请求设置
        AddSetting(buildPath);

        //关闭 bitcode 快手SDK不支持 
        CloseBitCode(buildPath);
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

    //pod install 后对特殊源进行配置修改
    [PostProcessBuildAttribute(55)]
    private static void PostConfigureProcessBuild_iOS(BuildTarget target, string buildPath)
    {
        string debug = buildPath + "/Pods/Target Support Files/Pods-Unity-iPhone/Pods-Unity-iPhone.debug.xcconfig";
        string release = buildPath + "/Pods/Target Support Files/Pods-Unity-iPhone/Pods-Unity-iPhone.release.xcconfig";
        string releaseforprofiling = buildPath + "/Pods/Target Support Files/Pods-Unity-iPhone/Pods-Unity-iPhone.releaseforprofiling.xcconfig";
        string releaseforrunning = buildPath + "/Pods/Target Support Files/Pods-Unity-iPhone/Pods-Unity-iPhone.releaseforrunning.xcconfig";
        string[] pathArray = new string[] { debug, release, releaseforprofiling, releaseforrunning };

        //KSAdSDK 快手 动态库无法直接添加在UnityFramework v3.3.27
        string KSAdSDKPath = buildPath + "/Pods/Target Support Files/KSAdSDK/";
        DirectoryInfo pathDir = new DirectoryInfo(KSAdSDKPath);
        //确认pod是否有快手
        if (pathDir.Exists)
        {
            string[] sdkPathArray = new string[] {
                "\"${PODS_ROOT}/KSAdSDK\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/KSAdSDK\""};

            string[] frameworkArray = new string[] {
                "-framework \"KSAdSDK\""};
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }

        //百度 其资源包需要添加在主工程目录下 v4.881
        string BuaiduPath = buildPath + "/Pods/Target Support Files/BaiduMobAdSDK/";
        pathDir = new DirectoryInfo(BuaiduPath);
        //确认pod是否有百度
        if (pathDir.Exists)
        {
            string[] sdkPathArray = new string[] {
                "\"${PODS_ROOT}/BaiduMobAdSDK\""};

            string[] frameworkArray = new string[] {
                "-framework \"BaiduMobAdSDK\""};

            string[] sysFrameworkArray = new string[] {
                "CoreLocation.framework"};
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray, sysFrameworkArray);
        }

        //smaato
        string SmattoPath = buildPath + "/Pods/Target Support Files/smaato-ios-sdk/";
        pathDir = new DirectoryInfo(SmattoPath);
        //确认pod是否有smaato
        if (pathDir.Exists)
        {
            string[] sdkPathArray = new string[] {
                "\"${PODS_ROOT}/smaato-ios-sdk\"",
                "\"${PODS_ROOT}/smaato-ios-sdk/vendor\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/smaato-ios-sdk/Modules/Banner\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/smaato-ios-sdk/Modules/Core\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/smaato-ios-sdk/Modules/Interstitial\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/smaato-ios-sdk/Modules/Native\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/smaato-ios-sdk/Modules/OpenMeasurement\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/smaato-ios-sdk/Modules/Outstream\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/smaato-ios-sdk/Modules/RewardedAds\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/smaato-ios-sdk/Modules/RichMedia\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/smaato-ios-sdk/Modules/Video\"",
            };

            string[] frameworkArray = new string[] {
                "-framework \"OMSDK_Smaato\"",
                "-framework \"SmaatoSDKBanner\"",
                "-framework \"SmaatoSDKCore\"",
                "-framework \"SmaatoSDKInterstitial\"",
                "-framework \"SmaatoSDKNative\"",
                "-framework \"SmaatoSDKOpenMeasurement\"",
                "-framework \"SmaatoSDKOutstream\"",
                "-framework \"SmaatoSDKRewardedAds\"",
                "-framework \"SmaatoSDKRichMedia\"",
                "-framework \"SmaatoSDKVideo\"",
            };

            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }

        //Verve v2.14.0
        string VerveSDKPath = buildPath + "/Pods/Target Support Files/HyBid/";
        pathDir = new DirectoryInfo(VerveSDKPath);
        //确认pod是否有Verve
        if (pathDir.Exists)
        {
            string omsdkStr = finidString(pathArray,"${PODS_ROOT}/HyBid/PubnativeLite/PubnativeLite/OMSDK-","\"");
            string[] sdkPathArray;
            if (omsdkStr != null)
            {
                omsdkStr = "\"" + omsdkStr + "\"";
                sdkPathArray = new string[] {
                    omsdkStr,
                    "\"${PODS_CONFIGURATION_BUILD_DIR}/HyBid\"",
                    "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/HyBid/Core\""
                };
            }
            else
            {
                sdkPathArray = new string[] {
                    "\"${PODS_CONFIGURATION_BUILD_DIR}/HyBid\"",
                    "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/HyBid/Core\""
                };
            }
            

            string[] frameworkArray = new string[] {
                "-framework \"HyBid\"",
                "-framework \"OMSDK_Pubnativenet\"",
            };
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }

        //Fyber v8.2.0
        string FyberSDKPath = buildPath + "/Pods/Target Support Files/Fyber_Marketplace_SDK/";
        pathDir = new DirectoryInfo(FyberSDKPath);
        //确认pod是否有Fyber
        if (pathDir.Exists)
        {
            string[] sdkPathArray = new string[] {
                "\"${PODS_ROOT}/Fyber_Marketplace_SDK/IASDKCore\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/Fyber_Marketplace_SDK\""};

            string[] frameworkArray = new string[] {
                "-framework \"IASDKCore\""};
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }

        //Ogury v2.1.0
        string OgurySDKPath = buildPath + "/Pods/Target Support Files/OgurySdk/";
        pathDir = new DirectoryInfo(OgurySDKPath);
        //确认pod是否有Ogury
        if (pathDir.Exists)
        {
            ArrayList sdkPathList = new ArrayList();
            sdkPathList.Add("\"${PODS_ROOT}/OguryAds\"");
            sdkPathList.Add("\"${PODS_ROOT}/OguryCore\"");
            sdkPathList.Add("\"${PODS_ROOT}/OgurySdk\"");
            sdkPathList.Add("\"${PODS_XCFRAMEWORKS_BUILD_DIR}/OguryAds\"");
            sdkPathList.Add("\"${PODS_XCFRAMEWORKS_BUILD_DIR}/OguryAds/OMID\"");
            sdkPathList.Add("\"${PODS_XCFRAMEWORKS_BUILD_DIR}/OguryCore\"");
            sdkPathList.Add("\"${PODS_XCFRAMEWORKS_BUILD_DIR}/OgurySdk\"");

            ArrayList frameworkList = new ArrayList();
            frameworkList.Add("-framework \"OMSDK_Ogury\"");
            frameworkList.Add("-framework \"OguryAds\"");
            frameworkList.Add("-framework \"OguryCore\"");
            frameworkList.Add("-framework \"OgurySdk\"");
            //v5.0.0+ Ogury 不包含 OguryChoiceManager.framework
            string checkPath = buildPath + "/Pods/Target Support Files/OguryChoiceManager/";
            DirectoryInfo checkInfo = new DirectoryInfo(checkPath);
            if(checkInfo.Exists)
            {
                sdkPathList.Add("\"${PODS_ROOT}/OguryChoiceManager\"");
                sdkPathList.Add("\"${PODS_XCFRAMEWORKS_BUILD_DIR}/OguryChoiceManager\"");
                frameworkList.Add("-framework \"OguryChoiceManager\"");
            }

            string[] sdkPathArray = (string[])sdkPathList.ToArray(typeof(string));
            string[] frameworkArray = (string[])frameworkList.ToArray(typeof(string));

            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }

        //Bigo
        string BigoSDKPath = buildPath + "/Pods/Target Support Files/BigoADS/";
        pathDir = new DirectoryInfo(BigoSDKPath);
        //确认pod是否有Bigo
        if (pathDir.Exists)
        {
            string podStr = finidString(pathArray, "${PODS_XCFRAMEWORKS_BUILD_DIR}/BigoADS", "\"");
            if(podStr != null)
            {
                string[] sdkPathArray = new string[] {
                    "\"${PODS_ROOT}/BigoADS/BigoADS\"",
                    "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/BigoADS\"",
                };

                string[] frameworkArray = new string[] {
                    "-framework \"BigoADS\"",
                    "-framework \"OMSDK_Bigosg\"",
                };
                removeSetting(pathArray, sdkPathArray, frameworkArray);
                AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
            }
        }

        //Start.io
        string StartIOSDKPath = buildPath + "/Pods/Target Support Files/StartAppSDK/";
        pathDir = new DirectoryInfo(StartIOSDKPath);
        //确认pod是否有Start.io
        if (pathDir.Exists)
        {
            string[] sdkPathArray = new string[] {
                "\"${PODS_ROOT}/StartAppSDK\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/StartAppSDK\"",
            };

            string[] frameworkArray = new string[] {
                "-framework \"StartApp\""
            };
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }
        //Maio-v2
        string MaioDKPath = buildPath + "/Pods/Target Support Files/MaioSDK-v2/";
        pathDir = new DirectoryInfo(MaioDKPath);
        //确认pod是否有Maio-v2
        if (pathDir.Exists)
        {
            string[] sdkPathArray = new string[] {
                "\"${PODS_ROOT}/MaioSDK-v2\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/MaioSDK-v2\"",
            };

            string[] frameworkArray = new string[] {
                "-framework \"Maio\""
            };
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }
        ///AmazonPublisherServicesSDK
        string AmazonSDKPath = buildPath + "/Pods/Target Support Files/AmazonPublisherServicesSDK/";
        pathDir = new DirectoryInfo(AmazonSDKPath);
        //确认pod是否有Amazon
        if (pathDir.Exists)
        {
            string podStr = finidString(pathArray, "${PODS_ROOT}/AmazonPublisherServicesSDK/APS_iOS_SDK-", "\"");
            string[] sdkPathArray;
            if (podStr != null)
            {
                podStr = "\"" + podStr + "\"";
                sdkPathArray = new string[] {
                    podStr,
                    "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/AmazonPublisherServicesSDK\"",
                };
            }
            else
            {
                sdkPathArray  = new string[] {
                    "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/AmazonPublisherServicesSDK\"",
                };
            }

            string[] frameworkArray = new string[] {
                "-framework \"DTBiOSSDK\""
            };
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }
        ///Tapjoy 
        string TapjoySDKPath = buildPath + "/Pods/Target Support Files/TapjoySDK/";
        pathDir = new DirectoryInfo(TapjoySDKPath);
        //确认pod是否有Tapjoy
        if (pathDir.Exists)
        {
            string podStr = finidString(pathArray, "${PODS_ROOT}/TapjoySDK/TapjoySDK_iOS_", "\"");
            string[] sdkPathArray;
            if (podStr != null)
            {
                podStr = "\"" + podStr + "\"";
                sdkPathArray = new string[] {
                     podStr,
                     "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/TapjoySDK\"",
                 };
            }
            else
            {
                sdkPathArray = new string[] {
                     "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/TapjoySDK\"",
                 };
            }

            string[] frameworkArray = new string[] {
                 "-framework \"Tapjoy\""
             };
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }
        ///KwaiAdsSDK 
        string KwaiAdsSDKPath = buildPath + "/Pods/Target Support Files/TradPlusKwaiAdsSDK/";
        pathDir = new DirectoryInfo(KwaiAdsSDKPath);
        //确认pod是否有TradPlusKwaiAdsSDK
        if (pathDir.Exists)
        {
            string[] sdkPathArray = new string[] {
                "\"${PODS_ROOT}/TradPlusKwaiAdsSDK/KwaiAdsSDK\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/TradPlusKwaiAdsSDK\"",
            };

            string[] frameworkArray = new string[] {
                "-framework \"KwaiAdsSDK\""
            };
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }
        //AppLovin
        string AppLovinSDKPath = buildPath + "/Pods/Target Support Files/AppLovinSDK/";
        pathDir = new DirectoryInfo(AppLovinSDKPath);
        //确认pod是否有AppLovin
        if (pathDir.Exists)
        {
            string podStr = finidString(pathArray, "${PODS_ROOT}/AppLovinSDK/applovin-ios-sdk-", "\"");
            string[] sdkPathArray;
            if (podStr != null)
            {
                podStr = "\"" + podStr + "\"";
                sdkPathArray = new string[] {
                     podStr,
                     "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/AppLovinSDK\"",
                 };
            }
            else
            {
                sdkPathArray = new string[] {
                     "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/AppLovinSDK\"",
                 };
            }
            string[] frameworkArray = new string[] {
                "-framework \"AppLovinSDK\""
            };
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }
        //Inmobi
        string InmobiSDKPath = buildPath + "/Pods/Target Support Files/InMobiSDK/";
        pathDir = new DirectoryInfo(InmobiSDKPath);
        //确认pod是否有Inmobi
        if (pathDir.Exists)
        {
            string[] sdkPathArray = new string[] {
                "\"${PODS_ROOT}/InMobiSDK\"",
                "\"${PODS_XCFRAMEWORKS_BUILD_DIR}/InMobiSDK\"",
            };

            string[] frameworkArray = new string[] {
                "-framework \"InMobiSDK\""
            };
            removeSetting(pathArray, sdkPathArray, frameworkArray);
            AddFrameworkPath(buildPath, sdkPathArray, frameworkArray);
        }
        //YSO
        string YSOSDKPath = buildPath + "/Pods/YsoNetworkSDK/";
        pathDir = new DirectoryInfo(YSOSDKPath);
        //确认pod是否有YSO
        if (pathDir.Exists)
        {
            string SDKPath = buildPath + "/Pods/YsoNetworkSDK/YsoNetwork.framework";
            pathDir = new DirectoryInfo(SDKPath);
            if (pathDir.Exists)
            {
                string projectPath = PBXProject.GetPBXProjectPath(buildPath);
                PBXProject pbxProject = new PBXProject();
                pbxProject.ReadFromFile(projectPath);
                string fileGuid = pbxProject.AddFile(SDKPath, "Frameworks/YsoNetwork.framework");
                pbxProject.AddFileToEmbedFrameworks(pbxProject.GetUnityMainTargetGuid(), fileGuid);
                pbxProject.WriteToFile(projectPath);
            }
        }
    }

    private static void AddFrameworkPath(string buildPath,string[] sdkPathArray,string[] frameworkArray = null,string[] sysFrameworkArray = null)
    {
        PBXProject pbxProject = new PBXProject();
        string projectPath = Path.Combine(buildPath, "./Unity-iPhone.xcodeproj/project.pbxproj");
        pbxProject.ReadFromFile(projectPath);
        string target = pbxProject.GetUnityFrameworkTargetGuid();

        foreach (string sdkPath in sdkPathArray)
        {
            pbxProject.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", sdkPath);
        }
        if(frameworkArray != null)
        {
            foreach (string sdkPath in frameworkArray)
            {
                pbxProject.AddBuildProperty(target, "OTHER_LDFLAGS", sdkPath);
            }
        }
        if(sysFrameworkArray != null)
        {
            foreach (string sysFramework in sysFrameworkArray)
            {
                pbxProject.AddFrameworkToProject(target, sysFramework,false);
            }
        }
        pbxProject.WriteToFile(projectPath);
    }

    private static void removeSetting(string[] pathArray,string[] sdkPathArray, string[] frameworkArray)
    {
        foreach (string fliePath in pathArray)
        {
            if (System.IO.File.Exists(fliePath))
            {
                string fileData = File.ReadAllText(fliePath);
                foreach (string sdkPath in sdkPathArray)
                {
                    fileData = fileData.Replace(sdkPath,"");
                }
                foreach (string framework in frameworkArray)
                {
                    fileData = fileData.Replace(framework, "");
                }
                File.WriteAllText(fliePath, fileData);
            }
        }
    }
    private static string finidString(string[] pathArray, string startStr, string endStr)
    {
        foreach (string fliePath in pathArray)
        {
            if (System.IO.File.Exists(fliePath))
            {
                string str = File.ReadAllText(fliePath);
                int startIndex = str.IndexOf(startStr);
                string callBackStr = "";
                if(startIndex > 0)
                {
                    string tempStr = str.Substring(startIndex);
                    int endIndex = tempStr.IndexOf(endStr);
                    if(endIndex > 0)
                    {
                        callBackStr = tempStr.Remove(endIndex);
                    }
                }
                if(callBackStr.Length > 0)
                {
                    return callBackStr;
                }
            }
        }
        return null;
    }
}
#endif
