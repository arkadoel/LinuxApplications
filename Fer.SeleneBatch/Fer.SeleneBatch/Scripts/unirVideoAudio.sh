#!/bin/bash

# El primer argumento ($1) es la ruta completa del vídeo que le pasa C#
inFile="$1"

# Extraemos la carpeta y el nombre sin extensión automáticamente
outDir=$(dirname "${inFile}")
filename=$(basename "${inFile}")
title="${filename%.*}"

# Construimos la ruta del archivo .mp3 correspondiente
audioFile="${outDir}/${title}.mp3"

echo "Archivo de vídeo: ${inFile}"
echo "Archivo de audio: ${audioFile}"
echo "Directorio de salida: ${outDir}"
echo "Título: ${title}"

# Comprobamos si existe el archivo de audio antes de procesar
if [ ! -f "${audioFile}" ]; then
    echo "Error: No se encontró el archivo de audio coincidente (${audioFile})"
    exit 1
fi

now=$(date +"%Y%m%d%H%M")

# -map 0:v:0 -> Toma la pista de vídeo del primer entrada ($inFile)
# -map 1:a:0 -> Toma la pista de audio de la segunda entrada ($audioFile)
ffmpeg -i "${inFile}" -i "${audioFile}" \
       -map 0:v:0 -map 1:a:0 \
       -c:v mpeg2video -q:v 2 -s 854x480 -aspect 854:480 -threads 2 \
       -c:a libmp3lame -filter:a dynaudnorm \
       -y "${outDir}/${title}_${now}.mpg"

#mv "${outDir}/${title}_${now}.mpg" /media/WD/Series/