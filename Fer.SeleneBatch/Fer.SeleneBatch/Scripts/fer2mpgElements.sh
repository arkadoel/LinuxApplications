#!/bin/bash

# El primer argumento ($1) es la ruta completa del vídeo que le pasa C#
inFile="$1"

# Extraemos la carpeta y el nombre sin extensión automáticamente
outDir=$(dirname "${inFile}")
filename=$(basename "${inFile}")
title="${filename%.*}"

echo "Archivo de entrada: ${inFile}"
echo "Directorio de salida: ${outDir}"
echo "Título: ${title}"

now=$(date +"%Y%m%d%H%M")

#sudo killall cpulimit
#cpulimit -P ffmpeg -l 20 &

ffmpeg -i "${inFile}" -c:v mpeg2video -c:s mov_text -q:v 2 -s 854x480 -aspect 854:480 -threads 2 -c:a libmp3lame -filter:a dynaudnorm -y "${outDir}/${title}_${now}.mpg"
#elegir pista de audio con parametro  map
#ffmpeg -i "${inFile}" -c:v mpeg2video -c:s mov_text -q:v 2 -s 854x480 -aspect 854:480 -threads 2 -c:a libmp3lame -filter:a dynaudnorm -map 0:0 -map 0:1 -y "${outDir}/${title}_${now}.mpg"

mv "${outDir}/${title}_${now}.mpg" /media/WD/Series/
