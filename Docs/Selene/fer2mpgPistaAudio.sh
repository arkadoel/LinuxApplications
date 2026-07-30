#/bin/bash
echo infile
echo "${inFile}"
echo out
echo "${outDir}/${title}.mpg"
now=$(date +"%Y%m%d%H%M")
#Listar pistas de audio
#ffprobe -v error -show_entries stream=index:stream_tags=language:stream=codec_name -select_streams a -of default=noprint_wrappers=1 -i tu_archivo.mkv

#ffmpeg -i "${inFile}" -c copy -map 0:v -map 0:m:language:spa ./OnlyEsp.mkv
# usar ffmpeg -i archivo y buscar el numero Stream #0:1 e ir probando
ffmpeg -i "${inFile}" -c:v mpeg2video -q:v 2 -s 854x480 -aspect 854:480 -threads 2 -c:a libmp3lame -filter:a dynaudnorm -map 0:0 -map 0:2 -y "${outDir}/${title}_${now}.mpg"

mv "${outDir}/${title}_${now}.mpg" /media/WD/Series/
