using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SteamLanSync
{
    public class AppInfo
    {
        public string AppId = "";
        public string Name = "";
        public string InstallDir = "";
        public int BuildId = 0;
        public long Size = 0;
        public int StateFlags = 0;

        public AppInfo()
        {

        }

        public AppInfo(string _name, long _size, string _appId)
        {
            Name = _name;
            Size = _size;
            AppId = _appId;
        }

        public override bool Equals(object obj)
        {
            AppInfo app = obj as AppInfo;
            if ((object)app == null)
            {
                return false;
            }
            return app.AppId.Equals(AppId);
        }

        public override int GetHashCode()
        {
            return AppId.GetHashCode();
        }

        public static AppInfo FromAcf(string s)
        {
            Match m;
            AppInfo info = new AppInfo();

            m = Regex.Match(s, @"""appID""\s+""([^""]+)""", RegexOptions.IgnoreCase);
            if (m.Groups.Count > 1)
            { 
                info.AppId = m.Groups[1].Value; 
            }

            m = Regex.Match(s, @"""SizeOnDisk""\s+""(\d+)""", RegexOptions.IgnoreCase);
            if (m.Groups.Count > 1)
            {
                info.Size = long.Parse(m.Groups[1].Value);
            }

            m = Regex.Match(s, @"""name""\s+""([^""]+)""", RegexOptions.IgnoreCase);
            if (m.Groups.Count > 1)
            {
                info.Name = m.Groups[1].Value;
            }

            m = Regex.Match(s, @"""installdir""\s+""([^""]+)""", RegexOptions.IgnoreCase);
            if (m.Groups.Count > 1)
            {
                info.InstallDir = m.Groups[1].Value;
            }

            m = Regex.Match(s, @"""StateFlags""\s+""(\d+)""", RegexOptions.IgnoreCase);
            if (m.Groups.Count > 1)
            {
                info.StateFlags = int.Parse(m.Groups[1].Value);
            }

            m = Regex.Match(s, @"""buildid""\s+""(\d+)""", RegexOptions.IgnoreCase);
            if (m.Groups.Count > 1)
            {
                info.BuildId = int.Parse(m.Groups[1].Value);
            }

            if (info.AppId.Length > 0 && info.Name.Length > 0 && info.Size > 0 && info.InstallDir.Length > 0)
            {
                return info;
            }
            else
            {
                return null;
            }
        }
    }
}
