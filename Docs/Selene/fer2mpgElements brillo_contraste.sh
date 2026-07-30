#/bin/bash
echo infile
echo "${inFile}"
echo out
echo "${outDir}/${title}.mpg"
now=$(date +"%Y%m%d%H%M")

#brightness: This option sets the luma offset, which shifts the luma values by a constant amount. The range is -1 to 1, with 0 being the default value. A positive value increases the brightness, while a negative value decreases it.
#contrast: This option sets the luma gain, which scales the luma values by a constant factor. The range is 0 to 2, with 1 being the default value. A value greater than 1 increases the contrast, while a value less than 1 decreases it.

#ffmpeg -i input.mp4 -vf eq=brightness=1.2:contrast=1.5 output.mp4
#En este comando, el brillo aumenta un 20% (1,2 veces) y el contraste un 50% (1,5 veces).

#ffmpeg -i "${inFile}" -c:v mpeg2video -c:s mov_text -q:v 2 -s 854x480 -aspect 854:480 -threads 2 -c:a libmp3lame -vf eq=brightness=0.5:contrast=0.5 -filter:a dynaudnorm -y "${outDir}/${title}_${now}.mpg"

ffmpeg -i "${inFile}" -c:v mpeg2video -c:s mov_text -q:v 2 -s 854x480 -aspect 854:480 -threads 2 -c:a libmp3lame -vf eq=brightness=0.15:contrast=1 -filter:a dynaudnorm -y "${outDir}/${title}_${now}.mpg"


mv "${outDir}/${title}_${now}.mpg" /media/WD/Series/
