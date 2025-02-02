# Konfiguracja i uruchomienie API

To repozytorium zawiera backendową część aplikacji API. Poniżej znajdziesz pełną konfigurację oraz instrukcje, jak uruchomić aplikację po raz pierwszy.

## Spis treści

- [Wymagania](#wymagania)
- [Klonowanie repozytorium](#klonowanie-repozytorium)
- [Konfiguracja środowiska](#konfiguracja-środowiska)
  - [Tworzenie bazy danych](#tworzenie-bazy-danych)
  - [Plik `appsettings.json` i zmienne środowiskowe](#plik-appsettingsjson-i-zmienne-środowiskowe)
- [Migracje bazy danych](#migracje-bazy-danych)
- [Konfiguracja ról i uprawnień](#konfiguracja-ról-i-uprawnień)
- [Uruchomienie aplikacji](#uruchomienie-aplikacji)
- [Swagger](#swagger)
- [Przeniesienie aplikacji na środowisko produkcyjne](#przeniesienie-aplikacji-na-środowisko-produkcyjne)
- [Dodatkowe informacje](#dodatkowe-informacje)

## Wymagania

- Środowisko .NET (wersja zgodna z projektem)
- Serwer bazy danych (np. MySQL, PostgreSQL, SQL Server itp.)
- Narzędzie do wysyłania zapytań HTTP (np. Postman) lub dostęp do [Swaggera](#swagger) w trybie deweloperskim

## Klonowanie repozytorium

Sklonuj repozytorium na swój komputer:

```bash
git clone https://github.com/Tdyda/CineCastAPI.git
```

## Konfiguracja środowiska
### 1. Tworzenie bazy danych
    - Po pobraniu repozytorium utwórz nową bazę danych na docelowym serwerze.
    - Upewnij się, że posiadasz odpowiednie uprawnienia do tworzenia bazy danych oraz modyfikacji jej struktury.

### 2. Plik appsettings.json i zmienne środowiskowe
    Aplikacja korzysta z konfiguracji zapisanej w pliku appsettings.json oraz ze zmiennych środowiskowych. Poniżej znajduje się przykładowa zawartość pliku appsettings.json:
```json
    {
        "Logging": {
            "LogLevel": {
                "Default": "Information",
                "Microsoft.AspNetCore": "Warning"
            }
        },
        "AllowedHosts": "*",
        "ConnectionStrings": {
            "ConnectionString": "Server=${DB_SERVER};Port=${DB_PORT};Database=${DB_NAME};User=${DB_USER};Password=${DB_PASSWORD};"
        },
        "Jwt": {
            "Key": "[KEY]",
            "Issuer": "[ISSUER]",
            "Audience": "[AUDIENCE]"
        },
        "AppSettings": {
            "uploadFolderPath": "[PATH TO VIDEO DIRECTORY]",
            "actorsPhotosPath": "[PATH TO ACTORS PHOTOS DIRECTORY]",
            "tempChunksPath": "[PATH TO TEMP CHUNKS DIRECTORY]",
            "ffmpegScriptPath": "[PATH TO FFMPEG SCRIPT DIRECTORY]"
        },
        "Swagger": {
            "Enabled": true
        },
        "Kestrel": {
            "Endpoints": {
                "Http": {
                    "Url": "http://127.0.0.1:5003"
                }
            }
        }
    }
```

Sposób konfiguracji
- ConnectionString:
ConnectionString zawiera zmienne w postaci ${NAZWA_ZMIENNEJ}. Aplikacja podmienia te zmienne za pomocą następującego fragmentu kodu:

```csharp
var connectionStringTemplate = builder.Configuration.GetConnectionString("ConnectionString") ?? throw new Exception("ConnectionString is missing in appsettings.json");

var connectionString = Regex.Replace(connectionStringTemplate, @"\$\{(\w+)\}", match =>
{
    var envVarName = match.Groups[1].Value;
    var envValue = Environment.GetEnvironmentVariable(envVarName);

    if (string.IsNullOrEmpty(envValue))
    {
        Console.WriteLine($"Brak wartości dla zmiennej: {envVarName}");
        return match.Value;
    }

    return envValue;
});
```
Aby poprawnie skonfigurować połączenie z bazą danych, ustaw następujące zmienne środowiskowe:

- DB_SERVER
- DB_PORT
- DB_NAME
- DB_USER
- DB_PASSWORD

JWT oraz AppSettings:
Parametry dla sekcji Jwt i AppSettings są pobierane z zmiennych środowiskowych dzięki metodzie builder.Configuration.AddEnvironmentVariables();. Upewnij się, że na docelowym serwerze ustawione są odpowiednie zmienne środowiskowe, np.:

- Klucz JWT: Jwt__Key
- Issuer: Jwt__Issuer
- Audience: Jwt__Audience
- Ścieżki dla konfiguracji aplikacji:
- AppSettings__uploadFolderPath
- AppSettings__actorsPhotosPath
- AppSettings__tempChunksPath
- AppSettings__ffmpegScriptPath


## Migracje bazy danych
W projekcie znajdują się przygotowane migracje. Aby zastosować migracje na utworzonej bazie danych, uruchom poniższą komendę w terminalu:

``` bash
dotnet ef database update
```
Upewnij się, że connection string (po podmianie zmiennych) wskazuje na właściwą, utworzoną wcześniej bazę danych.

## Konfiguracja ról i uprawnień
Po przeprowadzeniu migracji bazy danych należy skonfigurować rolę administratora:

### 1. Dodanie roli administratora:

Wyślij zapytanie HTTP POST na endpoint:

``` bash
/api/ManageRoles/AddRole
```
Możesz użyć do tego Postmana lub Swaggera (domyślnie dostępnego w trybie deweloperskim).

### 2. Przypisanie roli administratora do użytkownika:

Aby przypisać utworzoną rolę do konkretnego użytkownika, wyślij zapytanie HTTP POST na endpoint:

```bash
/api/ManageRoles/AssignRole
```
W treści żądania przekaż identyfikator użytkownika oraz rolę administratora.

## Uruchomienie aplikacji
Po wykonaniu powyższych kroków (utworzenie bazy danych, konfiguracja środowiska, zastosowanie migracji oraz dodanie roli administratora) możesz uruchomić aplikację:

```bash
dotnet run
```
Aplikacja zostanie uruchomiona na skonfigurowanym porcie, domyślnie:

```cpp
http://127.0.0.1:5003
```

## Swagger
Jeśli w konfiguracji appsettings.json opcja Swaggera jest włączona ("Enabled": true), możesz uzyskać dostęp do interfejsu Swagger, który umożliwia testowanie endpointów. W trybie deweloperskim otwórz przeglądarkę i przejdź pod adres:

```bash
http://127.0.0.1:5003/swagger
```

## Przeniesienie aplikacji na środowisko produkcyjne

Aby uruchomić aplikację w środowisku produkcyjnym, wykonaj następujące kroki:

### 1. Przygotowanie serwera
- Upewnij się, że serwer posiada wymagane zależności, takie jak:
  - .NET Runtime (zgodna wersja z projektem)
  - Serwer bazy danych (np. MySQL, PostgreSQL, SQL Server)
  - Nginx/Apache jako reverse proxy (opcjonalnie)
  - Certyfikat SSL (zalecane dla HTTPS)

### 2. Publikacja aplikacji
Aby wygenerować pliki do uruchomienia na serwerze, użyj polecenia:

```bash
dotnet publish -c Release -o ./publish
```

### 3. Zmienne środowiskowe
Ustawiamy je dokładnie w ten sam sposób co dla środowiska testowego (opisanme powyżej)

### 4. Uruchomienie aplikacji
Po opublikowaniu i skonfigurowaniu aplikacji uruchom ją poleceniem:

```bash
dotnet YourApp.dll
```

## Dodatkowe informacje
### 1. Logi aplikacji:
W przypadku problemów sprawdź logi, aby zdiagnozować ewentualne błędy.

### 2. Zmienne środowiskowe:
Upewnij się, że na środowisku produkcyjnym wszystkie wymagane zmienne środowiskowe są poprawnie ustawione.

### 3. Dalsza konfiguracja:
Możesz dostosować inne ustawienia w pliku appsettings.json (np. konfiguracja logowania, Kestrel) zgodnie z potrzebami.

### 4. Skrypty
W katalogu `/scripts` znajdują się dwa skrypty processVideo:  
- `processVideo.ps1` – dla systemów Windows (PowerShell)  
- `processVideo.sh` – dla systemów Linux/Mac (Bash)  

Wybierz odpowiednią wersję skryptu w zależności od używanego systemu operacyjnego na serwerze.

### 5. Domyślnie aplikacja jest skonfigurowana do uruchamiania na serwerach Linuxowych.  
  Aby uruchomić ją w środowisku Windows, należy zmodyfikować metodę `GenerateHlsManifestAsync` w pliku `Services/FileUploadService.cs`,  
  zmieniając jej sposób wywoływania skryptu z Basha na PowerShella.  
  Odpowiednia implementacja dla Windows znajduje się poniżej:

  ```csharp
  public async Task<string> GenerateHlsManifestAsync(string filePath, string outputFolderPath, string ffmpegScriptPath)
  {
      string outputManifestPath = Path.Combine(outputFolderPath, "output.m3u8");

      var process = new Process
      {
          StartInfo = new ProcessStartInfo
          {
              FileName = "powershell",
              Arguments = $"-File \"{ffmpegScriptPath}\" \"{filePath}\" \"{outputManifestPath}\"",
              RedirectStandardOutput = true,
              RedirectStandardError = true,
              UseShellExecute = false,
              CreateNoWindow = true
          }
      };

      var output = new System.Text.StringBuilder();
      var errorOutput = new System.Text.StringBuilder();

      process.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);
      process.ErrorDataReceived += (sender, args) => errorOutput.AppendLine(args.Data);

      try
      {
          process.Start();
          process.BeginOutputReadLine();
          process.BeginErrorReadLine();

          await process.WaitForExitAsync();

          if (process.ExitCode != 0)
          {
              throw new InvalidOperationException($"Skrypt zakończył się błędem:\n{errorOutput}\nWyjście:\n{output}");
          }
          return outputManifestPath;
      }
      catch
      {
          Console.WriteLine($"Błąd wykonywania skryptu: {errorOutput}");
          throw new InvalidOperationException($"Skrypt zakończył się błędem:\n{errorOutput}\nWyjście:\n{output}");
      }
  }