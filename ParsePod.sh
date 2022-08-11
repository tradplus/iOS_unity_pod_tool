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
        if [[ $name == 'Ads-CN' ]]
        then
            echo "\t\t<iosPod name=\"$name/International\" version=\"$version\"/>" >> $saveFile
            echo "\t\t<iosPod name=\"$name/BUAdSDK\" version=\"$version\"/>" >> $saveFile
        elif [[ $name == 'KSAdSDK' || $name == 'OgurySdk' || $name == 'HyBid' || $name == 'smaato-ios-sdk'|| $name == 'BaiduMobAdSDK' || $name == 'UnityAds' ]]
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
    UnityiPhonePod="\\\ntarget 'Unity-iPhone' do\\\n${UnityiPhonePod}end"
fi
saveString="string podInfo = \"$UnityiPhonePod\";"
sed -i "" "12d" "$(dirname $0)/TPPodSet.cs"
sed -i "" "12i\\
        $saveString
" "$(dirname $0)/TPPodSet.cs"
