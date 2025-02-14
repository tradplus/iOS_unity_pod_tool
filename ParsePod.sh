#!/bin/sh

fileName="TPPods"

saveFile="$(dirname $0)/${fileName}Dependencies.xml"

if [ ! -f "$saveFile" ]
then
touch $saveFile
fi

UnityiPhonePod="";

echo "<dependencies>" > $saveFile
echo "\t<iosPods>" >> $saveFile

while read line || [[ -n ${line} ]]
do
    if [[ $line == pod* ]]
    then
        name=${line#*\'}
        name=${name%%\'*}
        version=${line#*,}
        version=${version#*\'}
        version=${version%%\'*}
        #穿山甲&Pangle pod配置
        if [[ $name == 'Ads-CN' ]]
        then
            tempVersion=${version//./}
                if [ ${#tempVersion} > 4 ]
            then
                tempVersion=${tempVersion:0:4}
            fi
            #5.2.0.0穿山甲pod版本已拆分
            if [ $tempVersion -ge 5200 ]
            then
                echo "\t\t<iosPod name=\"$name\" version=\"$version\"/>" >> $saveFile
            else
                echo "\t\t<iosPod name=\"$name/International\" version=\"$version\"/>" >> $saveFile
                echo "\t\t<iosPod name=\"$name/BUAdSDK\" version=\"$version\"/>" >> $saveFile
            fi
        #过滤特殊源的 pod配置
        #KSAdSDK 快手动态库 无法直接添加到 UnityFramework
        #BaiduMobAdSDK 百度其资源包需要放置在主项目中
        #OgurySdk HyBid（Verve） smaato-ios-sdk AmazonPublisherServicesSDK MaioSDK-v2 SDK中有动态库 无法直接添加到 UnityFramework
        elif [[ $name == 'KSAdSDK' || $name == 'OgurySdk' || $name == 'HyBid' || $name == 'smaato-ios-sdk' || $name == 'BaiduMobAdSDK' || $name == 'AmazonPublisherServicesSDK' || $name == 'MaioSDK-v2' || $name == 'TradPlusKwaiAdsSDK' ]]
        then
            UnityiPhonePod="$UnityiPhonePod\\\t$line\\\n"
        #Bigo
        elif [[ $name == 'BigoADS' ]]
            then
            tempVersion=${version//./}
                if [ ${#tempVersion} > 3 ]
            then
                tempVersion=${tempVersion:0:3}
            fi
            #4.1.1+ 无动态库
            if [ $tempVersion -ge 411 ]
            then
                echo "\t\t<iosPod name=\"$name\" version=\"$version\"/>" >> $saveFile
            else
                UnityiPhonePod="$UnityiPhonePod\\\t$line\\\n"
            fi
        #Tapjoy v13.3.0+ 动态库
        elif [[ $name == 'TapjoySDK' ]]
        then
            tempVersion=${version//./}
            if [ ${#tempVersion} > 4 ]
            then
                tempVersion=${tempVersion:0:4}
            fi
            if [ $tempVersion -ge 1330 ]
            then
                UnityiPhonePod="$UnityiPhonePod\\\t$line\\\n"
            else
                echo "\t\t<iosPod name=\"$name\" version=\"$version\"/>" >> $saveFile
            fi
        #Start.io v4.9.1 动态库
        elif [[ $name == 'StartAppSDK' ]]
        then
            tempVersion=${version//./}
            if [ ${#tempVersion} > 3 ]
            then
                tempVersion=${tempVersion:0:3}
            fi
            if [ $tempVersion -eq 491 ]
            then
                UnityiPhonePod="$UnityiPhonePod\\\t$line\\\n"
            else
                echo "\t\t<iosPod name=\"$name\" version=\"$version\"/>" >> $saveFile
            fi
        #Fyber >=v8.2.0  <v8.2.7 动态库
        elif [[ $name == 'Fyber_Marketplace_SDK' ]]
        then
            tempVersion=${version//./}
            if [ ${#tempVersion} > 3 ]
            then
                tempVersion=${tempVersion:0:3}
            fi
            if [ $tempVersion -ge 820 -a $tempVersion -lt 827 ]
            then
                UnityiPhonePod="$UnityiPhonePod\\\t$line\\\n"
            else
                echo "\t\t<iosPod name=\"$name\" version=\"$version\"/>" >> $saveFile
            fi
        #AppLovinSDK >= 12.4.1 动态库
        elif [[ $name == 'AppLovinSDK' ]]
        then
            tempVersion=${version//./}
            if [ ${#tempVersion} > 4 ]
            then
                tempVersion=${tempVersion:0:4}
            fi
            if [ $tempVersion -ge 1241 ]
            then
                UnityiPhonePod="$UnityiPhonePod\\\t$line\\\n"
            else
                echo "\t\t<iosPod name=\"$name\" version=\"$version\"/>" >> $saveFile
            fi
        #InMobiSDK >= 10.7.2 动态库
        elif [[ $name == 'InMobiSDK' ]]
        then
            tempVersion=${version//./}
            if [ ${#tempVersion} > 4 ]
            then
                tempVersion=${tempVersion:0:4}
            fi
            if [ $tempVersion -ge 1072 ]
            then
                UnityiPhonePod="$UnityiPhonePod\\\t$line\\\n"
            else
                echo "\t\t<iosPod name=\"$name\" version=\"$version\"/>" >> $saveFile
            fi
        else
            echo "\t\t<iosPod name=\"$name\" version=\"$version\"/>" >> $saveFile
        fi
    fi
done < $(dirname $0)/$fileName

echo "\t</iosPods>" >> $saveFile
echo "</dependencies>" >> $saveFile
    
if [[ ${#UnityiPhonePod} > 0 ]]
then
    UnityiPhonePod="\\\ntarget 'Unity-iPhone' do\\\n\\\tuse_frameworks!\\\n${UnityiPhonePod}"
fi
saveString="string podInfo = \"$UnityiPhonePod\";"
sed -i "" "15d" "$(dirname $0)/TPPodSet.cs"
sed -i "" "15i\\
        $saveString
" "$(dirname $0)/TPPodSet.cs"
