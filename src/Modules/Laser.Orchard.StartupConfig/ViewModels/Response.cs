using Orchard.Localization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;


namespace Laser.Orchard.StartupConfig.ViewModels {

    /// <summary>
    /// per user enum range 1000
    /// </summary>
    public enum ErrorCode { NoError = 0, GenericError = 1, ToConfirmEmail = 1003, InvalidUser = 1000, InvalidXSRF = 1001, UnAuthorized = 1002, Validation = 2000, MissingPolicies = 3000, MissingParameters = 4000 }

    /// <summary>
    /// per user enum range 1000
    /// </summary>
    public enum ResolutionAction { NoAction = 0, Login = 1000, ToConfirmEmail = 1003, AcceptPolicies = 3000, AddParameter = 4000 }

    public enum ResponseType { None = 0, Success = 1, ToConfirmEmail = 2 , InvalidUser = 1000, InvalidXSRF = 1001, UnAuthorized = 1002, Validation = 2000, MissingPolicies = 3000, MissingParameters = 4000 }


    public class Response {
        public bool Success { get; set; }

        public string Message { get; set; }

        public dynamic Data { get; set; }

        public ErrorCode ErrorCode { get; set; }

        public ResolutionAction ResolutionAction { get; set; }

        public Response() {
            this.ErrorCode = ErrorCode.GenericError;
            this.Success = false;
            this.Message = "Generic Error";
            this.ResolutionAction = ResolutionAction.NoAction;
        }
        //public Response()
        //    : this(ResponseType.None) {
           
        //}

        //public Response(ResponseType ResponseType, dynamic data = null) {


        //    this.ErrorCode = ErrorCode.GenericError;
        //    this.Success = false;
        //    this.Message = "Generic Error";
        //    switch (ResponseType) {
        //        case ResponseType.Success:
        //            this.Success = true;
        //            this.Message = "Successfully Executed";
        //            this.ErrorCode = ErrorCode.NoError;
        //            this.Data = data;
        //            this.ResolutionAction = ResolutionAction.NoAction;
        //            break;

        //        case ResponseType.InvalidUser:
        //            this.Success = false;
        //            this.Message = "Invalid User";
        //            this.ErrorCode = ErrorCode.InvalidUser;
        //            this.Data = data;
        //            this.ResolutionAction = ResolutionAction.Login;
        //            break;

        //        case ResponseType.InvalidXSRF:
        //            this.Success = false;
        //            this.Message = "Invalid Token/csrfToken";
        //            this.ErrorCode = ErrorCode.InvalidXSRF;
        //            this.Data = data;
        //            this.ResolutionAction = ResolutionAction.Login;
        //            break;

        //        case ResponseType.Validation:
        //            this.Success = false;
        //            this.Message = "Validation error";
        //            this.ErrorCode = ErrorCode.Validation;
        //            this.Data = data;
        //            this.ResolutionAction = ResolutionAction.NoAction;
        //            break;

        //        case ResponseType.UnAuthorized:
        //            this.Success = false;
        //            this.Message = "UnAuthorized Action";
        //            this.ErrorCode = ErrorCode.UnAuthorized;
        //            this.Data = data;
        //            this.ResolutionAction = ResolutionAction.NoAction;
        //            break;
        //    }
        //}
    }
}