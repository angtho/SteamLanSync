using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SteamLanSync
{
    public class AppLibrary
    {
        private string _path;
        public string Path 
        {
            get { return _path;  }
        }
        public Dictionary<String, AppInfo> Apps = new Dictionary<string,AppInfo>();

        public static AppLibrary FromDirectory(string path)
        {
            AppLibrary lib = new AppLibrary();
            lib._path = path;
            string acf;
            AppInfo appInfo;

            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Length < 10 * 1024)
                {
                    acf = File.ReadAllText(fi.FullName);
                    appInfo = AppInfo.FromAcf(acf);
                    if (appInfo != null && appInfo.StateFlags == 4) // 4 = fully installed
                    {
                        lib.Apps.Add(appInfo.AppId, appInfo);
                    }
                }
            }

            return lib;
        }

        public static string[] DetermineLibraryLocation()
        {
            string[] result = new string[] {};

            List<string> candidates = new List<string>();

            // check registry for steam install location
            object regResult;
            regResult = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", String.Empty);
            
            // if not found, exit (user will have to tell us)
            if (regResult == null || (string)regResult == string.Empty)
                return new string[] { };

            string steamPath = (string)regResult;
            Utility.EnsureEndsWithSlash(ref steamPath);
            steamPath = steamPath.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);

            // see if we can find a "config.vdf" file
            string vdfPath = steamPath + System.IO.Path.DirectorySeparatorChar + "config" + System.IO.Path.DirectorySeparatorChar + "config.vdf";
            if (new FileInfo(vdfPath).Exists)
            {
                // if so, look for "installdir" elements
                TextReader reader = File.OpenText(vdfPath);
                string line;
                Regex rgx = new Regex(@"""installdir""\s+""([^""]+)""", RegexOptions.IgnoreCase);
                Match m;
                string installDir;

                while ((line = reader.ReadLine()) != null)
                {
                    m = rgx.Match(line);
                    if (m.Groups.Count < 2)
                        continue;

                    // we will end up with something like "D:\\INSTALL\\Steam\\steamapps\\common\\Crysis Wars"
                    // need to unescape the slashes and find "steamapps" in the path
                    installDir = m.Groups[1].Value;
                    installDir = installDir.Replace(@"\\", System.IO.Path.DirectorySeparatorChar.ToString()); 
                    installDir = installDir.ToLower();
                    int pos = installDir.IndexOf("steamapps");
                    if (pos < 0)
                        continue;

                    installDir = installDir.Substring(0, pos + "steamapps".Length);
                    if (!candidates.Contains(installDir))
                        candidates.Add(installDir);
                }
                
            }
            else
            {
                // if not found, check for "steamapps" within install location
                if (!candidates.Contains(steamPath + "steamapps"))
                    candidates.Add(steamPath + "steamapps");
            }
            
            if (candidates.Count == 0)
                return new string[] { };

            List<string> filtered = new List<string>();
            DirectoryInfo di;

            foreach (string cand in candidates)
            {
                // test if the candidate exists
                di = new DirectoryInfo(cand.ToUpper());
                if (!di.Exists)
                    continue;

                if (!IsValidLibraryPath(di.FullName))
                    continue;
                

                filtered.Add(Utility.GetProperDirectoryCapitalization(di));
            }

            return filtered.ToArray();
        }

        // Tests that a directory exists and is named "steamapps"
        public static bool IsValidLibraryPath(string path)
        {
            if (path.Length == 0)
                return false;

            try
            {
                // see if it has a subdirectory called "common"
                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists)
                    return false;

                if (di.Name.ToUpper() != "STEAMAPPS")
                    return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Caught exception in IsValidLibraryPath with path [" + path + "]");
                Debug.WriteLine(ex);
            }
            
            return true;
        }

        public static bool CanWriteToDirectory(string path)
        {
            if (path.Length == 0)
                return false;

            try
            {
                // see if it has a subdirectory called "common"
                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists)
                    return false;

                Utility.EnsureEndsWithSlash(ref path);
                int i = 0;
                string filePath;
                do
                {
                    filePath = path + "testfile" + i.ToString() + ".txt";
                } while (File.Exists(filePath));
                
                FileStream fs = File.OpenWrite(filePath);
                byte[] bytes = Encoding.UTF8.GetBytes("I CAN WRITE");
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
                fs.Close();
                string readBack = File.ReadAllText(filePath);
                if (readBack == "I CAN WRITE") {
                    File.Delete(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Caught exception in CanWriteToDirectory with path [" + path + "]");
                Debug.WriteLine(ex);
            }

            return false;
        }
    }
}
