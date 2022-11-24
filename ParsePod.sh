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
            echo "\t\t<iosPod name=\"$name/International\" version=\"$version\"/>" >> $saveFile
            echo "\t\t<iosPod name=\"$name/BUAdSDK\" version=\"$version\"/>" >> $saveFile
        #过滤特殊源的 pod配置
        #KSAdSDK 快手动态库 无法直接添加到 UnityFramework
        #BaiduMobAdSDK 百度其资源包需要放置在主项目中
        #OgurySdk HyBid（Verve） smaato-ios-sdk SDK中有动态库 无法直接添加到 UnityFramework
        elif [[ $name == 'KSAdSDK' || $name == 'OgurySdk' || $name == 'HyBid' || $name == 'smaato-ios-sdk'|| $name == 'BaiduMobAdSDK' || $name == 'BigoADS' ]]
        then
            UnityiPhonePod="$UnityiPhonePod\\\t$line\\\n"
        else
            echo "\t\t<iosPod name=\"$name\" version=\"$version\"/>" >> $saveFile
        fi
    fi
done < $(dirname $0)/$fileName

echo "\t</iosPods>" >> $saveFile
echo "</dependencies>" >> $saveFile
    
if [[ ${#UnityiPhonePod} > 0 ]]
then
    UnityiPhonePod="\\\ntarget 'Unity-iPhone' do\\\n${UnityiPhonePod}"
fi
saveString="string podInfo = \"$UnityiPhonePod\";"
sed -i "" "12d" "$(dirname $0)/TPPodSet.cs"
sed -i "" "12i\\
        $saveString
" "$(dirname $0)/TPPodSet.cs"
