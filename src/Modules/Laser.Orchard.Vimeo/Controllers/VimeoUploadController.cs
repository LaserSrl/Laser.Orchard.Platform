using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.Vimeo.Services;
using Newtonsoft.Json;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.IO;
using System.Web.Mvc;

namespace Laser.Orchard.Vimeo.Controllers {

    public class VimeoUploadController : Controller {

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private readonly IVimeoUploadServices _vimeoUploadServices;
        private readonly IUtilsServices _utilsServices;

        public VimeoUploadController(IVimeoUploadServices vimeoUploadServices, IUtilsServices utilsServices) {
            _vimeoUploadServices = vimeoUploadServices;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ActionResult TryStartUpload(Int64 fileSize) {
            //TODO: make all the ticket creation in a single call from here. This is mostly so we do not send record ids
            //or anything like that out of the services
            try {
                int uploadId = _vimeoUploadServices.IsValidFileSize(fileSize);
                string message = T("Everything is fine").ToString();
                if (uploadId >= 0) {
                    //If there is enough quota available, open an upload ticket, by posting to VimeoEndpoints.VideoUpload
                    //with parameter type=streaming
                    string uploadUrl = _vimeoUploadServices.GenerateUploadTicket(uploadId);
                    //create a new MediaPart 
                    int MediaPartId = _vimeoUploadServices.GenerateNewMediaPart(uploadId);
                    object data = new { MediaPartId, uploadUrl };
                    return Json(new VimeoOrchardResponse{
                        ErrorCode = VimeoOrchardErrorCode.NoError,
                        Success = true,
                        Message = message,
                        Data = data
                    });
                } else {
                    //If there is not enough upload quota available, return an error or something.
                    message = T("Error: Not enough upload quota available").ToString();
                    return Json(new VimeoOrchardResponse {
                        ErrorCode = VimeoOrchardErrorCode.NotEnoughQuota,
                        Success = false,
                        Message = message,
                        ResolutionAction = VimeoResolutionAction.NoAction
                    });
                }
            } catch (VimeoRateException vre) {
                return Json(new VimeoOrchardResponse {
                    ErrorCode = VimeoOrchardErrorCode.RateLimited,
                    Success = false,
                    Message = string.Format("Rate will reset on {0} UTC", vre.resetTime.Value.ToString()),
                    ResolutionAction = VimeoResolutionAction.NoAction
                });
            }
        }


#if DEBUG
        //this method to test extracting the URL of the vimeo streams. It will not be present in the production systems
        //NOTE: at any time, these methods here in this region may not be functional, as they are continuosly tweaked to 
        //test different things.
        public ActionResult ExtractVimeoStreamUrl(int ucId) {
            //_vimeoUploadServices.FinishMediaPart(ucId);
            string ret = _vimeoUploadServices.ExtractVimeoStreamURL(ucId);//_vimeoUploadServices.GetVideoStatus(ucId);//
            return Content(ret); //JsonConvert.SerializeObject(new { ret })
        }

        public ActionResult ClearUploadRepositoryTables() {
            _vimeoUploadServices.ClearRepositoryTables();
            return null;
        }
#endif


        public ActionResult FinishUpload(int mediaPartId) {
            string message = "";
            VimeoOrchardErrorCode eCode = VimeoOrchardErrorCode.GenericError;
            VimeoResolutionAction rAction = VimeoResolutionAction.NoAction;
            bool success = false;
            string uploadUrl = null; //I use this in case we need to resume an upload
            //re-verify upload
            try {
                switch (_vimeoUploadServices.VerifyUpload(mediaPartId)) {
                    case VerifyUploadResult.CompletedAlready:
                        //the periodic task had already verified that the upload had completed
                        message = T("The upload process has finished.").ToString();
                        eCode = VimeoOrchardErrorCode.NoError;
                        rAction = VimeoResolutionAction.NoAction;
                        success = true;
                        break;
                    case VerifyUploadResult.Complete:
                        //we just found out that the upload is complete
                        try {
                            if (_vimeoUploadServices.TerminateUpload(mediaPartId)) {
                                //Make sure the finisher task exists
                                message = T("The upload process has finished.").ToString();
                                eCode = VimeoOrchardErrorCode.NoError;
                                rAction = VimeoResolutionAction.NoAction;
                                success = true;
                            } else {
                                //we might end up here in the case when the termination was requested at the same time from here and from the task
                                message = T("The upload has completed, but there was an error while handling the finishing touches.").ToString();
                                eCode = VimeoOrchardErrorCode.FinishingErrors;
                                rAction = VimeoResolutionAction.NoAction;
                                success = false;
                            }
                        } catch (Exception) {
                            //we might end up here in the case when the termination was requested at the same time from here and from the task
                            message = T("The upload has completed, but there was an error while handling the finishing touches.").ToString();
                            eCode = VimeoOrchardErrorCode.FinishingErrors;
                            rAction = VimeoResolutionAction.NoAction;
                            success = false;
                        }
                        break;
                    case VerifyUploadResult.Incomplete:
                        //the upload is still going on
                        message = T("The upload is still in progress.").ToString();
                        eCode = VimeoOrchardErrorCode.InProgress;
                        rAction = VimeoResolutionAction.ContinueUpload;
                        success = false;
                        //the client may want to resume the upload, so we send them the upload Url
                        uploadUrl = _vimeoUploadServices.GetUploadUrl(mediaPartId);
                        break;
                    case VerifyUploadResult.StillUploading:
                        //the upload is still going on
                        message = T("The upload is still in progress.").ToString();
                        eCode = VimeoOrchardErrorCode.InProgress;
                        rAction = VimeoResolutionAction.ContinueUpload;
                        success = false;
                        //the client may want to resume the upload, so we send them the upload Url
                        uploadUrl = _vimeoUploadServices.GetUploadUrl(mediaPartId);
                        break;
                    case VerifyUploadResult.NeverExisted:
                        //we never started an upload with the given Id
                        message = T("The upload was never started, or the MediaPart is not for a Vimeo video.").ToString();
                        eCode = VimeoOrchardErrorCode.UploadNeverStarted;
                        rAction = VimeoResolutionAction.RestartUpload;
                        success = false;
                        break;
                    case VerifyUploadResult.Error:
                        //something went wrong
                        message = T("Unknown error.").ToString();
                        eCode = VimeoOrchardErrorCode.GenericError;
                        rAction = VimeoResolutionAction.RestartUpload;
                        success = false;
                        break;
                    default:
                        //we should never be here
                        message = T("Unknown error.").ToString();
                        eCode = VimeoOrchardErrorCode.GenericError;
                        rAction = VimeoResolutionAction.RestartUpload;
                        success = false;
                        break;
                }
            } catch (VimeoRateException vre) {
                return Json(new VimeoOrchardResponse {
                    ErrorCode = VimeoOrchardErrorCode.RateLimited,
                    Success = false,
                    Message = string.Format("Rate will reset on {0} UTC", vre.resetTime.Value.ToString()),
                    ResolutionAction = VimeoResolutionAction.NoAction
                });
            }
            
            var response = new VimeoOrchardResponse {
                ErrorCode = eCode,
                Success = success,
                Message = message,
                ResolutionAction =  rAction
            };
            //There are issues with using ajax to get the return value from this method:
            // response is a VimeoOrchardResponse, that overrides the ErrorCode field from the Response class.
            // Ajax sees the ErrorCode value from the base object, rather than the correct one. As a fix, we
            // put the error code also in the Data object. Ajax will read that rather than the correct field.
            var ErrorCode = eCode;
            if (!string.IsNullOrWhiteSpace(uploadUrl)) {
                response.Data = new { uploadUrl, ErrorCode };
            } else {
                response.Data = new { ErrorCode };
            }

            //return Json(new { ErrorCode = eCode, Success = success, Message = message, Data = response.Data }); //this breaks the api controller, so no
            return Json(response);
        }

        //We extend Laser.Orchard.StartupConfig.ViewModels.Response because we have specific error codes for Vimeo
        public enum VimeoOrchardErrorCode { NoError = 0, GenericError = 1,
            //errors we may get from the attemtp to finish an upload
            InProgress = 4001, UploadNeverStarted = 4002, FinishingErrors = 4003,
            //other vimeo errors
            NotEnoughQuota = 4004, RateLimited = 4005
        }
        public enum VimeoResolutionAction { NoAction = 0, ContinueUpload = 4001, RestartUpload = 4002 }
        public class VimeoOrchardResponse : Laser.Orchard.StartupConfig.ViewModels.Response {
            new public VimeoOrchardErrorCode ErrorCode { get; set; }
            new public VimeoResolutionAction ResolutionAction { get; set; }

            public VimeoOrchardResponse() {
                this.ErrorCode = VimeoOrchardErrorCode.GenericError;
                this.Success = false;
                this.Message = "Generic Error";
                this.ResolutionAction = VimeoResolutionAction.NoAction;
            }
        }

        /// <summary>
        /// Endpoint where the applications may send their error messages when there are upload issues. We accept the following messages:
        /// 3001: User stopped the upload
        /// 3002: Upload stopped, will not resume
        /// 3003: Upload stopped, but may resume
        /// </summary>
        /// <param name="msgJson">A JSON representing a <type>Response</type> object describing the error.</param>
        /// <returns></returns>
        public ActionResult ErrorHandler() {
            string msgJson = new StreamReader(Request.InputStream).ReadToEnd();
            Laser.Orchard.StartupConfig.ViewModels.Response response = ErrorHandler(msgJson);

            return Json(response);
        }

        internal Response ErrorHandler(string msgJson) {
            VimeoResponse resp = JsonConvert.DeserializeObject<VimeoResponse>(msgJson);
            int mpId = resp.Data.id; //the id of the MediaPart for whom we were doing the upload
            Laser.Orchard.StartupConfig.ViewModels.Response response;
            string msg = "";
            switch (resp.ErrorCode) {
                case VimeoErrorCode.NoError:
                    //nothing to do here. Honestly, the app should not have called the error action
                    response = _utilsServices.GetResponse(ResponseType.None, "");
                    break;
                case VimeoErrorCode.GenericError:
                    //Something happened, but we do not know what.
                    //Just log this
                    response = _utilsServices.GetResponse(ResponseType.None, "");
                    break;
                case VimeoErrorCode.UserStopped:
                    //The user stopped the upload, with no intention of resuming it.
                    //clear the records and destroy the MediaPart we were creating
                    msg = _vimeoUploadServices.DestroyUpload(mpId);
                    response = _utilsServices.GetResponse(ResponseType.Success, msg);
                    break;
                case VimeoErrorCode.UploadStopped:
                    //The upload stopped for an error, and there is no way to resume it.
                    //clear the records and destroy the MediaPart we were creating
                    msg = _vimeoUploadServices.DestroyUpload(mpId);
                    response = _utilsServices.GetResponse(ResponseType.Success, msg);
                    break;
                case VimeoErrorCode.UploadMayResume:
                    //The upload stopped for an error, but may resume later.
                    //Do not clear or destroy anything.
                    response = _utilsServices.GetResponse(ResponseType.None, "");
                    break;
                default:
                    //No reason why we should be here
                    response = _utilsServices.GetResponse(ResponseType.None, "");
                    break;
            }

            Logger.Error(msgJson);
            if (!string.IsNullOrWhiteSpace(msg))
                Logger.Information(msg);

            return response;
        }
    }



    //We extend Laser.Orchard.StartupConfig.ViewModels.Response because we have specific error codes for Vimeo
    public enum VimeoErrorCode { NoError = 0, GenericError = 1, UserStopped = 3001, UploadStopped = 3002, UploadMayResume = 3003 }
    public class VimeoResponse : Laser.Orchard.StartupConfig.ViewModels.Response {
        new public VimeoErrorCode ErrorCode { get; set; }

        public VimeoResponse() {
            this.ErrorCode = VimeoErrorCode.GenericError;
            this.Success = false;
            this.Message = "Generic Error";
            this.ResolutionAction = ResolutionAction.NoAction;
        }
    }
}