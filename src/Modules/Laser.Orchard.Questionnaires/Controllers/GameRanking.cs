using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.Settings;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization.Models;
using Orchard.Logging;
using Orchard.UI.Admin;
using System;
using System.Web.Http;

namespace Laser.Orchard.Questionnaires.Controllers {
    [WebApiKeyFilter(false)]
    public class GameRanking : ApiController {
        private readonly IOrchardServices _orchardServices;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IQuestionnairesServices _questionnairesServices;
        private readonly IUtilsServices _utilsServices;

        public ILogger Logger { get; set; }

        public GameRanking(IOrchardServices orchardServices, ICsrfTokenHelper csrfTokenHelper, IQuestionnairesServices questionnairesServices, IUtilsServices utilsServices) {
            Logger = NullLogger.Instance;
            _orchardServices = orchardServices;
            _csrfTokenHelper = csrfTokenHelper;
            _questionnairesServices = questionnairesServices;
            _utilsServices = utilsServices;
        }

        // METODO GET USATO PER AVVIARE MANUALMENTE IL METODO GENERATO DAL TRIGGER DELL'INVIO EMAIL  E WORKFLOW
        //public void Get() {
        //    bool b = _questionnairesServices.SendTemplatedEmailRanking();
        //}

        /// <summary>
        ///  esempio di chiamata
        ///  http://localhost/Orchard.Community/testgame/api/laser.orchard.questionnaires/GameRanking
        /// parametri {"Point":12,"Identifier":"io","UsernameGameCenter":"123","Device":"Apple","ContentIdentifier":22}
        /// Device è un enumeratore Android, Apple, WindowsMobile
        /// </summary>
        /// <param name="Risp"></param>
        /// <returns></returns>
        public Response Post([FromBody] RankingVM Risp) {
            //HttpResponseMessage Response = new HttpResponseMessage();
            //foreach (string coo in _orchardServices.WorkContext.HttpContext.Request.Cookies) {
            //    var authCookie = _orchardServices.WorkContext.HttpContext.Request.Cookies[coo].Value;
            //    Logger.Error(coo + " " + authCookie);
            //}

            bool AccessSecured = false;
            Int32 User_Id = 0;
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                User_Id = currentUser.Id;
                //      Logger.Error("User logged= " + User_Id.ToString());
                if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                    AccessSecured = true;
                }
            }
            Int32 ContentItemToLink = Risp.ContentIdentifier;
            ContentItem cilinked = _orchardServices.ContentManager.Get(Risp.ContentIdentifier);
            if (cilinked != null) {
                if (cilinked.As<GamePart>() == null)
                    return (_utilsServices.GetResponse(ResponseType.Validation, "ContentItem with no GamePart " + Risp.ContentIdentifier.ToString()));
                else
                    if (!cilinked.As<GamePart>().Settings.GetModel<GamePartSettingVM>().Ranking) {
                        return (_utilsServices.GetResponse(ResponseType.Validation, "This Game is not enabled to ranking " + Risp.ContentIdentifier.ToString()));
                    }
                    else {
                        if (cilinked.As<LocalizationPart>() != null)
                            if (cilinked.As<LocalizationPart>().MasterContentItem != null)
                                ContentItemToLink = cilinked.As<LocalizationPart>().MasterContentItem.Id;
                    }
            }
            else {
                return (_utilsServices.GetResponse(ResponseType.Validation, "No ContentItem with id " + Risp.ContentIdentifier.ToString()));
            }
            var RankingCi = _orchardServices.ContentManager.New("Ranking");
            RankingPart rankingpart = RankingCi.As<RankingPart>();
            rankingpart.Point = Risp.Point;
            rankingpart.ContentIdentifier = ContentItemToLink;
            rankingpart.Device = Risp.Device;
            rankingpart.Identifier = Risp.Identifier;
            rankingpart.RegistrationDate = DateTime.Now; ;
            rankingpart.UsernameGameCenter = Risp.UsernameGameCenter;
            rankingpart.AccessSecured = AccessSecured;
            rankingpart.User_Id = User_Id;
            _orchardServices.ContentManager.Create(RankingCi);
            return (_utilsServices.GetResponse(ResponseType.Success));
        }


    }
}