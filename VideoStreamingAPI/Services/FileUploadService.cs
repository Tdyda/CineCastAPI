using System.Diagnostics;

namespace VideoStreamingAPI.Services
{
    public class FileUploadService : IFileUploadService
    {
        public async Task<bool> SaveChunkAsync(IFormFile file, int chunkIndex, string tempFolderPath)
        {
            var tempFilePath = Path.Combine(tempFolderPath, $"chunk_{chunkIndex}");
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return true;
        }

        public Task<bool> AreAllChunksReceived(int totalChunks, string tempFolderPath)
        {
            var receivedChunks = Directory.GetFiles(tempFolderPath, "chunk_*").Length;
            return Task.FromResult(receivedChunks == totalChunks);
        }

        public async Task<string> CombineChunksAsync(string fileName, string tempFolderPath, string uploadFolderPath)
        {
            var outputFolderPath = Path.Combine(uploadFolderPath, fileName);
            Directory.CreateDirectory(outputFolderPath);
            var finalFilePath = Path.Combine(outputFolderPath, $"{fileName}.mp4");

            using (var writeStream = new FileStream(finalFilePath, FileMode.Create))
            {
                var chunks = Directory.GetFiles(tempFolderPath, "chunk_*").Length;
                for (int i = 0; i < chunks; i++)
                {
                    var chunkPath = Path.Combine(tempFolderPath, $"chunk_{i}");
                    using (var readStream = new FileStream(chunkPath, FileMode.Open))
                    {
                        await readStream.CopyToAsync(writeStream);
                    }
                    System.IO.File.Delete(chunkPath);
                }
            }

            return finalFilePath;
        }

        public async Task<string> GenerateHlsManifestAsync(string filePath, string outputFolderPath, string ffmpegScriptPath)
        {
            string outputManifestPath = Path.Combine(outputFolderPath, "output.m3u8");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"\"{ffmpegScriptPath}\" \"{filePath}\" \"{outputManifestPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            var output = new System.Text.StringBuilder();
            var errorOutput = new System.Text.StringBuilder();

            // Przechwytywanie wyjścia i błędów
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
                    throw new InvalidOperationException($"Skrypt zakończył się błędem:\n{errorOutput.ToString()}\nWyjście:\n{output.ToString()}");
                }
                return outputManifestPath;
            }
            catch
            {
                Console.WriteLine($"blad wykonywania skryptu: {errorOutput}");
                throw new InvalidOperationException($"Skrypt zakończył się błędem:\n{errorOutput.ToString()}\nWyjście:\n{output.ToString()}");
            }
        }


        public async Task<bool> UploadPhoto(string actorsPhotosPath, IFormFile photo)
        {
            if (!Directory.Exists(actorsPhotosPath))
            {
                Directory.CreateDirectory(actorsPhotosPath);
            }

            var photoName = photo.FileName;

            var path = Path.Combine(actorsPhotosPath, photoName);

            if (File.Exists(path))
            {
                return false;
            }

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }
            return true;
        }

        public bool RenameFile(string oldPath, string newPath)
        {
            if (!Directory.Exists(oldPath))
            {
                return false;
            }

            Directory.Move(oldPath, newPath);

            return true;
        }
    }
}
