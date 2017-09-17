using System.IO;

namespace ConsoleBlob.Services
{
    public interface IBlobService
    {
        void ClearMetadata();
        void CopyBlob(string fromBlobName, string toBlobName);
        void ListAttributes();
        void ListMetadata();
        void SetMetaData(string key, string value);
        void UploadBlob(string blobName, Stream file);
        void UploadBlobSubDirectory(string directoryName, string subDirectoryName, string blobName, Stream file);
    }
}
