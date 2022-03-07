using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if __IOS__ || __MACOS__
using Foundation;
#endif
using Windows.Storage;

namespace NewsScroll.Cache
{
    public class Cache
    {
        //Clear webcache from device
        public static bool ClearCache()
        {
            try
            {
#if __ANDROID__
                return ClearCacheAndroid();
#elif __IOS__ || __MACOS__
                return ClearCacheApple(false);
#elif WINDOWS_UWP
                return ClearCacheWindows();
#endif
            }
            catch { }
            return false;
        }

        public static bool ClearCacheAndroid()
        {

            try
            {
#if __ANDROID__
                Android.Webkit.WebStorage.Instance.DeleteAllData();
                string cacheDirectoryString = System.IO.Path.GetTempPath();
                Java.IO.File cacheDirectoryFile = new Java.IO.File(cacheDirectoryString);
                if (cacheDirectoryFile != null && cacheDirectoryFile.Exists())
                {
                    string[] fileNames = cacheDirectoryFile.List();
                    foreach (string fileName in fileNames)
                    {
                        try
                        {
                            if (!fileNames.Equals("lib"))
                            {
                                new Java.IO.File(cacheDirectoryFile, fileName).Delete();
                            }
                        }
                        catch { }
                    }
                }
#endif
                return true;
            }
            catch { }
            return false;
        }

        public static bool ClearCacheApple(bool removeCookies)
        {
            try
            {
#if __IOS__ || __MACOS__
                NSError nsError = null;
                NSFileManager fileManager = NSFileManager.DefaultManager;
                NSUrl libraryDirectoryUrl = fileManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User).FirstOrDefault();

                //Remove app cache
                string cachesDirectoryString = Path.Combine(libraryDirectoryUrl.Path, "Caches");
                fileManager.Remove(cachesDirectoryString, out nsError);

                //Remove webkit cache
                string webkitDirectoryString = Path.Combine(libraryDirectoryUrl.Path, "WebKit");
                fileManager.Remove(webkitDirectoryString, out nsError);

                //Remove cookies
                if (removeCookies)
                {
                    string cookiesDirectoryString = Path.Combine(libraryDirectoryUrl.Path, "Cookies");
                    fileManager.Remove(cookiesDirectoryString, out nsError);
                }
#endif
                return true;
            }
            catch { }
            return false;
        }

        public static bool ClearCacheWindows()
        {
            try
            {
#if WINDOWS_UWP
                string[] streamCacheDirectories = Directory.GetDirectories(ApplicationData.Current.LocalFolder.Path + "\\..\\ac\\inetcache");
                foreach (string streamDirectory in streamCacheDirectories)
                {
                    IEnumerable<string> streamCacheFiles = Directory.EnumerateFiles(streamDirectory);
                    foreach (string streamFile in streamCacheFiles)
                    {
                        try
                        {
                            if (DateTime.Now.Subtract(File.GetLastAccessTime(streamFile)).TotalSeconds > 60)
                            {
                                File.Delete(streamFile);
                            }
                        }
                        catch { }
                    }
                }
#endif
                return true;
            }
            catch { }
            return false;
        }
    }
}