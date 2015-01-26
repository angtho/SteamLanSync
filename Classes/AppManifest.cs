using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SteamLanSync
{
    
    public class AppManifest
    {
        public const string STEAM_COMMON_DIR = @"common\";

        public AppInfo appInfo;
        public List<AppManifestFile> files = new List<AppManifestFile>();

        public static AppManifest FromAppInfo(AppInfo app, AppLibrary library)
        {
            string libPath = library.Path;
            Utility.EnsureEndsWithSlash(ref libPath);
            DirectoryInfo di = new DirectoryInfo(libPath);
            string acfPath = di.FullName + "appmanifest_" + app.AppId + ".acf";
            AppManifest manifest = new AppManifest();
            
            try
            {
                manifest.AddDirectory(new DirectoryInfo(libPath + AppManifest.STEAM_COMMON_DIR + app.InstallDir), di);
                if (File.Exists(acfPath))
                {
                    FileInfo fi = new FileInfo(acfPath);
                    manifest.AddFile(new AppManifestFile(Utility.GetRelativePath(libPath, fi.FullName), (long)fi.Length));
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return manifest;
        }

        public static AppManifest FromDirectory(string path)
        {
            AppManifest manifest = new AppManifest();
            DirectoryInfo di = new DirectoryInfo(path);
            try
            {
                manifest.AddDirectory(di, di);
            }
            catch (Exception e)
            {
                throw e;
            }

            return manifest;
        }

        private void AddDirectory(DirectoryInfo di, DirectoryInfo root)
        {
            foreach (DirectoryInfo subdir in di.GetDirectories())
            {
                this.AddDirectory(subdir, root);
            }
            foreach (FileInfo fi in di.GetFiles())
            {
                this.AddFile(new AppManifestFile(Utility.GetRelativePath(root.FullName, fi.FullName), (long)fi.Length));
            }
        }

        private void AddFile(AppManifestFile file) 
        {
            files.Add(file);
        }
    }

    public struct AppManifestFile
    {
        public string path;
        public long size;

        public AppManifestFile(string _path, long _size)
        {
            path = _path;
            size = _size;
        }
    }
}
