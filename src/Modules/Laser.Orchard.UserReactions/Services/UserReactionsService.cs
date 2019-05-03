using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard;
using Orchard.Data;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.UserReactions.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using System.Web.Script.Serialization;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security.Permissions;
using Orchard.Roles.Services;
using Orchard.Workflows.Services;

namespace Laser.Orchard.UserReactions.Services {

    public interface IUserReactionsService : IDependency {
        IQueryable<UserReactionsTypesRecord> GetTypesTable();
        UserReactionsTypes GetTypesTableWithStyles();
        List<UserReactionsVM> GetTot(UserReactionsPart part);
        UserReactionsVM CalculateTypeClick(int IconType, int CurrentPage);
        ReactionsSummaryVM GetSummaryReaction(int CurrentPage);
        UserReactionsPartSettings GetSettingPart(UserReactionsPartSettings Model);
        LocalizedString GetReactionEnumTranslations(ReactionsNames reactionName);
        List<UserReactionsClickRecord> GetListTotalReactions(int Content);
        bool HasPermission(string contentType);
        IQueryable<UserReactionsTypesRecord> GetTypesTableFiltered();
        void NormalizeAllSummaries();
    }

    //Class definition to user type
    /// <summary>
    /// 
    /// </summary>
    public class UserReactionsService : IUserReactionsService {
        private readonly IRepository<UserReactionsTypesRecord> _repoTypes;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRepository<UserReactionsSummaryRecord> _repoSummary;
        private readonly IRepository<UserReactionsClickRecord> _repoClick;
        private readonly IClock _clock;
        private readonly IRepository<UserPartRecord> _repoUser;
        private readonly IRepository<UserReactionsPartRecord> _repoPartRec;
        private readonly IOrchardServices _orchardServices;
        private readonly IRoleService _roleService;
        private readonly IWorkflowManager _workflowManager;
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repoTypes"></param>
        /// <param name="repoTot"></param>
        /// <param name="repoClick"></param>
        /// <param name="authenticationService"></param>
        /// <param name="clock"></param>
        /// <param name="repoUser"></param>
        /// <param name="repoPartRec"></param>
        /// <param name="repoSummary"></param>
        public UserReactionsService(IRepository<UserReactionsTypesRecord> repoTypes,
                                    IRepository<UserReactionsClickRecord> repoClick,
                                    IAuthenticationService authenticationService,
                                    IClock clock,
                                    IRepository<UserPartRecord> repoUser,
                                    IRepository<UserReactionsPartRecord> repoPartRec,
                                    IRepository<UserReactionsSummaryRecord> repoSummary,
                                    IOrchardServices orchardServices,
                                    IRoleService roleService,
                                    IWorkflowManager workflowManager) {
            _repoTypes = repoTypes;
            _authenticationService = authenticationService;
            _repoClick = repoClick;
            _clock = clock;
            _repoUser = repoUser;
            _repoPartRec = repoPartRec;
            _repoSummary = repoSummary;
            _orchardServices = orchardServices;
            _roleService = roleService;
            _workflowManager = workflowManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserReactionsTypesRecord> GetTypesTable() {

            return _repoTypes.Table.OrderBy(o => o.Priority);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserReactionsTypesRecord> GetTypesTableFiltered() {

            return _repoTypes.Table.Where(z => z.Activating == true).OrderBy(o => o.Priority);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserReactionsTypesRecord> GetTypesTableFilteredByTypeReactions(List<UserReactionsSettingTypesSel> typeSettingsReactions) {

            IList<UserReactionsTypesRecord> typeReactionsSelected = _repoTypes.Table.Where(z => z.Activating == true).OrderBy(o => o.Priority).ToList();
            int[] ids = null;
            ids = typeSettingsReactions.Where(s => s.checkReaction == true).Select(s => s.Id).ToArray();
            typeReactionsSelected = typeReactionsSelected.Where(w => (ids.Contains(w.Id))).ToList();

            return typeReactionsSelected.AsQueryable();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public UserReactionsPartSettings GetSettingPart(UserReactionsPartSettings Model) {
            UserReactionsPartSettings retval = new UserReactionsPartSettings();
            IQueryable<UserReactionsTypesRecord> repotypesAll = _repoTypes.Table.Where(z => z.Activating == true && z.TypeName != null).OrderBy(o => o.Priority);

            List<UserReactionsSettingTypesSel> partSelectedAll = repotypesAll.Select(r => new UserReactionsSettingTypesSel {
                Id = r.Id,
                nameReaction = r.TypeName,
                checkReaction = false

            }).ToList();

            List<UserReactionsSettingTypesSel> viewmodel;
            List<UserReactionsSettingTypesSel> TypeReactionsPartsModel = new List<UserReactionsSettingTypesSel>();
            TypeReactionsPartsModel = Model.TypeReactionsPartsSelected;

            if (TypeReactionsPartsModel.Count() == 0)
                viewmodel = partSelectedAll;
            else
                viewmodel = Model.TypeReactionsPartsSelected.Except(partSelectedAll).ToList();

            retval.TypeReactionsPartsSelected = viewmodel;
            retval.Filtering = Model.Filtering;
            retval.UserChoiceBehaviour = Model.UserChoiceBehaviour;
            return retval;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public UserReactionsTypes GetTypesTableWithStyles() {
            var reactionSettings = _orchardServices.WorkContext.CurrentSite.As<UserReactionsSettingsPart>();
            var userRT = new UserReactionsTypes();
            var styleAcronime = new Laser.Orchard.UserReactions.StyleAcroName();
            userRT.CssName = reactionSettings.StyleFileNameProvider;
            userRT.AllowMultipleChoices = reactionSettings.AllowMultipleChoices;

            userRT.UserReactionsType = GetTypesTable().Select(r => new UserReactionsTypeVM {
                Id = r.Id,
                Priority = r.Priority,
                TypeName = r.TypeName,
                Activating = r.Activating,
                Delete = false
            }).ToList();

            int newPriority = userRT.UserReactionsType.Count + 1;
            foreach (var type in Enum.GetNames(typeof(ReactionsNames))) {
                if (userRT.UserReactionsType.FirstOrDefault(x => x.TypeName == type) == null) {
                    userRT.UserReactionsType.Add(new UserReactionsTypeVM {
                        Id = 0,
                        Activating = false,
                        Delete = false,
                        Priority = newPriority,
                        TypeName = type
                    });
                    newPriority++;
                }
            }
            return userRT;
        }

        /// <summary>
        /// ClickTable ordered by date descending.
        /// If there are records with same date, uses Id to order descending.
        /// </summary>
        /// <returns></returns>
        private IQueryable<UserReactionsClickRecord> GetOrderedClickTable() {
            return _repoClick.Table.OrderByDescending(o => o.CreatedUtc).OrderByDescending(o => o.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IUser CurrentUser() {
            return _authenticationService.GetAuthenticatedUser();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private ReactionsUserIds GetReactionsUserIds(IUser user) {
            ReactionsUserIds ids = new ReactionsUserIds();
            string userCookie = string.Empty;

            if (user != null) {
                ids.Id = user.Id;
                ids.Guid = string.Empty;
            }
            else {
                if (HttpContext.Current.Request.Cookies["userCookie"] != null) {
                    userCookie = HttpContext.Current.Request.Cookies["userCookie"].Value.ToString();
                }
                else {
                    Guid userNameCookie = System.Guid.NewGuid();
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie("userCookie", userNameCookie.ToString()));
                    userCookie = userNameCookie.ToString();
                }
                ids.Id = 0;
                ids.Guid = userCookie;
            }
            return ids;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>       
        public List<UserReactionsVM> GetTot(UserReactionsPart part) {
            //Part
            IList<UserReactionsVM> viewmodel = new List<UserReactionsVM>();
            //settings type
            List<UserReactionsVM> listType = new List<UserReactionsVM>();

            // if content does not exist return an empty list
            if (part == null) {
                return listType;
            }

            /////////////////////
            //reaction type settings
            UserReactionsPartSettings settings = part.TypePartDefinition.Settings.GetModel<UserReactionsPartSettings>();
            bool FilterApplied = settings.Filtering;

            List<UserReactionsSettingTypesSel> SettingType = new List<UserReactionsSettingTypesSel>();

            if (part.Settings.Count > 0) {
                SettingType = new JavaScriptSerializer().Deserialize<List<UserReactionsSettingTypesSel>>(part.Settings["UserReactionsPartSettings.TypeReactionsPartsSelected"]);
            }
            /////////////////////////////////////////////////

            //Reactions type 
            // Prendi i valori delle reactions type
            if (FilterApplied == false) {
                listType = GetTypesTableFiltered()
                .Select(x => new UserReactionsVM {
                    Id = part.Id,
                    Quantity = 0,
                    TypeName = x.TypeName,
                    TypeId = x.Id,
                    OrderPriority = x.Priority,
                    Activating = x.Activating,
                }).ToList();
            }
            else {
                // prendi i valori filtrati
                listType = GetTypesTableFilteredByTypeReactions(SettingType)
                                .Select(x => new UserReactionsVM {
                                    Id = part.Id,
                                    Quantity = 0,
                                    TypeName = x.TypeName,
                                    TypeId = x.Id,
                                    OrderPriority = x.Priority,
                                    Activating = x.Activating
                                }).ToList();

            }

            /////////////////////////////////////////////////////////////////
            //Part type
            viewmodel = part.Reactions.Select(s => new UserReactionsVM {
                Id = s.Id,
                Quantity = s.Quantity,
                TypeName = s.UserReactionsTypesRecord.TypeName,
                TypeId = s.UserReactionsTypesRecord.Id,
                OrderPriority = s.UserReactionsTypesRecord.Priority,
                Activating = s.UserReactionsTypesRecord.Activating,
            }).ToList();

            List<UserReactionsVM> retData = new List<UserReactionsVM>();

            foreach (UserReactionsVM itemTypeReactions in listType) {
                UserReactionsVM totItem = itemTypeReactions;
                UserReactionsVM viewModel = viewmodel.FirstOrDefault(z => z.TypeId.Equals(itemTypeReactions.TypeId));

                if (viewModel != null) {
                    totItem.Quantity = viewModel.Quantity;
                }
                retData.Add(totItem);
            }

            return retData;
        }

        private Permission GetPermissionByName(string permission) {
            if (!string.IsNullOrEmpty(permission)) {
                var listpermissions = _roleService.GetInstalledPermissions().Values;
                foreach (IEnumerable<Permission> sad in listpermissions) {
                    foreach (Permission perm in sad) {
                        if (perm.Name == permission) {
                            return perm;
                        }
                    }
                }
            }
            return null;
        }

        /// <param name="CurrentUser"></param>
        /// <param name="IconType"></param>
        /// <param name="CurrentPage"></param>
        /// <returns></returns>
        public ReactionsSummaryVM GetSummaryReaction(int CurrentPage) {
            ReactionsSummaryVM result = new ReactionsSummaryVM();
            IUser userId = this.CurrentUser();
            UserReactionsClickRecord res = new UserReactionsClickRecord();
            string userCookie = string.Empty;
            var part = _orchardServices.ContentManager.Get<UserReactionsPart>(CurrentPage);
            if (part != null) {
                var items = GetTot(part);
                ReactionsUserIds reactionsCurrentUser = new ReactionsUserIds();
                reactionsCurrentUser = GetReactionsUserIds(userId);
                List<UserReactionsVM> newSommaryRecord = new List<UserReactionsVM>();
                foreach (UserReactionsVM item in items) {
                    int IconType = item.TypeId;
                    //Verifica che non sia già stato eseguito un click 
                    if (reactionsCurrentUser.Id > 0) {
                        res = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id.Equals(IconType) && w.UserPartRecord.Id.Equals(reactionsCurrentUser.Id) && w.ContentItemRecordId.Equals(CurrentPage)).FirstOrDefault();
                    }
                    else {
                        userCookie = reactionsCurrentUser.Guid;
                        res = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id.Equals(IconType) && w.UserGuid.Equals(userCookie) && w.ContentItemRecordId.Equals(CurrentPage)).FirstOrDefault();
                    }

                    if (res != null)
                        item.Clicked = res.ActionType;

                    newSommaryRecord.Add(item);
                }
                result.ContentId = part.ContentItem.Id;
                if (reactionsCurrentUser.Id != 0) {
                    result.UserAuthenticated = true;
                }
                if (HasPermission(part.ContentItem.ContentType)) {
                    result.UserAuthorized = true;
                }
                result.Reactions = newSommaryRecord.ToArray();
            }
            return result;
        }


        public List<UserReactionsClickRecord> GetListTotalReactions(int Content) {
            var retVal = GetOrderedClickTable().Where(z => z.ContentItemRecordId == Content).ToList();
            retVal.Reverse();
            return retVal;
        }



        /// <param name="iconTypeId"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public UserReactionsVM CalculateTypeClick(int iconTypeId, int pageId) {
            UserReactionsClickRecord previousState = new UserReactionsClickRecord();
            UserReactionsVM retVal = new ViewModels.UserReactionsVM();
            int actionType = 1;
            bool previouslyClicked = false;

            //Verifica user
            IUser currentUser = CurrentUser();
            ReactionsUserIds userIds = GetReactionsUserIds(currentUser);
            var contentItem = _orchardServices.ContentManager.Get(pageId);

            if (HasPermission(contentItem.ContentType)) {
                //Verifica che non sia già stato eseguito un click
                var elencoUserReactions = GetUserReactions(pageId, userIds);
                previousState = elencoUserReactions.FirstOrDefault(x => x.UserReactionsTypesRecord.Id == iconTypeId);

                //Se già cliccato quella reaction
                if (previousState != null) {
                    previouslyClicked = true;
                    if (previousState.ActionType == 1) {
                        // se era cliccato allora diventa unclicked
                        actionType = -1;
                    }
                }

                //Salva i dati
                try {
                    UserReactionsTypesRecord reactType = GetTypesTable().Where(w => w.Id.Equals(iconTypeId)).FirstOrDefault();
                    InsertClick(pageId, userIds, actionType, reactType);
                    int qty = UpdateSummary(pageId, userIds, actionType, reactType, previouslyClicked);

                    //gestisce la scelta esclusiva, se richiesto
                    bool isExclusive = IsExclusive(contentItem.ContentType);
                    if (isExclusive && actionType == 1) {
                        // cerca tutti i clicked diversi da quello corrente per lo stesso utente e la stessa pagina
                        var clicked = GetClickedReactions(elencoUserReactions);

                        foreach (var reaction in clicked) {
                            // non agisce sulla reaction appena cliccata
                            if (reaction.Id != reactType.Id) {
                                InsertClick(pageId, userIds, -1, reaction);
                                UpdateSummary(pageId, userIds, -1, reaction);
                            }
                        }
                    }

                    retVal.Clicked = 1;
                    retVal.Quantity = qty;
                    retVal.TypeId = iconTypeId;
                    retVal.Id = pageId;

                    //solleva l'evento per il workflow
                    _workflowManager.TriggerEvent("ReactionClicked", contentItem, () => new Dictionary<string, object> {
                        { "Content", contentItem },
                        { "ReactionId", iconTypeId },
                        { "Action", actionType },
                        { "UserEmail", (currentUser != null ? currentUser.Email : "") },
                        { "UserId", (currentUser != null ? currentUser.Id : 0) }
                    });
                }
                catch (Exception) {
                    retVal.Clicked = 5;
                }
            }
            else {
                // l'utente non ha le permission
                retVal.Clicked = 1;
                retVal.Quantity = 0;
                retVal.TypeId = iconTypeId;
                retVal.Id = pageId;
            }
            return retVal;
        }
        public LocalizedString GetReactionEnumTranslations(ReactionsNames reactionName) {
            if (reactionName.Equals(ReactionsNames.angry)) {
                return T("Angry");
            }
            else if (reactionName.Equals(ReactionsNames.boring)) {
                return T("Boring");
            }
            else if (reactionName.Equals(ReactionsNames.exhausted)) {
                return T("Exhausted");
            }
            else if (reactionName.Equals(ReactionsNames.happy)) {
                return T("Happy");
            }
            else if (reactionName.Equals(ReactionsNames.like)) {
                return T("I Like");
            }
            else if (reactionName.Equals(ReactionsNames.iwasthere)) {
                return T("I Was There");
            }
            else if (reactionName.Equals(ReactionsNames.joke)) {
                return T("Joke");
            }
            else if (reactionName.Equals(ReactionsNames.kiss)) {
                return T("Kiss");
            }
            else if (reactionName.Equals(ReactionsNames.love)) {
                return T("Love");
            }
            else if (reactionName.Equals(ReactionsNames.pain)) {
                return T("Pain");
            }
            else if (reactionName.Equals(ReactionsNames.sad)) {
                return T("Sad");
            }
            else if (reactionName.Equals(ReactionsNames.shocked)) {
                return T("Shocked");
            }
            else if (reactionName.Equals(ReactionsNames.silent)) {
                return T("Silent");
            }
            else if (reactionName.Equals(ReactionsNames.excited)) {
                return T("Excited");
            }
            else if (reactionName.Equals(ReactionsNames.curious)) {
                return T("Curious");
            }
            else if (reactionName.Equals(ReactionsNames.interested)) {
                return T("Interested");
            }
            else {
                return T("None");
            }
        }

        public bool HasPermission(string contentType) {
            bool result = false;
            Permission permissionToTest = GetPermissionByName("ReactionsFor" + contentType);
            result = _orchardServices.Authorizer.Authorize(permissionToTest);
            return result;
        }

        public void NormalizeAllSummaries() {
            // recupera tutte le reaction type, anche quelle non abilitate
            var elencoTypes = GetTypesTable();

            // recupera tutti i contenuti con le reactions
            var elencoContenuti = _repoPartRec.Fetch(x => x.Id > 0);

            // controlla che esista un valore per ogni reaction type, se non c'è inserisce zero
            foreach (var record in elencoContenuti) {
                var elencoSummary = _repoSummary.Fetch(x => x.UserReactionsPartRecord.Id == record.Id);
                foreach (var type in elencoTypes) {
                    if (elencoSummary.FirstOrDefault(x => x.UserReactionsTypesRecord.Id == type.Id) == null) {
                        _repoSummary.Create(new UserReactionsSummaryRecord {
                            Quantity = 0,
                            UserReactionsPartRecord = record,
                            UserReactionsTypesRecord = type
                        });
                    }
                }
            }
        }
        /// <summary>
        /// Crea nuovo record dati click.
        /// </summary>
        private void InsertClick(int pageId, ReactionsUserIds reactionsUserIds, int actionType, UserReactionsTypesRecord reactType) {
            UserPartRecord userRec = null;
            string guid = null;
            UserReactionsClickRecord clickRecord = new UserReactionsClickRecord();
            clickRecord.CreatedUtc = _clock.UtcNow;
            clickRecord.ContentItemRecordId = pageId;
            clickRecord.ActionType = actionType;
            clickRecord.UserReactionsTypesRecord = reactType;
            if (reactionsUserIds.Id > 0) {
                userRec = _repoUser.Table.Where(w => w.Id.Equals(reactionsUserIds.Id)).FirstOrDefault();
            }
            else {
                guid = reactionsUserIds.Guid;
            }
            clickRecord.UserPartRecord = userRec;
            clickRecord.UserGuid = guid;
            _repoClick.Create(clickRecord);
        }
        private int UpdateSummary(int pageId, ReactionsUserIds reactionsUserIds, int actionType, UserReactionsTypesRecord reactType, bool previouslyClicked = false) {
            UserReactionsSummaryRecord summaryRecord = null;
            UserReactionsPartRecord reactionsPart = null;
            //Verifica che ci sia già un record cliccato per quell' icona in quel documento
            summaryRecord = _repoSummary.Table.Where(z => z.UserReactionsTypesRecord.Id == reactType.Id && z.UserReactionsPartRecord.Id == pageId).FirstOrDefault();

            // se 0 record aggiungi il record
            if (summaryRecord == null) {
                //Create
                summaryRecord = new UserReactionsSummaryRecord();
                reactionsPart = _repoPartRec.Table.FirstOrDefault(z => z.Id.Equals(pageId));
                summaryRecord.Quantity = 1;
                summaryRecord.UserReactionsTypesRecord = reactType;
                summaryRecord.UserReactionsPartRecord = reactionsPart;
                _repoSummary.Create(summaryRecord);

                if (previouslyClicked) {
                    Logger.Error("UserReactionsService.UpdateSummary -> Missing summary record!");
                }
            }
            else {
                // Va in update ed aggiorna il campo Quantity
                if (actionType == 1) {
                    summaryRecord.Quantity++;
                }
                else {
                    summaryRecord.Quantity--;
                }
                _repoSummary.Update(summaryRecord);
            }
            return summaryRecord.Quantity;
        }
        private bool IsExclusive(string contentType) {
            bool result = false;
            var ctypeDefinition = _orchardServices.ContentManager.GetContentTypeDefinitions().Where(x => x.Name == contentType).FirstOrDefault();
            var part = ctypeDefinition.Parts.FirstOrDefault(x => x.PartDefinition.Name == "UserReactionsPart");
            var partSetting = part.Settings.FirstOrDefault(x => x.Key == "UserReactionsPartSettings.UserChoiceBehaviour");
            if (partSetting.Value == "RestrictToSingle") {
                result = true;
            }
            else if (partSetting.Value == "Inherit" || string.IsNullOrWhiteSpace(partSetting.Value)) {
                var globalSettings = _orchardServices.WorkContext.CurrentSite.As<UserReactionsSettingsPart>();
                if (globalSettings.AllowMultipleChoices == false) {
                    result = true;
                }
            }
            return result;
        }
        private List<UserReactionsClickRecord> GetUserReactions(int pageId, ReactionsUserIds userIds) {
            if (userIds.Id == 0) {
                return GetOrderedClickTable().Where(
                    x => userIds.Guid != null && x.UserGuid == userIds.Guid && x.ContentItemRecordId == pageId).ToList();
            }
            else {
                return GetOrderedClickTable().Where(
                    x => x.UserPartRecord.Id == userIds.Id && x.ContentItemRecordId == pageId).ToList();
            }
        }
        private List<UserReactionsTypesRecord> GetClickedReactions(List<UserReactionsClickRecord> reactions) {
            List<UserReactionsTypesRecord> clicked = new List<UserReactionsTypesRecord>();
            List<UserReactionsTypesRecord> unclicked = new List<UserReactionsTypesRecord>();
            foreach (var item in reactions) {
                if (clicked.Contains(item.UserReactionsTypesRecord) == false
                    && unclicked.Contains(item.UserReactionsTypesRecord) == false) {
                    if (item.ActionType == 1) {
                        clicked.Add(item.UserReactionsTypesRecord);
                    }
                    else {
                        unclicked.Add(item.UserReactionsTypesRecord);
                    }
                }
            }
            return clicked;
        }
    }
}
