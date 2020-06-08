using Laser.Orchard.AdvancedSearch.ViewModels;
using Laser.Orchard.StartupConfig.Localization;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Services;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Contents;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Contents.ViewModels;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Fields.Settings;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Records;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.Projections.Models;
using Orchard.Settings;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using Mvc = Orchard.Mvc;

//using System.Diagnostics;

namespace Laser.Orchard.AdvancedSearch.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionService _contentDefinitionService;

        private readonly ITransactionManager _transactionManager;
        private readonly ISiteService _siteService;
        private readonly ICultureManager _cultureManager;
        private readonly IRepository<CultureRecord> _cultureRepo;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IUserService _userService;
        private readonly INotifier _notifier;
        private readonly IDateLocalization _dataLocalization;

        private readonly IRepository<FieldIndexPartRecord> _cpfRepo;
        private readonly ILocalizationService _localizationService;
        private readonly ICommonsServices _commonService;

        public AdminController(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IContentDefinitionService contentDefinitionService,
            ITransactionManager transactionManager,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            ICultureManager cultureManager,
            IRepository<CultureRecord> cultureRepo,
            INotifier notifier,
            IUserService userService,
            IDateLocalization dataLocalization,
            ITaxonomyService taxonomyService,
            IRepository<FieldIndexPartRecord> cpfRepo,
            ILocalizationService localizationService,
            ICommonsServices commonService) {
            Services = orchardServices;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionService = contentDefinitionService;
            _transactionManager = transactionManager;
            _siteService = siteService;
            _cultureManager = cultureManager;
            _cultureRepo = cultureRepo;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
            _taxonomyService = taxonomyService;
            _dataLocalization = dataLocalization;
            _userService = userService;
            _notifier = notifier;
            _cpfRepo = cpfRepo;
            _localizationService = localizationService;
            _commonService = commonService;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [Admin]
        public ActionResult List(ListContentsViewModelExtension model, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var versionOptions = VersionOptions.Latest;
            switch (model.Options.ContentsStatus) {
                case ContentsStatus.Published:
                    versionOptions = VersionOptions.Published;
                    break;
                case ContentsStatus.Draft:
                    versionOptions = VersionOptions.Draft;
                    break;
                case ContentsStatus.AllVersions:
                    versionOptions = VersionOptions.AllVersions;
                    break;
                default:
                    versionOptions = VersionOptions.Latest;
                    break;
            }

            var query = _contentManager.Query(versionOptions, GetListableTypes(false).Select(ctd => ctd.Name).ToArray());

            //the lQuery is used only in the case where we have the language queries, but since we cannot clone IContentQuery objects,
            //we create it here and build is as we build the other
            var lQuery = _contentManager.Query(versionOptions, GetListableTypes(false).Select(ctd => ctd.Name).ToArray());

            if (!string.IsNullOrEmpty(model.TypeName)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.TypeName);
                if (contentTypeDefinition == null)
                    return HttpNotFound();

                model.TypeDisplayName = !string.IsNullOrWhiteSpace(contentTypeDefinition.DisplayName)
                                            ? contentTypeDefinition.DisplayName
                                            : contentTypeDefinition.Name;
                query = query.ForType(model.TypeName);
                lQuery = lQuery.ForType(model.TypeName);
            }

            // FILTER QUERIES: START //

            // terms query
            if (model.AdvancedOptions.SelectedTermId > 0) {
                var termId = model.AdvancedOptions.SelectedTermId;
                query = query.Join<TermsPartRecord>().Where(x => x.Terms.Any(a => a.TermRecord.Id == termId));
                lQuery = lQuery.Join<TermsPartRecord>().Where(x => x.Terms.Any(a => a.TermRecord.Id == termId));
            }

            // owner query
            if (    //user cannot see everything by default
                    (
                        !Services.Authorizer.Authorize(AdvancedSearchPermissions.SeesAllContent)
                        || (Services.Authorizer.Authorize(AdvancedSearchPermissions.SeesAllContent) && model.AdvancedOptions.OwnedByMeSeeAll)
                    ) && (//user has either limitation
                        ((Services.Authorizer.Authorize(AdvancedSearchPermissions.MayChooseToSeeOthersContent))
                            && (model.AdvancedOptions.OwnedByMe))
                        || (Services.Authorizer.Authorize(AdvancedSearchPermissions.CanSeeOwnContents)
                        && !Services.Authorizer.Authorize(AdvancedSearchPermissions.MayChooseToSeeOthersContent))
                    )
                ) {
                //this user can only see the contents they own
                var lowerName = Services.WorkContext.CurrentUser.UserName.ToLowerInvariant();
                var email = Services.WorkContext.CurrentUser.Email;
                var user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName || u.Email == email).List().FirstOrDefault();
                query = query.Join<CommonPartRecord>().Where(x => x.OwnerId == user.Id);
                lQuery = lQuery.Join<CommonPartRecord>().Where(x => x.OwnerId == user.Id);
            } else if (!String.IsNullOrWhiteSpace(model.AdvancedOptions.SelectedOwner)) {
                var lowerName = model.AdvancedOptions.SelectedOwner == null ? "" : model.AdvancedOptions.SelectedOwner.ToLowerInvariant();
                var email = model.AdvancedOptions.SelectedOwner;
                var user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName || u.Email == email).List().FirstOrDefault();
                if (user != null) {
                    query = query.Join<CommonPartRecord>().Where(x => x.OwnerId == user.Id);
                    lQuery = lQuery.Join<CommonPartRecord>().Where(x => x.OwnerId == user.Id);
                } else {
                    _notifier.Add(NotifyType.Warning, T("No user found. Ownership filter not applied."));
                }
            }

            //date query
            if (model.AdvancedOptions.SelectedFromDate != null || model.AdvancedOptions.SelectedToDate != null) {
                //set default dates for From and To if they are null.
                var fromD = _dataLocalization.StringToDatetime(model.AdvancedOptions.SelectedFromDate, "") ?? _dataLocalization.StringToDatetime("09/05/1985", "");
                var toD = _dataLocalization.StringToDatetime(model.AdvancedOptions.SelectedToDate, "") ?? DateTime.Now;

                if (model.AdvancedOptions.DateFilterType == DateFilterOptions.Created) {
                    query = query.Join<CommonPartRecord>().Where(x => x.CreatedUtc >= fromD && x.CreatedUtc <= toD);
                    lQuery = lQuery.Join<CommonPartRecord>().Where(x => x.CreatedUtc >= fromD && x.CreatedUtc <= toD);
                } else if (model.AdvancedOptions.DateFilterType == DateFilterOptions.Modified) {
                    query = query.Join<CommonPartRecord>().Where(x => x.ModifiedUtc >= fromD && x.ModifiedUtc <= toD);
                    lQuery = lQuery.Join<CommonPartRecord>().Where(x => x.ModifiedUtc >= fromD && x.ModifiedUtc <= toD);
                } else if (model.AdvancedOptions.DateFilterType == DateFilterOptions.Published) {
                    query = query.Join<CommonPartRecord>().Where(x => x.PublishedUtc >= fromD && x.PublishedUtc <= toD);
                    lQuery = lQuery.Join<CommonPartRecord>().Where(x => x.PublishedUtc >= fromD && x.PublishedUtc <= toD);
                }
            }

            // Has media query
            if (model.AdvancedOptions.HasMedia) {
                var allCt = GetListableTypes(false);
                var listFields = new List<string>();
                foreach (var ct in allCt) {
                    var allMediaFld = _contentDefinitionService.GetType(ct.Name).Fields.Where(w =>
                        w._Definition.FieldDefinition.Name == "MediaLibraryPickerField");
                    var allFieldNames = allMediaFld.Select(s => ct.Name + "." + s.Name + ".");
                    listFields.AddRange(allFieldNames);
                }

                query = query.Join<FieldIndexPartRecord>().Where(w => w.StringFieldIndexRecords.Any(
                    w2 => listFields.Contains(w2.PropertyName) && w2.Value != ""
                    ));
                lQuery = lQuery.Join<FieldIndexPartRecord>().Where(w => w.StringFieldIndexRecords.Any(
                    w2 => listFields.Contains(w2.PropertyName) && w2.Value != ""
                    ));
            }

            // Extended Status query
            if (!String.IsNullOrWhiteSpace(model.AdvancedOptions.SelectedStatus)) {
                query = query.Join<FieldIndexPartRecord>().Where(w => w.StringFieldIndexRecords.Any(
                    w2 => w2.PropertyName == "PublishExtensionPart.PublishExtensionStatus." && w2.Value == model.AdvancedOptions.SelectedStatus
                    ));
                lQuery = lQuery.Join<FieldIndexPartRecord>().Where(w => w.StringFieldIndexRecords.Any(
                    w2 => w2.PropertyName == "PublishExtensionPart.PublishExtensionStatus." && w2.Value == model.AdvancedOptions.SelectedStatus
                    ));
            }
            // FILTER QUERIES: END //


            switch (model.Options.OrderBy) {
                case ContentsOrder.Modified:
                    //query = query.OrderByDescending<ContentPartRecord, int>(ci => ci.ContentItemRecord.Versions.Single(civr => civr.Latest).Id);
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                    lQuery = lQuery.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                    lQuery = lQuery.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    //query = query.OrderByDescending<ContentPartRecord, int>(ci => ci.Id);
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                    lQuery = lQuery.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                    break;
            }

            model.Options.SelectedFilter = model.TypeName;
            model.Options.FilterOptions = GetListableTypes(false)
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            // FILTER MODELS: START //
            // language filter model
            model.AdvancedOptions.LanguageOptions = _commonService.ListCultures().Select(x=> new KeyValuePair<int, string>(x.Id, x.Culture));
            // taxonomy filter model
            var termList = new List<KeyValuePair<int, string>>();
            foreach (var taxonomy in _taxonomyService.GetTaxonomies()) {
                termList.Add(new KeyValuePair<int, string>(-1, taxonomy.Name));
                foreach (var term in _taxonomyService.GetTerms(taxonomy.Id)) {
                    var gap = new string('-', term.GetLevels());

                    if (gap.Length > 0) {
                        gap += " ";
                    }
                    termList.Add(new KeyValuePair<int, string>(term.Id, gap + term.Name));
                }
            }
            model.AdvancedOptions.TaxonomiesOptions = termList;

            // extended status
            var partDefinition = _contentDefinitionService.GetPart("PublishExtensionPart");
            if (partDefinition != null) {

                var partField = partDefinition.Fields.Where(w => w.Name == "PublishExtensionStatus").SingleOrDefault();
                var settings = partField.Settings.GetModel<EnumerationFieldSettings>().Options;
                string[] options = (!String.IsNullOrWhiteSpace(settings)) ? settings.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None) : null;
                model.AdvancedOptions.StatusOptions = options.Select(s => new KeyValuePair<string, string>(s, T(s).Text));
            }

            #region TEST OF CPF QUERIES
            //if (model.AdvancedOptions.CPFOwnerId != null) {
            //    var item = _contentManager.Get((int)model.AdvancedOptions.CPFOwnerId);
            //    var parts = item.Parts;
            //    list = Shape.List();
            //    foreach (var part in parts) {
            //        foreach (var field in part.Fields) {
            //            if (field.FieldDefinition.Name == "ContentPickerField") {
            //                bool noName = String.IsNullOrWhiteSpace(model.AdvancedOptions.CPFName);
            //                if (noName || (!noName && field.Name == model.AdvancedOptions.CPFName)) {
            //                    var relatedItems = _contentManager.GetMany<ContentItem>((IEnumerable<int>)field.GetType().GetProperty("Ids").GetValue(field), VersionOptions.Latest, QueryHints.Empty);

            //                    list.AddRange(relatedItems.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));
            //                }
            //            }
            //        }
            //    }
            //}
            if (model.AdvancedOptions.CPFIdToSearch != null && !String.IsNullOrWhiteSpace(model.AdvancedOptions.CPFName)) {
                //given an Id, search for all items that have a Content Picker Field whose PropertyName is PCFName and that have the
                //Id among the corresponding values.
                string fieldName = (string)model.AdvancedOptions.CPFName;
                query = query.Join<FieldIndexPartRecord>()
                    .Where(fip =>
                        fip.StringFieldIndexRecords
                            .Any(sfi =>
                                sfi.PropertyName.Contains(fieldName)
                                && sfi.LatestValue.Contains("{" + model.AdvancedOptions.CPFIdToSearch.ToString() + "}")
                            )
                    );
                lQuery = lQuery.Join<FieldIndexPartRecord>()
                    .Where(fip =>
                        fip.StringFieldIndexRecords
                            .Any(sfi =>
                                sfi.PropertyName.Contains(fieldName)
                                && sfi.LatestValue.Contains("{" + model.AdvancedOptions.CPFIdToSearch.ToString() + "}")
                            )
                    );
            }
            #endregion

            // FILTER MODELS: END //


            //EXECUTE QUERIES

            var pagerShape = Shape.Pager(pager).TotalItemCount(0);
            var list = Shape.List();
            IEnumerable<ContentItem> pageOfContentItems = (IEnumerable<ContentItem>)null;

            //Stopwatch sw = Stopwatch.StartNew();

            //the user may not have permission to see anything: in that case, do not execute the query
            #region original query 
            //this is roughly 1000 slower than the variant below
            //if (Services.Authorizer.Authorize(AdvancedSearchPermissions.CanSeeOwnContents)) {
            //    // language query
            //    //For any language query, remember that Orchard's localization table, as of Orchard 1.8, has an issue where the content
            //    //created but never translated does not have the default Culture assigned to it.
            //    Expression<Func<LocalizationPartRecord, bool>> selLangPredicate = null;
            //    if (model.AdvancedOptions.SelectedLanguageId > 0) {
            //        bool siteCultureSelected = _cultureManager.GetSiteCulture() == _cultureManager.GetCultureById(model.AdvancedOptions.SelectedLanguageId).Culture;
            //        if (siteCultureSelected) {
            //            selLangPredicate =
            //                x => x.CultureId == model.AdvancedOptions.SelectedLanguageId ||
            //                    x.CultureId == 0;
            //        } else {
            //            selLangPredicate =
            //                x => x.CultureId == model.AdvancedOptions.SelectedLanguageId;
            //        }
            //    }
            //    if (model.AdvancedOptions.SelectedLanguageId > 0) {
            //        query = query.Join<LocalizationPartRecord>().Where(selLangPredicate);
            //    }
            //    //if we want only items that do not have a specific translation, we have to do things differently,
            //    //because the check is done after the query. Hence, for example, we cannot directly page.
            //    if (model.AdvancedOptions.SelectedUntranslatedLanguageId > 0) {
            //        var allCi = query.List();
            //        //for (int i = 0; i < allCi.Count(); i++) { //this loop is used to test null conditions and other stuff like that while debugging
            //        //    var ci = allCi.ElementAt(i);
            //        //    if (ci.Is<LocalizationPart>()) {
            //        //        var lci = _localizationService.GetLocalizations(ci, versionOptions);
            //        //        var clci = lci.Count();
            //        //        var bo = lci.Any(li => li.Culture.Id == model.AdvancedOptions.SelectedUntranslatedLanguageId);
            //        //    }
            //        //}
            //        var untranslatedCi = allCi
            //            .Where(x =>
            //                x.Is<LocalizationPart>() && //the content is translatable
            //                (
            //                    x.As<LocalizationPart>().Culture == null || //this is the case where the content was created and never translated to any other culture.
            //                    x.As<LocalizationPart>().Culture.Id != model.AdvancedOptions.SelectedUntranslatedLanguageId //not a content in the language in which we are looking translations
            //                ) &&
            //                _localizationService.GetLocalizations(x, versionOptions) != null &&
            //                !_localizationService.GetLocalizations(x, versionOptions).Any(li =>
            //                    li.Culture != null &&
            //                    li.Culture.Id == model.AdvancedOptions.SelectedUntranslatedLanguageId
            //                )
            //            );
            //        //.Where(x =>
            //        //    x.Is<LocalizationPart>() && //some content items may not be translatable
            //        //    (
            //        //        (x.As<LocalizationPart>().Culture != null && x.As<LocalizationPart>().Culture.Id != model.AdvancedOptions.SelectedUntranslatedLanguageId) ||
            //        //        (x.As<LocalizationPart>().Culture == null) //this is the case where the content was created and never translated to any other culture. 
            //        //        //In that case, in Orchard 1.8, no culture is directly assigned to it, even though the default culture is assumed.
            //        //    ) &&
            //        //    x.As<LocalizationPart>().MasterContentItem == null &&
            //        //    !allCi.Any(y =>
            //        //        y.Is<LocalizationPart>() &&
            //        //        (y.As<LocalizationPart>().MasterContentItem == x || y.As<LocalizationPart>().MasterContentItem == x.As<LocalizationPart>().MasterContentItem) &&
            //        //        y.As<LocalizationPart>().Culture.Id == model.AdvancedOptions.SelectedUntranslatedLanguageId
            //        //    )
            //        //);
            //        //Paging
            //        pagerShape = Shape.Pager(pager).TotalItemCount(untranslatedCi.Count());
            //        int pSize = pager.PageSize != 0 ? pager.PageSize : untranslatedCi.Count();
            //        pageOfContentItems = untranslatedCi
            //            .Skip(pager.GetStartIndex())
            //            .Take((pager.GetStartIndex() + pSize) > untranslatedCi.Count() ?
            //                untranslatedCi.Count() - pager.GetStartIndex() :
            //                pSize)
            //            .ToList();
            //    } else {
            //        pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
            //        pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            //    }
            //} else {
            //    Services.Notifier.Error(T("Not authorized to visualize any item."));
            //}
            #endregion

            if (Services.Authorizer.Authorize(AdvancedSearchPermissions.CanSeeOwnContents)) {
                // language queries
                //For any language query, remember that Orchard's localization table, as of Orchard 1.8, has an issue where the content
                //created but never translated does not have the default Culture assigned to it.
                Expression<Func<LocalizationPartRecord, bool>> selLangPredicate = null;
                if (model.AdvancedOptions.SelectedLanguageId > 0) {
                    bool siteCultureSelected = _cultureManager.GetSiteCulture() == _cultureManager.GetCultureById(model.AdvancedOptions.SelectedLanguageId).Culture;
                    if (siteCultureSelected) {
                        selLangPredicate =
                            x => x.CultureId == model.AdvancedOptions.SelectedLanguageId ||
                                x.CultureId == 0;
                    } else {
                        selLangPredicate =
                            x => x.CultureId == model.AdvancedOptions.SelectedLanguageId;
                    }

                    query = query.Join<LocalizationPartRecord>().Where(selLangPredicate);
                }
                Expression<Func<LocalizationPartRecord, bool>> untranLangPredicate = null;
                if (model.AdvancedOptions.SelectedUntranslatedLanguageId > 0) {
                    bool siteCultureSelected = _cultureManager.GetSiteCulture() == _cultureManager.GetCultureById(model.AdvancedOptions.SelectedUntranslatedLanguageId).Culture;
                    if (siteCultureSelected) {
                        untranLangPredicate =
                            x => x.CultureId == model.AdvancedOptions.SelectedUntranslatedLanguageId ||
                                x.CultureId == 0;
                    } else {
                        untranLangPredicate =
                            x => x.CultureId == model.AdvancedOptions.SelectedUntranslatedLanguageId;
                    }

                    lQuery = lQuery.Join<LocalizationPartRecord>().Where(untranLangPredicate);
                    var lRes = lQuery.List();
                    var masters = lRes.Where(x => x.As<LocalizationPart>().Record.MasterContentItemId != 0).Select(x => x.As<LocalizationPart>().Record.MasterContentItemId).ToList();
                    var items = lRes.Select(x => x.Id).ToList();

                    Expression<Func<LocalizationPartRecord, bool>> sulPredicate =
                        x => x.CultureId != model.AdvancedOptions.SelectedUntranslatedLanguageId &&
                            !masters.Contains(x.Id) && !masters.Contains(x.MasterContentItemId) && !items.Contains(x.MasterContentItemId);

                    query = query.Join<LocalizationPartRecord>().Where(sulPredicate);
                }

                pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
                pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            } else {
                Services.Notifier.Error(T("Not authorized to visualize any item."));
            }

            //sw.Stop();
            //Services.Notifier.Error(new LocalizedString(sw.Elapsed.TotalMilliseconds.ToString()));

            if (pageOfContentItems != null) {
                list.AddRange(pageOfContentItems.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));
            }

            var viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .Options(model.Options)
                .AdvancedOptions(model.AdvancedOptions)
                .TypeDisplayName(model.TypeDisplayName ?? "");

            return View(viewModel);
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd =>
                Services.Authorizer.Authorize(Permissions.EditContent, _contentManager.New(ctd.Name)) &&
                ctd.Settings.GetModel<ContentTypeSettings>().Creatable &&
                (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

        private IEnumerable<ContentTypeDefinition> GetListableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd =>
                Services.Authorizer.Authorize(Permissions.EditContent, _contentManager.New(ctd.Name)) &&
                ctd.Settings.GetModel<ContentTypeSettings>().Listable &&
                (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

        [Admin]
        [HttpPost, ActionName("List")]
        [Mvc.FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ContentOptions options, AdvancedContentOptions advancedOptions) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                bool seeAll = Services.Authorizer.Authorize(AdvancedSearchPermissions.SeesAllContent);
                bool maySee = Services.Authorizer.Authorize(AdvancedSearchPermissions.MayChooseToSeeOthersContent);
                if ((seeAll && advancedOptions.OwnedByMeSeeAll)
                    || (!seeAll && maySee && advancedOptions.OwnedByMe)) {
                    advancedOptions.SelectedOwner = Services.WorkContext.CurrentUser.UserName;
                }

                routeValues["Options.OrderBy"] = options.OrderBy; //todo: don't hard-code the key
                routeValues["Options.ContentsStatus"] = options.ContentsStatus; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedLanguageId"] = advancedOptions.SelectedLanguageId; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedUntranslatedLanguageId"] = advancedOptions.SelectedUntranslatedLanguageId; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedTermId"] = advancedOptions.SelectedTermId; //todo: don't hard-code the key
                //condition to add the owner to the query string only if we are not going to ignore it anyway
                if (    //user may see everything
                        (seeAll
                        && (!advancedOptions.OwnedByMeSeeAll))
                        || (  //user does not have limitations
                            (maySee)
                            && (!advancedOptions.OwnedByMe)
                        )
                    ) {
                    routeValues["AdvancedOptions.SelectedOwner"] = advancedOptions.SelectedOwner; //todo: don't hard-code the key
                }
                routeValues["AdvancedOptions.SelectedFromDate"] = advancedOptions.SelectedFromDate; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedToDate"] = advancedOptions.SelectedToDate; //todo: don't hard-code the key
                routeValues["AdvancedOptions.DateFilterType"] = advancedOptions.DateFilterType; //todo: don't hard-code the key
                routeValues["AdvancedOptions.HasMedia"] = advancedOptions.HasMedia; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedStatus"] = advancedOptions.SelectedStatus; //todo: don't hard-code the key
                routeValues["AdvancedOptions.OwnedByMe"] = advancedOptions.OwnedByMe; //todo: don't hard-code the key
                routeValues["AdvancedOptions.OwnedByMeSeeAll"] = advancedOptions.OwnedByMeSeeAll; //todo: don't hard-code the key

                //Querying base off content picker field
                if (advancedOptions.CPFIdToSearch != null) {
                    routeValues["AdvancedOptions.CPFIdToSearch"] = advancedOptions.CPFIdToSearch;
                    if (!String.IsNullOrWhiteSpace(advancedOptions.CPFName)) {
                        routeValues["AdvancedOptions.CPFName"] = advancedOptions.CPFName;
                    }
                }


                if (GetListableTypes(false).Any(ctd => string.Equals(ctd.Name, options.SelectedFilter, StringComparison.OrdinalIgnoreCase))) {
                    routeValues["id"] = options.SelectedFilter;
                } else {
                    routeValues.Remove("id");
                }
            }

            return RedirectToAction("List", routeValues);
        }

        [Admin]
        [HttpPost, ActionName("List")]
        [Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult ListPOST(ContentOptions options, IEnumerable<int> itemIds, string returnUrl) {
            if (itemIds != null) {
                var checkedContentItems = _contentManager.GetMany<ContentItem>(itemIds, VersionOptions.Latest, QueryHints.Empty);
                switch (options.BulkAction) {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.PublishNow:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't publish selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Publish(item);
                        }
                        Services.Notifier.Information(T("Content successfully published."));
                        break;
                    case ContentsBulkAction.Unpublish:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't unpublish selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Unpublish(item);
                        }
                        Services.Notifier.Information(T("Content successfully unpublished."));
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.DeleteContent, item, T("Couldn't remove selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Remove(item);
                        }
                        Services.Notifier.Information(T("Content successfully removed."));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        ActionResult CreatableTypeList(int? containerId) {
            var viewModel = Shape.ViewModel(ContentTypes: GetCreatableTypes(containerId.HasValue), ContainerId: containerId);

            return View("CreatableTypeList", viewModel);
        }

        ActionResult ListableTypeList(int? containerId) {
            var viewModel = Shape.ViewModel(ContentTypes: GetListableTypes(containerId.HasValue), ContainerId: containerId);

            return View("ListableTypeList", viewModel);
        }

        [Admin]
        public ActionResult Create(string id, int? containerId) {
            if (string.IsNullOrEmpty(id))
                return CreatableTypeList(containerId);

            var contentItem = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Cannot create content")))
                return new HttpUnauthorizedResult();

            if (containerId.HasValue && contentItem.Is<ContainablePart>()) {
                var common = contentItem.As<CommonPart>();
                if (common != null) {
                    common.Container = _contentManager.Get(containerId.Value);
                }
            }

            var model = _contentManager.BuildEditor(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Save")]
        public ActionResult CreatePOST(string id, string returnUrl) {
            return CreatePOST(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [Admin]
        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Publish")]
        public ActionResult CreateAndPublishPOST(string id, string returnUrl) {

            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, dummyContent, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            return CreatePOST(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private ActionResult CreatePOST(string id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var contentItem = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            _contentManager.Create(contentItem, VersionOptions.Draft);

            var model = _contentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            conditionallyPublish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been created.")
                : T("Your {0} has been created.", contentItem.TypeDefinition.DisplayName));
            if (!string.IsNullOrEmpty(returnUrl)) {
                return this.RedirectLocal(returnUrl);
            }
            var adminRouteValues = _contentManager.GetItemMetadata(contentItem).AdminRouteValues;
            return RedirectToRoute(adminRouteValues);
        }

        [Admin]
        public ActionResult Edit(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Cannot edit content")))
                return new HttpUnauthorizedResult();

            var model = _contentManager.BuildEditor(contentItem);
            return View(model);
        }

        [Admin]
        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int id, string returnUrl) {
            return EditPOST(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [Admin]
        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Publish")]
        public ActionResult EditAndPublishPOST(int id, string returnUrl) {
            var content = _contentManager.Get(id, VersionOptions.Latest);

            if (content == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, content, T("Couldn't publish content")))
                return new HttpUnauthorizedResult();

            return EditPOST(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        [Admin]
        private ActionResult EditPOST(int id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var contentItem = _contentManager.Get(id, VersionOptions.DraftRequired);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Couldn't edit content")))
                return new HttpUnauthorizedResult();

            string previousRoute = null;
            if (contentItem.Has<IAliasAspect>()
                && !string.IsNullOrWhiteSpace(returnUrl)
                && Request.IsLocalUrl(returnUrl)
                // only if the original returnUrl is the content itself
                && String.Equals(returnUrl, Url.ItemDisplayUrl(contentItem), StringComparison.OrdinalIgnoreCase)
                ) {
                previousRoute = contentItem.As<IAliasAspect>().Path;
            }

            var model = _contentManager.UpdateEditor(contentItem, this);
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View("Edit", model);
            }

            conditionallyPublish(contentItem);

            if (!string.IsNullOrWhiteSpace(returnUrl)
                && previousRoute != null
                && !String.Equals(contentItem.As<IAliasAspect>().Path, previousRoute, StringComparison.OrdinalIgnoreCase)) {
                returnUrl = Url.ItemDisplayUrl(contentItem);
            }

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been saved.")
                : T("Your {0} has been saved.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Edit", new RouteValueDictionary { { "Id", contentItem.Id } }));
        }

        [Admin]
        [HttpPost]
        public ActionResult Clone(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Couldn't clone content")))
                return new HttpUnauthorizedResult();

            try {
                Services.ContentManager.Clone(contentItem);
            } catch (InvalidOperationException) {
                Services.Notifier.Warning(T("Could not clone the content item."));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
            }

            Services.Notifier.Information(T("Successfully cloned. The clone was saved as a draft."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [Admin]
        [HttpPost]
        public ActionResult Remove(int id, string returnUrl) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (!Services.Authorizer.Authorize(Permissions.DeleteContent, contentItem, T("Couldn't remove content")))
                return new HttpUnauthorizedResult();

            if (contentItem != null) {
                _contentManager.Remove(contentItem);
                Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                    ? T("That content has been removed.")
                    : T("That {0} has been removed.", contentItem.TypeDefinition.DisplayName));
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [Admin]
        [HttpPost]
        public ActionResult Publish(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("Couldn't publish content")))
                return new HttpUnauthorizedResult();

            _contentManager.Publish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName) ? T("That content has been published.") : T("That {0} has been published.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [Admin]
        [HttpPost]
        public ActionResult Unpublish(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("Couldn't unpublish content")))
                return new HttpUnauthorizedResult();

            _contentManager.Unpublish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName) ? T("That content has been unpublished.") : T("That {0} has been unpublished.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

}