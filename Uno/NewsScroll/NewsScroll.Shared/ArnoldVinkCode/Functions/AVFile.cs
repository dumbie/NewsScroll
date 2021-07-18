using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ArnoldVinkCode
{
    class AVFile
    {
        //Save Text to file
        internal static async Task<StorageFile> SaveText(string FileName, string FileText)
        {
            try
            {
                StorageFile CreateFileAsync = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(CreateFileAsync, FileText);
                return CreateFileAsync;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed saving Text to file.");
                return null;
            }
        }

        //Save Buffer to file
        internal static async Task<StorageFile> SaveBuffer(string FileName, IBuffer FileBuffer)
        {
            try
            {
                StorageFile CreateFileAsync = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBufferAsync(CreateFileAsync, FileBuffer);
                return CreateFileAsync;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed saving Buffer to file.");
                return null;
            }
        }

        //Save Bytes to file
        internal static async Task<StorageFile> SaveBytes(string FileName, byte[] FileBytes)
        {
            try
            {
                StorageFile CreateFileAsync = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
                using (Stream OpenStreamForWriteAsync = await CreateFileAsync.OpenStreamForWriteAsync()) { await OpenStreamForWriteAsync.WriteAsync(FileBytes, 0, FileBytes.Length); }
                return CreateFileAsync;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed saving Bytes to file.");
                return null;
            }
        }
    }
}