using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager
{
    public interface IModRepository
    {
        #region Custom Events

        event EventHandler UserStatusUpdate;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the id of the mod repository.
        /// </summary>
        /// <value>The id of the mod repository.</value>
        string Id { get; }

        /// <summary>
        /// Gets the name of the mod repository.
        /// </summary>
        /// <value>The name of the mod repository.</value>
        string Name { get; }

        /// <summary>
        /// Gets the user membership status.
        /// </summary>
        /// <value>The user membership status.</value>
        string[] UserStatus { get; }

        /// <summary>
        /// Gets the User Agent used for the mod repository.
        /// </summary>
        /// <value>The User Agent.</value>
        string UserAgent { get; }

        /// <summary>
        /// Gets whether the repository is in a forced offline mode.
        /// </summary>
        /// <value>Whether the repository is in a forced offline mode.</value>
        bool IsOffline { get; }

        /// <summary>
        /// Gets whether the repository supports unauthenticated downloads.
        /// </summary>
        /// <value>Whether the repository supports unauthenticated downloads.</value>
        bool SupportsUnauthenticatedDownload { get; }

        /// <summary>
        /// Gets the repository's file server zones.
        /// </summary>
        /// <value>the repository's file server zones.</value>
        List<FileServerZone> FileServerZones { get; }

        /// <summary>
        /// Gets the number allowed connections.
        /// </summary>
        /// <value>The number allowed connections.</value>
        Int32 AllowedConnections { get; }

        /// <summary>
        /// Gets the number of maximum allowed concurrent downloads.
        /// </summary>
        /// <value>The number of maximum allowed concurrent downloads.</value>
        Int32 MaxConcurrentDownloads { get; }

        #endregion

        #region Account Management

        /// <summary>
        /// Logs the user into the mod repository.
        /// </summary>
        /// <param name="p_strUsername">The username of the account with which to login.</param>
        /// <param name="p_strPassword">The password of the account with which to login.</param>
        /// <param name="p_dicTokens">The returned tokens that can be used to login instead of the username/password
        /// credentials.</param>
        /// <returns><c>true</c> if the login was successful;
        /// <c>fales</c> otherwise.</returns>
        bool Login(string p_strUsername, string p_strPassword, out Dictionary<string, string> p_dicTokens);

        bool Login(Dictionary<string, string> tokens_dictionary);

        void Logout();

        #endregion

        IModInfo GetModInfoForFile(string fileName);

        IModInfo GetModInfo(string modId);

        List<IModInfo> GetModListInfo(List<string> modList);

        bool ToggleEndorsement(string modId, int localState);

        IList<IModFileInfo> GetModFileInfo(string modId);

        Uri[] GetFilePartUrls(string modId, string fileId);

        List<FileServerInfo> GetFilePartInfo(string modId, string fileId, string userLocation, out string repositoryMessage);

        IModFileInfo GetFileInfo(string modId, string fileId);

        IModFileInfo GetFileInfoForFile(string fileName);

        IModFileInfo GetDefaultFileInfo(string modId);

        IList<IModInfo> FindMods(string modNameSearchString, bool includeAllTerms);

        IList<IModInfo> FindMods(string modNameSearchString, string authorSearchString);

        IList<IModInfo> FindMods(string modNameSearchString, string authorSearchString, bool includeAllTerms);
    }
}
