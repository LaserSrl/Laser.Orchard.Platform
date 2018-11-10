using Laser.Orchard.Vimeo.Models;
using Laser.Orchard.Vimeo.ViewModels;
using Orchard;
using Orchard.MediaLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.Vimeo.Services {

    public interface IVimeoTaskServices : IDependency {
        void ScheduleUploadVerification();
        void ScheduleVideoCompletion();
        int VerifyAllUploads();
        int TerminateUploads();
    }

    public interface IVimeoAdminServices : IDependency {
        //bool Create(VimeoSettingsPartViewModel settings);
        //VimeoSettingsPartViewModel GetByToken(string aToken);
        VimeoSettingsPartViewModel GetSettingsVM();
        void UpdateSettings(VimeoSettingsPartViewModel vm);

        void ConsolidateTokensList(VimeoSettingsPartViewModel vm);
        void CommitTokensUpdate(VimeoSettingsPartViewModel vm);

        bool TokenIsValid(VimeoSettingsPartViewModel vm);
        string TokensAreValid(VimeoSettingsPartViewModel vm);

        bool GroupIsValid(VimeoSettingsPartViewModel vm);
        bool AlbumIsValid(VimeoSettingsPartViewModel vm);
        bool ChannelIsValid(VimeoSettingsPartViewModel vm);
    }

    public interface IVimeoUploadServices : IDependency {
        //call these methods to properly start an upload
        //TODO: replace these with a single method
        int IsValidFileSize(Int64 fileSize);
        string GenerateUploadTicket(int uploadId);
        int GenerateNewMediaPart(int uploadId);

        string GetUploadUrl(int mediaPartId);

        //these methods terminate an upload
        VerifyUploadResult VerifyUpload(int mediaPartId);
        bool TerminateUpload(int mediaPartId);

        string DestroyUpload(int mediaPartId);

#if DEBUG
        //these methods here don't need to be exposed, but they are easier to test and debug if they may be called directly.
        bool TokenIsValid(string aToken, bool shouldUpdateRateLimits);
        //bool GroupIsValid(string gName, string aToken);
        //bool AlbumIsValid(string aName, string aToken);
        //bool ChannelIsValid(string cName, string aToken);

        VimeoUploadQuota CheckQuota();
        Int64 UsedQuota();
        Int64 FreeQuota();

        string PatchVideo(int ucId, string name = "", string description = "");
        string TryAddVideoToGroup(int ucId);
        string TryAddVideoToChannel(int ucId);
        string TryAddVideoToAlbum(int ucId);

        string ExtractVimeoStreamURL(int ucId);

        void FinishMediaPart(int ucId);

        string GetVideoStatus(int ucId);

        void ClearRepositoryTables();
#endif
    }

    public interface IVimeoContentServices : IDependency {
        string ExtractVimeoStreamURL(OEmbedPart part);
        string EncryptedVideoUrl(string url);
    }
}
