#!/bin/bash

# Sprawdź, czy podano oba argumenty
if [ "$#" -ne 2 ]; then
    echo "Użycie: ./script.sh <ścieżka do pliku wejściowego> <ścieżka do pliku manifestu wyjściowego>"
    exit 1
fi

filePath="$1"
outputManifestPath="$2"

# Sprawdź, czy plik wejściowy istnieje
if [ ! -f "$filePath" ]; then
    echo "Plik wejściowy nie istnieje, proszę sprawdzić ścieżkę."
    exit 1
fi

# Uzyskaj pełną ścieżkę i nazwę pliku bez rozszerzenia
folderPath=$(dirname "$filePath")
fileName=$(basename "$filePath" | sed 's/\.[^.]*$//')

# Ścieżki do innych plików wyjściowych
previewVideoPath="$folderPath/thumbnail.mp4"
thumbnailImagePath="$folderPath/thumbnail.jpg"

# Oblicz długość pliku w sekundach za pomocą ffprobe
durationOutput=$(ffprobe -v error -select_streams v:0 -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "$filePath")
if [ -z "$durationOutput" ]; then
    echo "Nie udało się odczytać czasu trwania pliku. Upewnij się, że ffprobe jest zainstalowane."
    exit 1
fi

# Zamień czas trwania na liczbę
totalDurationInSeconds=$(printf "%.0f" "$durationOutput")

# Oblicz wartości startowe dla klipów
start1=$((totalDurationInSeconds * 20 / 100))
start2=$((totalDurationInSeconds * 50 / 100))
start3=$((totalDurationInSeconds * 80 / 100))

# Zdefiniuj czas trwania klipu
clipDuration=5  # Czas trwania klipu w sekundach

# Przygotuj argumenty do ffmpeg dla wygenerowania manifestu HLS
ffmpegArgsManifest="-i \"$filePath\" -codec:v copy -codec:a copy -start_number 0 -hls_time 5 -hls_list_size 0 -f hls \"$outputManifestPath\""

# Wyświetlenie argumentów dla debugu
echo "Używane argumenty ffmpeg dla manifestu HLS: $ffmpegArgsManifest"

# Uruchom ffmpeg dla manifestu HLS
echo "Generowanie manifestu HLS..."
eval ffmpeg $ffmpegArgsManifest

# Potwierdzenie zakończenia
echo "Manifest HLS zapisany w: $outputManifestPath"

# Przygotuj argumenty do ffmpeg dla wygenerowania podglądu
ffmpegArgsPreview="-i \"$filePath\" -filter_complex "
ffmpegArgsPreview+="\"[0]trim=start=$start1:end=$((start1 + clipDuration)),setpts=PTS-STARTPTS[v1];"
ffmpegArgsPreview+="[0]trim=start=$start2:end=$((start2 + clipDuration)),setpts=PTS-STARTPTS[v2];"
ffmpegArgsPreview+="[0]trim=start=$start3:end=$((start3 + clipDuration)),setpts=PTS-STARTPTS[v3];"
ffmpegArgsPreview+="[v1][v2][v3]concat=n=3:v=1:a=0[v]\" "
ffmpegArgsPreview+="-map \"[v]\" -s 640x360 -b:v 150k -c:v h264_nvenc -preset fast -crf 28 \"$previewVideoPath\""

# Wyświetlenie argumentów dla debugu
echo "Używane argumenty ffmpeg dla podglądu: $ffmpegArgsPreview"

# Uruchom ffmpeg dla podglądu
echo "Przetwarzanie pliku wideo w celu utworzenia podglądu..."
eval ffmpeg $ffmpegArgsPreview

# Potwierdzenie zakończenia
echo "Podgląd wideo zapisany w: $previewVideoPath"

# Przygotuj argumenty ffmpeg do stworzenia miniaturki z wygenerowanego podglądu (pierwsza klatka)
ffmpegArgsThumbnailFromPreview="-i \"$previewVideoPath\" -vframes 1 -q:v 2 \"$thumbnailImagePath\""

# Wyświetlenie argumentów dla debugu
echo "Używane argumenty ffmpeg dla miniaturki z podglądu: $ffmpegArgsThumbnailFromPreview"

# Uruchom ffmpeg dla miniaturki z podglądu
echo "Przetwarzanie podglądu w celu stworzenia miniaturki..."
eval ffmpeg $ffmpegArgsThumbnailFromPreview

# Potwierdzenie zakończenia
echo "Miniaturka z podglądu zapisana w: $thumbnailImagePath"
