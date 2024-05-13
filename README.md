# iOS_unity_pod_tool

如果您的unity项目使用了 PlayerServicesResolver

可以通过`ParsePod.sh`方便的把 Tradplus 打包平台上提供的pod 配置自动转成 PlayerServicesResolver 可用的配置

[PlayerServicesResolver项目地址](https://github.com/googlesamples/unity-jar-resolver)

## 使用方法

1. 将 ParsePod.sh TPPodSet.cs 两个文件放置在一个目录下

2. 从Tradplus [iOS打包平台](https://docs.tradplusad.com/docs/integration_ios/download)上获取SDK相关pod配置

3. 将配置覆盖TPPods文件中内容

4. 在Mac的终端中执行ParsePod.sh，这样就会自动更新 TPPodsDependencies.xml 和 TPPodSet.cs的内容

5. 将TPPodsDependencies.xml 和 TPPodSet.cs 两个文件放置项目中 Assets/ExternalDependencyManager/Editor 此文件夹下

另：如果没有使用 TPPodSet.cs 相关说明 中的相关广告源  ; 也不需要使用TPPodSet.cs内其他功能的时，只需要放入TPPodsDependencies.xml 就可以了。

这样当从unity项目导出xcode项目时就可以通过pod自动加载相关SDK。

## TPPodSet.cs 相关说明

TPPodSet.cs 中也包括 增加Info.plist内容 及 关闭bitcode 功能，请根据实际需求使用。

TPPodSet.cs 会处理 三方动态库的配置问题。

特殊源的说明：

1.**百度** 如果配置在UnityFramework 中会导致SDK无法找到百度的资源包

2.**快手** 三方SDK为动态库 无法直接配置在UnityFramework中

3.**Verve** 三方SDK库中有动态库无法直接配置在 UnityFramework中

4.**Smaato** 三方SDK库中有动态库无法直接配置在 UnityFramework中

5.**Ogury** 三方SDK库中有动态库无法直接配置在 UnityFramework中

6.**Amazon** 三方SDK库中有动态库无法直接配置在 UnityFramework中

7.**快手海外** 三方SDK库中有动态库无法直接配置在 UnityFramework中

7.**Bigo** v4.1.1之前版本 SDK库中有动态库无法直接配置在 UnityFramework中

8.**Fyber** v8.2.0-v8.2.7 三方SDK为动态库 无法直接配置在 UnityFramework中

9.**Start.io** v4.9.1 三方SDK为动态库 无法直接配置在 UnityFramework中

10.**Maio** v2.0及之后版本 三方SDK为动态库 无法直接配置在 UnityFramework中

11.**Tapjoy** v13.3.0及之后版本 三方SDK为动态库 无法直接配置在 UnityFramework中

12.**AppLovin** v12.4.1及之后版本 三方SDK为动态库 无法直接配置在 UnityFramework中

13.**InMobi** v10.7.2及之后版本 三方SDK为动态库 无法直接配置在 UnityFramework中
