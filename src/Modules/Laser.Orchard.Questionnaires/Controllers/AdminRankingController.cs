using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace Laser.Orchard.Questionnaires.Controllers {
    public class AdminRankingController : Controller {
        private readonly IOrchardServices _orchardServices;
        //metti come parametro del costruttore IQuestionnairesServices questionnairesServices e
        //orchard si occupa di andare ad iniettarlo in maniera corretta, quindi posso poi fare
        //un bottone per mandarmi una mail
        private readonly IQuestionnairesServices _questionnairesServices;
        private readonly IRepository<RankingPartRecord> _repoRanking;
        private readonly ITransactionManager _transactionManager;
        public AdminRankingController(IOrchardServices orchardServices, IQuestionnairesServices questionnairesServices, 
            IRepository<RankingPartRecord> repoRanking, ITransactionManager transactionManager) {
            _orchardServices = orchardServices;
            _questionnairesServices = questionnairesServices;
            _repoRanking = repoRanking;
            _transactionManager = transactionManager;
        }

        [Admin]
        public ActionResult TestEmail(Int32 ID) {
            _questionnairesServices.SendTemplatedEmailRanking(ID);
            return RedirectToAction("Index");
        }
      
        [Admin]
        public ActionResult GetList() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                return new HttpUnauthorizedResult();
            var query = _orchardServices.ContentManager.Query();
            // la condizione Id > 0 nelle due righe seguenti serve solo a forzare la join con le tabelle GamePartRecord e RankingPartRecord rispettivamente, perché la clausola ForPart non è sufficiente (fa solo scansione in memoria)
            var list = query.ForPart<GamePart>().Where<GamePartRecord>(x => x.Id > 0).List();
            var listranking = _orchardServices.ContentManager.Query().ForPart<RankingPart>().Where<RankingPartRecord>(x => x.Id > 0).List();
            List<DisplaRankingTemplateVM> listaAllRank = new List<DisplaRankingTemplateVM>();
            foreach (GamePart gp in list) {

                ContentItem Ci = gp.ContentItem;
                string titolo = Ci.As<TitlePart>().Title;
                var listordered= listranking.Where(z => z.As<RankingPart>().ContentIdentifier == Ci.Id && z.As<RankingPart>().Device==TipoDispositivo.Apple ).OrderByDescending(y => y.Point);
                List<RankingTemplateVM> rkt = new List<RankingTemplateVM>();
                foreach (RankingPart cirkt in listordered) {
                    RankingTemplateVM tmp = new RankingTemplateVM();
                    tmp.Point = cirkt.Point;
                    tmp.ContentIdentifier = cirkt.ContentIdentifier;
                    tmp.Device = cirkt.Device;
                    tmp.Identifier = cirkt.Identifier;
                    tmp.name = getusername(cirkt.User_Id);
                    tmp.UsernameGameCenter = cirkt.UsernameGameCenter;
                    tmp.AccessSecured = cirkt.AccessSecured;
                    tmp.RegistrationDate = cirkt.RegistrationDate;
                    rkt.Add(tmp);
                }
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo + " Apple", ListRank = rkt });
               
                listordered = listranking.Where(z => z.As<RankingPart>().ContentIdentifier == Ci.Id && z.As<RankingPart>().Device == TipoDispositivo.Android).OrderByDescending(y => y.Point);
                rkt = new List<RankingTemplateVM>();
                foreach (RankingPart cirkt in listordered) {
                    RankingTemplateVM tmp = new RankingTemplateVM();
                    tmp.Point = cirkt.Point;
                    tmp.ContentIdentifier = cirkt.ContentIdentifier;
                    tmp.Device = cirkt.Device;
                    tmp.Identifier = cirkt.Identifier;
                    tmp.name = getusername(cirkt.User_Id);
                    tmp.UsernameGameCenter = cirkt.UsernameGameCenter;
                    tmp.AccessSecured = cirkt.AccessSecured;
                    tmp.RegistrationDate = cirkt.RegistrationDate;
                    rkt.Add(tmp);
                }
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo + " Android", ListRank = rkt });

                listordered = listranking.Where(z => z.As<RankingPart>().ContentIdentifier == Ci.Id && z.As<RankingPart>().Device == TipoDispositivo.WindowsMobile).OrderByDescending(y => y.Point);
                rkt = new List<RankingTemplateVM>();
                foreach (RankingPart cirkt in listordered) {
                    RankingTemplateVM tmp = new RankingTemplateVM();
                    tmp.Point = cirkt.Point;
                    tmp.ContentIdentifier = cirkt.ContentIdentifier;
                    tmp.Device = cirkt.Device;
                    tmp.Identifier = cirkt.Identifier;
                    tmp.name = getusername(cirkt.User_Id);
                    tmp.UsernameGameCenter = cirkt.UsernameGameCenter;
                    tmp.AccessSecured = cirkt.AccessSecured;
                    tmp.RegistrationDate = cirkt.RegistrationDate;
                    rkt.Add(tmp);
                }
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo+" Windows Mobile", ListRank = rkt });
            }


            return View((object)listaAllRank);











            //var AllRecord = _PushNotificationService.SearchPushNotification(search.Expression);
            //var totRecord = AllRecord.Count();
            //Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            //dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(totRecord);

            //// Generate a list of shapes, restricting by pager parameters
            //var list = _orchardServices.New.List();
            //list.AddRange(AllRecord.Skip(pager.GetStartIndex())
            //                    .Take(pager.PageSize)
            //    // .Select(r => _orchardService.ContentManager.BuildDisplay(r, "ciao"))
            //                    );
            ////   (object) new model {Orders: list, Pager: pagerShape, Admn: hasPermission};

            ////var model = Shape.Orders(Orders: list, Pager: pagerShape, Admn: hasPermission, OrderPayedCount: countOrdersNew, Search: search);
            //var model = new PushIndex(list, search, pagerShape);

            //return View((object)model);
            ////return View((object)new {
            ////    Orders = list,
            ////    Pager = pagerShape,
            ////    Admn = hasPermission
            ////});
        }
        //The GelistSingleGame methods get the rankings for a single name, identified by its ID, and for a single Device.
        //For any user (identified by its phone number) only one score is in the output of the method.
        //TODO: Currently, DB accesses and list manipulations are done separately, but they should be merged into a single query to decrease data transfer between app and DB
        [HttpGet]
        [Admin]
        public ActionResult GetListSingleGame(int ID, int? page, int? pageSize, string deviceType = "General", bool ascending = false) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.GameRanking)) //(Permissions.AccessStatistics)) //(StandardPermissions.SiteOwner)) //
                return new HttpUnauthorizedResult();
            return GetListSingleGame(ID, new PagerParameters {
                Page = page, PageSize = pageSize
            }, DeviceType: deviceType, Ascending: ascending);
        }
        [HttpPost]
        [Admin]
        public ActionResult GetListSingleGame(int ID, PagerParameters pagerParameters, string DeviceType = "General", bool Ascending = false) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.GameRanking)) //(Permissions.AccessStatistics)) //(StandardPermissions.SiteOwner)) //
                return new HttpUnauthorizedResult();

            if (pagerParameters.PageSize == null)
                pagerParameters.PageSize = _orchardServices.WorkContext.CurrentSite.PageSize;
            if (pagerParameters.Page == null)
                pagerParameters.Page = 1;
            

            var query = _orchardServices.ContentManager.Query();
            var list = query.ForPart<GamePart>().Where<GamePartRecord>(x => x.Id == ID).List(); //list all games with the selected ID (should be only one)
            GamePart gp = list.FirstOrDefault(); //the game for which we want the rankings
            //Assuming there was no issues, gp should never be null. If gp is null, it probably means something happened in the DB, since we
            //read the ID from the DB to create the "caller" page, and the we read again in this method.
            ContentItem Ci = gp.ContentItem;
            string titolo = Ci.As<TitlePart>().Title;

            string devString = "General";
            //query to get the ranking out of the db, already sorted and paged, with multiple scores by a same user removed
            if (DeviceType == TipoDispositivo.Apple.ToString()) {
                devString = TipoDispositivo.Apple.ToString();
            } else if (DeviceType == TipoDispositivo.Android.ToString()) {
                devString = TipoDispositivo.Android.ToString();
            } else if (DeviceType == TipoDispositivo.WindowsMobile.ToString()) {
                devString = TipoDispositivo.WindowsMobile.ToString();
            }
            List<RankingTemplateVM> lRanka = _questionnairesServices.QueryForRanking(ID, devString, pagerParameters.Page.Value, pagerParameters.PageSize.Value, Ascending);

            var session = _transactionManager.GetSession();//_sessionLocator.For(typeof(RankingPartRecord));
            string queryString = "SELECT COUNT(DISTINCT Identifier) "
                + "FROM Laser.Orchard.Questionnaires.Models.RankingPartRecord as rpr "
                + "WHERE rpr.ContentIdentifier=" + ID + " ";

            if (DeviceType == TipoDispositivo.Apple.ToString()) {
                queryString += "AND rpr.Device = '" + TipoDispositivo.Apple + "' ";
            } else if (DeviceType == TipoDispositivo.Android.ToString()) {
                queryString += "AND rpr.Device = '" + TipoDispositivo.Android + "' ";
            } else if (DeviceType == TipoDispositivo.WindowsMobile.ToString()) {
                queryString += "AND rpr.Device = '" + TipoDispositivo.WindowsMobile + "' ";
            }
            var countQuery = session.CreateQuery(queryString);
            //var asd = countQuery.List(); // countQuery.UniqueResult();
            int scoresCount = (int)(countQuery.UniqueResult<long>());

            //create and initialize pager
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(scoresCount);
            int listStart = pager.GetStartIndex();
            int listEnd = listStart + ((pager.PageSize > scoresCount) ? scoresCount : pager.PageSize);
            listEnd = listEnd > scoresCount ? scoresCount : listEnd;
            DisplaRankingTemplateVM pageOfScores = new DisplaRankingTemplateVM {
                Title = titolo,
                GameID = ID,
                Device = devString,
                ListRank = lRanka //innerquery
            };

            var model = new DisplayRankingTemplateVMModel();
            model.Pager = pagerShape;
            model.drtvm = pageOfScores;

            return View((object)model); //((object)listaAllRank);
        }
        private string getusername(int id) {
            if (id > 0) {
                try {
                    return ((dynamic)_orchardServices.ContentManager.Get(id)).UserPart.UserName;
                }
                catch (Exception) {
                    return "No User";
                }
            }
            else
                return "No User";
        }

        //Adding functionality to list all games (published and unpublished)
        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, string searchExpression) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.GameRanking)) //(Permissions.AccessStatistics)) //
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, searchExpression);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, string searchExpression) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.GameRanking)) //(Permissions.AccessStatistics)) //
                return new HttpUnauthorizedResult();

            IContentQuery<ContentItem> contentQuery =
            //IContentQuery<GamePart> contentQuery =
                _orchardServices.ContentManager.Query()
                                               //.ForPart<GamePart>();
                                               .ForType("Game")
                                               .OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);

            if (!string.IsNullOrEmpty(searchExpression))
                contentQuery = contentQuery.Where<TitlePartRecord>(w => w.Title.Contains(searchExpression));

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(contentQuery.Count());
            var pageOfContentItems = contentQuery.Slice(pager.GetStartIndex(), pager.PageSize);

            var model = new GamePartSearchViewModel();
            model.Pager = pagerShape;
            model.GameParts = pageOfContentItems;

            return View((object)model);
        }
    }
}