using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EeveexModManager.Nexus
{
    public class NexusUrl : Uri
    {
        #region Properties

        public Uri SourceUrl;

        public string ModId
        {
            get
            {
                string[] strUriParts = TrimmedSegments;
                if ((strUriParts.Length > 2) && strUriParts[1].Equals("mods", StringComparison.OrdinalIgnoreCase))
                    return strUriParts[2];
                return null;
            }
        }
        public string FileId
        {
            get
            {
                string[] strUriParts = TrimmedSegments;
                if ((strUriParts.Length > 4) && strUriParts[3].Equals("files", StringComparison.OrdinalIgnoreCase))
                    return strUriParts[4];
                return null;
            }
        }

        public string GameName
        {
            get
            {
                return Authority;
            }
        }
        

        protected string[] TrimmedSegments
        {
            get
            {
                string[] strUriParts = Segments;
                for (Int32 i = 0; i < strUriParts.Length; i++)
                {
                    strUriParts[i] = strUriParts[i].TrimEnd(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                }
                return strUriParts;
            }
        }

        #endregion

        #region Constructors
        public NexusUrl(Uri p_uriUrl)
            : base(p_uriUrl.ToString())
        {
            if (!Scheme.Equals("nxm", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Scheme must be NXM.");
            else
            {
                SourceUrl = p_uriUrl;
            }
        }

        #endregion
    }
}
