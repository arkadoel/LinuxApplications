#/bin/bash
echo infile
echo "${inFile}"
echo out
echo "${outDir}/${title}.mpg"
now=$(date +"%Y%m%d%H%M")

#sudo killall cpulimit
#cpulimit -P ffmpeg -l 20 &

ffmpeg -i "${inFile}" -c:v mpeg2video -c:s mov_text -q:v 2 -s 854x480 -aspect 854:480 -threads 2 -c:a libmp3lame -filter:a dynaudnorm -y "${outDir}/${title}_${now}.mpg"

#elegir pista de audio con parametro  map
#ffmpeg -i "${inFile}" -c:v mpeg2video -c:s mov_text -q:v 2 -s 854x480 -aspect 854:480 -threads 2 -c:a libmp3lame -filter:a dynaudnorm -map 0:0 -map 0:1 -y "${outDir}/${title}_${now}.mpg"


mv "${outDir}/${title}_${now}.mpg" /media/WD/Series/
