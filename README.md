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

注：导出前需要关闭 PlayerServicesResolver 的 Always add the main target to Podfile 这个设置。设置面板路径 Unity菜单 `Assets > External Dependency Manager > iOS Resolver > Settings`

这样当从unity项目导出xcode项目时就可以通过pod自动加载相关SDK。
        
TPPodSet.cs 会处理  快手 百度 Verve smaato Ogury这几个特殊源的配置问题。

1.快手SDK 由于是动态库无法直接配置在UnityFramework中

2.百度 如果配置在UnityFramework 中会导致SDK无法找到百度的资源包

3.Verve smaato Ogury SDK库中有动态库无法直接配置在 UnityFramework中


另：TPPodSet.cs 中也包括 增加Info.plist内容 及 关闭bitcode 功能，请根据实际需求使用。
