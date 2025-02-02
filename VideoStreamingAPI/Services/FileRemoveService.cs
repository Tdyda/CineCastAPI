
namespace VideoStreamingAPI.Services
{
    public class FileRemoveService : IFileRemoveService
    {
        public bool RemoveFile(string filePath)
        {
            if (filePath != null)
            {
                Directory.Delete(filePath, recursive: true);
            }
            return true;
        }
    }
}
