namespace Modules.Storage.Core.Models.Data;

public static class StorageCacheKeys
{
    public static string TempBlobDownloadKey(string blobId)
    {
        return $"BlobDownload/{blobId}";
    }
}