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

        private bool _hashFiles = false;

        public static AppManifest FromAppInfo(AppInfo app, AppLibrary library)
        {
            return FromAppInfo(app, library, false);
        }

        public static AppManifest FromAppInfo(AppInfo app, AppLibrary library, bool hashFiles)
        {
            string libPath = library.Path;
            Utility.EnsureEndsWithSlash(ref libPath);
            DirectoryInfo di = new DirectoryInfo(libPath + AppManifest.STEAM_COMMON_DIR + app.InstallDir);
            AppManifest manifest = new AppManifest();
            manifest._hashFiles = hashFiles;

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

        public static AppManifest FromDirectory(string path)
        {
            return FromDirectory(path, false);
        }

        public static AppManifest FromDirectory(string path, bool hashFiles)
        {
            AppManifest manifest = new AppManifest();
            manifest._hashFiles = hashFiles;
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
            foreach (FileInfo fi in di.GetFiles())
            {
                AppManifestFile addThis = new AppManifestFile(Utility.GetRelativePath(root.FullName, fi.FullName), (long)fi.Length);
                if (this._hashFiles)
                {
                    using (FileStream fs = File.OpenRead(fi.FullName))
                    {
                        string hash = Utility.GetSha1Hash(fs);
                        fs.Close();
                        addThis.sha1_hash = hash;
                    }
                }
                this.AddFile(addThis);
            }
            foreach (DirectoryInfo subdir in di.GetDirectories())
            {
                this.AddDirectory(subdir, root);
            }
        }

        private void AddFile(AppManifestFile file) 
        {
            
            files.Add(file);
        }

        public void RemoveMatchingFiles(AppManifest otherManifest)
        {
            // build dictionary of files
            Dictionary<string, AppManifestFile> filesDict = new Dictionary<string, AppManifestFile>();
            foreach (AppManifestFile thisFile in files)
            {
                filesDict.Add(thisFile.NormalizedPath, thisFile);
            }

            // see which ones exist
            foreach (AppManifestFile f in otherManifest.files)
            {
                if (filesDict.ContainsKey(f.NormalizedPath))
                {
                    if (filesDict[f.NormalizedPath].sha1_hash == f.sha1_hash)
                    {
                        // the other manifest contains a file with the same path and same hash, so remove our local copy
                        files.Remove(filesDict[f.NormalizedPath]);
                    }
                }
                else
                {
                    // the other manifest contains a file that's not in our local manifest, so create it in our manifest
                    // but with a filesize of zero to indicate that it should be deleted
                    files.Add(new AppManifestFile(f.path, 0));
                }
            }
        }
    }

    public struct AppManifestFile
    {
        public string path;
        public long size;
        public string sha1_hash;

        public AppManifestFile(string _path, long _size)
        {
            path = _path;
            size = _size;
            sha1_hash = "";
        }

        public AppManifestFile(string _path, long _size, string _sha1_hash)
        {
            path = _path;
            size = _size;
            sha1_hash = _sha1_hash;
        }

        public string NormalizedPath 
        {
            get 
            {
               return path.ToUpper().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
        }
    }
}
