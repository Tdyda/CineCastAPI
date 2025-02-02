# Ścieżki wejściowa i wyjściowa przekazywane jako argumenty
param (
    [string]$filePath,
    [string]$outputManifestPath
)

# Ustaw kodowanie na UTF-8
[System.Console]::OutputEncoding = [System.Text.Encoding]::UTF8



# Sprawdź, czy podano oba argumenty
if (-not $filePath -or -not $outputManifestPath) {
    Write-Host "Użycie: .\script.ps1 <ścieżka do pliku wejściowego> <ścieżka do pliku manifestu wyjściowego>"
    exit 1
}

# Sprawdź, czy plik wejściowy istnieje
if (-not (Test-Path $filePath)) {
    Write-Host "Plik wejściowy nie istnieje, proszę sprawdzić ścieżkę."
    exit 1
}

# Uzyskaj pełną ścieżkę i nazwę pliku bez rozszerzenia
$folderPath = [System.IO.Path]::GetDirectoryName($filePath)
$fileName = [System.IO.Path]::GetFileNameWithoutExtension($filePath)

# Ścieżki do innych plików wyjściowych
$previewVideoPath = Join-Path $folderPath "thumbnail.mp4"
$thumbnailImagePath = Join-Path $folderPath "thumbnail.jpg"

# Oblicz długość pliku w sekundach za pomocą ffprobe
$durationOutput = & ffprobe -v error -select_streams v:0 -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "$filePath" | Out-String

# Sprawdź, czy wynik jest prawidłowy
if (-not $durationOutput) {
    Write-Host "Nie udało się odczytać czasu trwania pliku. Upewnij się, że ffprobe jest zainstalowane."
    exit 1
}

# Zamień czas trwania na liczbę
$totalDurationInSeconds = [math]::Round([double]$durationOutput.Trim())

# Oblicz wartości startowe dla klipów
$start1 = [math]::Round($totalDurationInSeconds * 0.2)
$start2 = [math]::Round($totalDurationInSeconds * 0.5)
$start3 = [math]::Round($totalDurationInSeconds * 0.8)

# Zdefiniuj czas trwania klipu
$clipDuration = 5  # Czas trwania klipu w sekundach

# Przygotuj argumenty do ffmpeg dla wygenerowania manifestu HLS
$ffmpegArgsManifest = "-i `"$filePath`" -codec:v copy -codec:a copy -start_number 0 -hls_time 5 -hls_list_size 0 -f hls `"$outputManifestPath`""

# Wyświetlenie argumentów dla debugu
Write-Host "Używane argumenty ffmpeg dla manifestu HLS: $ffmpegArgsManifest"

# Uruchom ffmpeg dla manifestu HLS
Write-Host "Generowanie manifestu HLS..."
Invoke-Expression "ffmpeg $ffmpegArgsManifest"

# Potwierdzenie zakończenia
Write-Host "Manifest HLS zapisany w: $outputManifestPath"

# Przygotuj argumenty do ffmpeg dla wygenerowania podglądu
$ffmpegArgsPreview = "-i `"$filePath`" -filter_complex "
$ffmpegArgsPreview += "`"[0]trim=start=$($start1):end=$($start1 + $clipDuration),setpts=PTS-STARTPTS[v1];"
$ffmpegArgsPreview += "[0]trim=start=$($start2):end=$($start2 + $clipDuration),setpts=PTS-STARTPTS[v2];"
$ffmpegArgsPreview += "[0]trim=start=$($start3):end=$($start3 + $clipDuration),setpts=PTS-STARTPTS[v3];"
$ffmpegArgsPreview += "[v1][v2][v3]concat=n=3:v=1:a=0[v]`" "

# Dodaj argumenty wyjściowe z ustawieniami rozdzielczości i bitrate
$ffmpegArgsPreview += "-map `[v]` -s 640x360 -b:v 150k -c:v h264_nvenc -preset fast -crf 28 `"$previewVideoPath`""

# Wyświetlenie argumentów dla debugu
Write-Host "Używane argumenty ffmpeg dla podglądu: $ffmpegArgsPreview"

# Uruchom ffmpeg dla podglądu
Write-Host "Przetwarzanie pliku wideo w celu utworzenia podglądu..."
Invoke-Expression "ffmpeg $ffmpegArgsPreview"

# Potwierdzenie zakończenia
Write-Host "Podgląd wideo zapisany w: $previewVideoPath"

# Przygotuj argumenty ffmpeg do stworzenia miniaturki z wygenerowanego podglądu (pierwsza klatka)
$ffmpegArgsThumbnailFromPreview = "-i `"$previewVideoPath`" -vframes 1 -q:v 2 `"$thumbnailImagePath`""

# Wyświetlenie argumentów dla debugu
Write-Host "Używane argumenty ffmpeg dla miniaturki z podglądu: $ffmpegArgsThumbnailFromPreview"

# Uruchom ffmpeg dla miniaturki z podglądu
Write-Host "Przetwarzanie podglądu w celu stworzenia miniaturki..."
Invoke-Expression "ffmpeg $ffmpegArgsThumbnailFromPreview"

# Potwierdzenie zakończenia
Write-Host "Miniaturka z podglądu zapisana w: $thumbnailImagePath"
