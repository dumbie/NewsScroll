using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace ArnoldVinkCode
{
    class AVImage
    {
        //Load SvgImage
        internal static async Task<SvgImageSource> LoadSvgImage(string Path)
        {
            if (!string.IsNullOrWhiteSpace(Path) && Path.Contains(".svg"))
            {
                TaskCompletionSource<SvgImageSource> TaskResult = new TaskCompletionSource<SvgImageSource>();
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        SvgImageSource SvgToBitmapImage = new SvgImageSource();
                        SvgToBitmapImage.UriSource = new Uri(Path, UriKind.RelativeOrAbsolute);
                        TaskResult.SetResult(SvgToBitmapImage);
                    }
                    catch { TaskResult.SetResult(null); }
                });
                return await TaskResult.Task;
            }
            else { return null; }
        }

        //Load BitmapImage
        internal static async Task<BitmapImage> LoadBitmapImage(string Path, bool AutoPlay)
        {
            if (!string.IsNullOrWhiteSpace(Path))
            {
                TaskCompletionSource<BitmapImage> TaskResult = new TaskCompletionSource<BitmapImage>();
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        BitmapImage ImageToBitmapImage = new BitmapImage();

                        //Check if image file exists
                        if (Path.StartsWith("ms-appdata:///local") && await AVFunctions.LocalFileExists(Path))
                        {
                            //System.Diagnostics.Debug.WriteLine("LocalImage exists: " + LocalFileName);
                            string LocalFileName = Path.Replace("ms-appdata:///local/", string.Empty);
                            StorageFile StorageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(LocalFileName);
                            using (IRandomAccessStream OpenAsync = await StorageFile.OpenAsync(FileAccessMode.Read))
                            {
                                await ImageToBitmapImage.SetSourceAsync(OpenAsync);
                                OpenAsync.Dispose();
                            }
                            TaskResult.SetResult(ImageToBitmapImage);
                        }
                        else if (Path.StartsWith("ms-appx:///") && await AVFunctions.AppFileExists(Path))
                        {
                            //System.Diagnostics.Debug.WriteLine("AppImage exists: " + Path);
                            ImageToBitmapImage.UriSource = new Uri(Path, UriKind.RelativeOrAbsolute);
                            TaskResult.SetResult(ImageToBitmapImage);
                        }
                        else if (Path.StartsWith("http"))
                        {
                            //System.Diagnostics.Debug.WriteLine("Image online: " + Path);
                            ImageToBitmapImage.UriSource = new Uri(Path, UriKind.RelativeOrAbsolute);
                            ImageToBitmapImage.AutoPlay = AutoPlay;
                            TaskResult.SetResult(ImageToBitmapImage);
                        }
                        else if (!Path.StartsWith("ms-") && File.Exists(Path))
                        {
                            //System.Diagnostics.Debug.WriteLine("Image exists: " + Path);
                            ImageToBitmapImage.UriSource = new Uri(Path, UriKind.RelativeOrAbsolute);
                            TaskResult.SetResult(ImageToBitmapImage);
                        }
                        else if (Path.StartsWith("data:"))
                        {
                            if (Path.Length > 200)
                            {
                                System.Diagnostics.Debug.WriteLine("Image data: " + Path);
                                TaskResult.SetResult(await Base64StringToBitmapImage(Path));
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Invalid image data received.");
                                TaskResult.SetResult(null);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Image not found: " + Path);
                            TaskResult.SetResult(null);
                        }
                    }
                    catch { TaskResult.SetResult(null); }
                });
                return await TaskResult.Task;
            }
            else { return null; }
        }

        //Convert Base64 image to BitmapImage
        private static async Task<BitmapImage> Base64StringToBitmapImage(string base64String)
        {
            try
            {
                string RawBase64Data = Regex.Match(base64String, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                byte[] byteBuffer = Convert.FromBase64String(RawBase64Data);

                BitmapImage image = new BitmapImage();
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(byteBuffer.AsBuffer());
                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                }
                return image;
            }
            catch { return null; }
        }
    }
}