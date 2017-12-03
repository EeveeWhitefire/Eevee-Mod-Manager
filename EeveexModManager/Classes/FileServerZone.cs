using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace EeveexModManager
{
    public class FileServerZone
    {
        private string ServerName = String.Empty;
        private string ServerId = String.Empty;
        private Int32 AffinityId = 0;
        private bool IsPremium = false;

        #region Properties

        /// <summary>
        /// Gets the name of the fileserver.
        /// </summary>
        /// <value>The name of the fileserver.</value>
        public string FileServerName
        {
            get
            {
                return ServerName;
            }
            private set
            {
                ServerName = value;
            }
        }

        /// <summary>
        /// Gets the ID of the fileserver.
        /// </summary>
        /// <value>The ID of the fileserver.</value>
        public string FileServerID
        {
            get
            {
                return ServerId;
            }
            private set
            {
                ServerId = value;
            }
        }

        /// <summary>
        /// Gets the affinity of the fileserver, affinity is used to choose the nearest alternative.
        /// </summary>
        /// <value>The affinity of the fileserver, affinity is used to choose the nearest alternative.</value>
        public Int32 FileServerAffinity
        {
            get
            {
                return AffinityId;
            }
            private set
            {
                AffinityId = value;
            }
        }

        /// <summary>
        /// Gets the premium status of the fileserver.
        /// </summary>
        /// <value>The premium status of the fileserver.</value>
        public bool FileServerIsPremium
        {
            get
            {
                return IsPremium;
            }
            private set
            {
                IsPremium = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// The default constructor the initializes the object.
        /// </summary>
        public FileServerZone()
        {
            ServerId = "default";
            ServerName = "Default (CDN)";
            AffinityId = 0;
            IsPremium = false;
        }

        /// <summary>
        /// A simple constructor the initializes the object with the given parameters.
        /// </summary>
        /// <param name="p_strFileServerID">The fileserver ID.</param>
        /// <param name="p_strFileServerName">The fileserver name.</param>
        /// <param name="p_intAffinityID">The fileserver affinity.</param>
        /// <param name="p_booPremium">The fileserver premium status.</param>
        public FileServerZone(string p_strFileServerID, string p_strFileServerName, Int32 p_intAffinityID, bool p_booPremium)
        {
            ServerId = p_strFileServerID;
            ServerName = p_strFileServerName;
            AffinityId = p_intAffinityID;
            IsPremium = p_booPremium;
        }

        #endregion
    }
}
