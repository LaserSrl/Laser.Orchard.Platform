using Laser.Orchard.DataProtection.Models;
using NHibernate;
using NHibernate.Criterion;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.UI.Admin;
using Orchard.Users.Models;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System;
using Orchard.Security.Permissions;
using Orchard.Core.Contents;
using Orchard.Projections.Descriptors.Filter;
using LExp = System.Linq.Expressions;

namespace Laser.Orchard.DataProtection.Services {
    public interface IDataProtectionCheckerService : IDependency {
        ContentItem CheckDataRestricitons(ContentItem contentItem, Permission permission);
        void CheckDataRestricitons(ICriteria criteria);
        void CheckDataRestricitons(FilterContext context);
    }
    public class DataProtectionCheckerService : IDataProtectionCheckerService {
        private readonly IRepository<UserPartRecord> _repoUsers;
        private readonly IRepository<ContentItemRecord> _repoSite;
        private readonly IRepository<DataRestrictionsRecord> _repoRestrictions;
        private bool _isBackEnd;
        private List<List<string>> _userRestrictionsForView;
        private List<List<string>> _userRestrictionsForEdit;
        private bool _isSuperUser;
        private bool _applyToFrontEnd;
        // Warning: se si aggiungono parametri al costruttore prestare attenzione a non creare dipendenze circolari
        // perché questo costruttore viene richiamato PRIMA di avere a disposizione il content manager.
        public DataProtectionCheckerService(
            IRepository<UserPartRecord> repoUsers,
            IRepository<ContentItemRecord> repoSite,
            IRepository<DataRestrictionsRecord> repoRestrictions) {
            _repoUsers = repoUsers;
            _repoSite = repoSite;
            _repoRestrictions = repoRestrictions;
            _userRestrictionsForView = new List<List<string>>();
            _userRestrictionsForEdit = new List<List<string>>();
            var context = HttpContext.Current;
            string userName = "";
            _applyToFrontEnd = true;
            var commaSeparator = new[] { ',' };
            try {
                _isBackEnd = AdminFilter.IsApplied(context.Request.RequestContext);
                if (_isBackEnd == false) {
                    _isBackEnd = context.Request.RawUrl.IndexOf("/Admin/", StringComparison.InvariantCultureIgnoreCase) >= 0;
                }
            } catch {
                _isBackEnd = false;
            }
            // recupera il site per capire se l'utente è un amministratore e leggere i settings
            var aux = _repoSite.Get(1).Infoset;
            try {
                userName = context.User.Identity.Name;
                var superuser = aux.Element.Element("SiteSettingsPart").Attribute("SuperUser").Value;
                if (userName == superuser) {
                    _isSuperUser = true;
                }
            } catch {
                _isSuperUser = true;
            }
            try {
                var applyToFrontEnd = aux.Element.Element("DataProtectionSiteSettings").Attribute("ApplyToFrontEnd").Value;
                if (applyToFrontEnd.Equals("true", StringComparison.InvariantCultureIgnoreCase) == false) {
                    _applyToFrontEnd = false;
                }
            } catch {
                _applyToFrontEnd = true;
            }
            // recupera le restrictions dell'utente
            try {
                if (string.IsNullOrWhiteSpace(userName) == false) {
                    // recupera l'id dell'utente
                    var userRecord = _repoUsers.Fetch(x => x.UserName == userName).FirstOrDefault();
                    if (userRecord != null) {
                        var userId = userRecord.Id;
                        // recupera le restrictions dell'utente tramite un repository
                        var restrictionsList = _repoRestrictions.Fetch(x => x.DataRestrictionsPartRecord_id == userId);
                        foreach (var set in restrictionsList) {
                            if (string.IsNullOrWhiteSpace(set.Restrictions) == false) {
                                var viewSection = "";
                                var editSection = "";
                                var sections = set.Restrictions.Split('?');
                                viewSection = sections[0];
                                if (sections.Length > 1) {
                                    editSection = sections[1];
                                }
                                // restrictions for view and edit content
                                var viewRestrictions = new List<string>();
                                var editRestrictions = new List<string>();
                                foreach (var val in viewSection.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries)) {
                                    viewRestrictions.Add(val);
                                    editRestrictions.Add(val);
                                }
                                _userRestrictionsForView.Add(viewRestrictions);
                                // restrictions for edit content
                                foreach (var val in editSection.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries)) {
                                    editRestrictions.Add(val);
                                }
                                _userRestrictionsForEdit.Add(editRestrictions);
                            }
                        }
                    }
                }
            } catch {
                // non aggiunge restrictions all'utente
            }
        }
        public ContentItem CheckDataRestricitons(ContentItem contentItem, Permission permission) {
            if (_isSuperUser || (_isBackEnd == false && _applyToFrontEnd == false)) {
                return contentItem;
            }
            List<List<string>> userRestrictionsToTest = null;
            if (permission == Permissions.ViewContent || permission == Permissions.PreviewContent) {
                userRestrictionsToTest = _userRestrictionsForView;
            } else {
                userRestrictionsToTest = _userRestrictionsForEdit;
            }
            var dataProtectionContextPart = contentItem.As<DataProtectionContextPart>();
            if (dataProtectionContextPart != null) {
                var dataProtectionContext = new List<string>();
                if (string.IsNullOrWhiteSpace(dataProtectionContextPart.Context) == false) {
                    foreach (var row in dataProtectionContextPart.Context.Trim().Split(',')) {
                        dataProtectionContext.Add(row);
                    }
                }
                // se l'item non ha nessun data context è visibile a tutti
                if (dataProtectionContext.Count == 0) {
                    return contentItem;
                }
                // almeno un set di restrictions dell'utente deve essere presente nell'item
                var granted = false;
                foreach (var set in userRestrictionsToTest) {
                    // check sul singolo set di restrictions
                    var setGranted = true;
                    foreach (var row in set) {
                        if (dataProtectionContext.Contains(row) == false) {
                            setGranted = false;
                            break;
                        }
                    }
                    if (setGranted) {
                        granted = true;
                        break;
                    }
                }
                if (granted == false) {
                    return null;
                }
            }
            return contentItem;
        }
        public void CheckDataRestricitons(ICriteria criteria) {
            if (_isSuperUser || (_isBackEnd == false && _applyToFrontEnd == false)) {
                return;
            }
            var newCriteria = criteria.CreateCriteria("DataProtectionContextPartRecord", "laserDataContext", NHibernate.SqlCommand.JoinType.LeftOuterJoin);
            AbstractCriterion crit = null;

            // almeno un set di restrictions dell'utente deve essere presente nell'item (or di and)
            foreach (var set in _userRestrictionsForView) {
                AbstractCriterion innerCrit = null;
                foreach (var row in set) {
                    var kv = "%," + row + ",%";
                    if (innerCrit == null) {
                        innerCrit = Restrictions.Like("laserDataContext.Context", kv);
                    } else {
                        innerCrit = Restrictions.And(innerCrit, Restrictions.Like("laserDataContext.Context", kv));
                    }
                }
                if (crit == null) {
                    crit = innerCrit;
                } else {
                    crit = Restrictions.Or(crit, innerCrit);
                }
            }
            if (crit != null) {
                newCriteria = newCriteria.Add(Restrictions.Or(Restrictions.Or(Restrictions.IsNull("laserDataContext.Context"), Restrictions.Eq("laserDataContext.Context", "")), crit));
            }
        }
        public void CheckDataRestricitons(FilterContext context) {
            if (_isSuperUser || (_isBackEnd == false && _applyToFrontEnd == false)) {
                return;
            }

            // almeno un set di restrictions dell'utente deve essere presente nell'item (or di and)
            Action<IHqlExpressionFactory> crit = null;
            foreach (var set in _userRestrictionsForView) {
                Action<IHqlExpressionFactory> innerCrit = null;
                foreach (var row in set) {
                    var kv = "," + row + ",";
                    if (innerCrit == null) {
                        innerCrit = BuildLikeExpression(kv);
                    } else {
                        innerCrit = BuildAndLikeExpression(innerCrit, kv);
                    }
                }
                if (crit == null) {
                    crit = innerCrit;
                } else {
                    crit = BuildOrExpression(crit, innerCrit);
                }
            }
            if (crit != null) {
                context.Query.Where(a => a.ContentPartRecord<DataProtectionContextPartRecord>("left join"), x1 =>
                    x1.Or(x2 => x2.Or(x3 => x3.IsNull("Context"), x4 => x4.Eq("Context", "")), crit)
                );
            }
        }
        // I tre metodi seguenti sembrano necessari: se si include il loro contenuto inline, si creano funzioni ricorsive
        // che generano una StackOverflowException. Invece utilizzando dei metodi separati questo non succede.
        private Action<IHqlExpressionFactory> BuildLikeExpression(string theValue) {
            return x => x.Like("Context", theValue, HqlMatchMode.Anywhere);
        }
        private Action<IHqlExpressionFactory> BuildAndLikeExpression(Action<IHqlExpressionFactory> lCrit, string theValue) {
            return y => y.And(lCrit, x => x.Like("Context", theValue, HqlMatchMode.Anywhere));
        }
        private Action<IHqlExpressionFactory> BuildOrExpression(Action<IHqlExpressionFactory> lCrit, Action<IHqlExpressionFactory> rCrit) {
            return x => x.Or(lCrit, rCrit);
        }
    }
}