using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Modules.Services;
using Orchard.Security.Permissions;
using Orchard.Roles.Services;
using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Fields.Settings;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.MediaLibrary.Fields;
using Orchard.Taxonomies.Fields;
using Orchard.Fields.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Localization.Models;
using Orchard.Taxonomies.Services;
using System.Web.Mvc;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;
using Laser.Orchard.StartupConfig.IdentityProvider;
using System.Collections;
using Orchard.Localization.Services;

namespace Laser.Orchard.StartupConfig.Services {

    public interface IUtilsServices : IDependency {

        string TenantPath { get; }

        string StorageMediaPath { get; }

        string VirtualMediaPath { get; }

        string PublicMediaPath { get; }

        void DisableFeature(string featureId);

        void EnableFeature(string featureId);

        bool FeatureIsEnabled(string featureId);

        /// <summary>
        /// This method may be called whenever we need to update some roles' permissions based on known stereotypes.
        /// For example, whenever we add new permissions and stereotypes to a module, we should update that module's
        /// migration, calling this method and giving the new stereotypes as parameters.
        /// </summary>
        /// <param name="stereotypes">An <type>IEnumerable<PermissionStereotype></type> obtained for example by a call to
        /// <example>new Permissions().GetDefaultStereotypes();</example></param>
        void UpdateStereotypesPermissions(IEnumerable<PermissionStereotype> stereotypes);

        Response GetResponse(ResponseType rsptype, string message = "", dynamic data = null);
        /// <summary>
        /// Convert a Response object to a ContentResult in json format.
        /// This is useful when you want to pass a json string in the Data property of the response 
        /// and you want it to be considered as a json object, not a string as method "Json(object)" does.
        /// So you can use this method when you want to gain full control on the json object set in Data property.
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        ContentResult ConvertToJsonResult(Response resp);
        /// <summary>
        /// Get a JObject which contains identity providers informations of the current user.
        /// </summary>
        /// <param name="identityProviders"></param>
        /// <returns></returns>
        JObject GetUserIdentityProviders(IEnumerable<IIdentityProvider> identityProviders);
        /// <summary>
        /// Get a successfull response to a login request with registration data.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Response GetUserResponse(String message, IEnumerable<IIdentityProvider> identityProviders);
        /// <summary>
        /// Set values on a field in a content part.
        /// </summary>
        /// <param name="listpart"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void StoreInspectExpandoFields(List<ContentPart> listpart, string key, object value);
    }

    public class UtilsServices : IUtilsServices {

        public Localizer T { get; set; }
        private readonly IModuleService _moduleService;
        private readonly string _tenantPath;
        private readonly string _storageMediaPath; // C:\orchard\media\default
        private readonly string _virtualMediaPath; // ~/Media/Default/
        private readonly string _publicMediaPath; // /Orchard/Media/Default/
        private readonly IRoleService _roleService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IOrchardServices _orchardServices;
        private readonly ILocalizationService _localizationServices;

        public UtilsServices(IModuleService moduleService, ShellSettings settings, IRoleService roleService, ITaxonomyService taxonomyService, IOrchardServices orchardServices, ILocalizationService localizationServices) {
            _moduleService = moduleService;
            _roleService = roleService;
            _taxonomyService = taxonomyService;
            _orchardServices = orchardServices;
            _localizationServices = localizationServices;
            var mediaPath = HostingEnvironment.IsHosted
                                ? HostingEnvironment.MapPath("~/Media/") ?? ""
                                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");

            _storageMediaPath = Path.Combine(mediaPath, settings.Name);
            _virtualMediaPath = "~/Media/" + settings.Name + "/";

            var appPath = "";
            if (HostingEnvironment.IsHosted) {
                appPath = HostingEnvironment.ApplicationVirtualPath;
            }
            if (!appPath.EndsWith("/"))
                appPath = appPath + '/';
            if (!appPath.StartsWith("/"))
                appPath = '/' + appPath;

            _publicMediaPath = appPath + "Media/" + settings.Name + "/";

            _tenantPath = HostingEnvironment.IsHosted ? HostingEnvironment.MapPath("~/") ?? "" : AppDomain.CurrentDomain.BaseDirectory;

            T = NullLocalizer.Instance;
        }

        public Response GetResponse(ResponseType rsptype, string message = "", dynamic data = null) {
            Response rsp = new Response();
            rsp.Message = message;
            switch (rsptype) {
                case ResponseType.Success:
                    rsp.Success = true;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = T("Successfully Executed").ToString();
                    rsp.ErrorCode = ErrorCode.NoError;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.NoAction;
                    break;

                case ResponseType.InvalidUser:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = T("Invalid User").ToString();
                    rsp.ErrorCode = ErrorCode.InvalidUser;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.Login;
                    break;

                case ResponseType.InvalidXSRF:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = T("Invalid Token/csrfToken").ToString();
                    rsp.ErrorCode = ErrorCode.InvalidXSRF;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.Login;
                    break;

                case ResponseType.Validation:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = T("Validation error").ToString();
                    rsp.ErrorCode = ErrorCode.Validation;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.NoAction;
                    break;

                case ResponseType.UnAuthorized:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = T("UnAuthorized Action").ToString();
                    rsp.ErrorCode = ErrorCode.UnAuthorized;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.NoAction;
                    break;

                case ResponseType.MissingPolicies:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = T("It seems you have not yet accepted the required policies").ToString();
                    rsp.ErrorCode = ErrorCode.MissingPolicies;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.AcceptPolicies;
                    break;

                case ResponseType.ToConfirmEmail:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = T("Thank you for registering. We sent you an e-mail with instructions to enable your account.").ToString();
                    rsp.ErrorCode = ErrorCode.ToConfirmEmail;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.ToConfirmEmail;
                    break;
            }
            return rsp;
        }
        public ContentResult ConvertToJsonResult(Response resp) {
            var data = "";
            if (resp.Data == null) {
                data = "null";
            } else if (resp.Data.GetType().Name == typeof(string).Name) {
                data = resp.Data;
            } else {
                data = Newtonsoft.Json.JsonConvert.SerializeObject(resp.Data);
            }
            var jstring = new JValue(resp.Message).ToString();
            var content = string.Format("{{ \"Success\": {0},\"Message\":\"{1}\",\"Data\":{4},\"ErrorCode\":{2},\"ResolutionAction\":{3}}}", resp.Success.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(), EscapeForJson(resp.Message), (int)resp.ErrorCode, (int)resp.ResolutionAction, data);
            return new ContentResult {
                Content = content,
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json"
            };
        }
        private string EscapeForJson(string text) {
            var result = text;
            result = result.Replace("\\", "\\\\");
            result = result.Replace("\"", "\\\"");
            result = result.Replace("\t", "\\t");
            result = result.Replace("\r", "\\r");
            result = result.Replace("\n", "\\n");
            result = result.Replace("\f", "\\f");
            result = result.Replace("\b", "\\b");
            return result;
        }
        public JObject GetUserIdentityProviders(IEnumerable<IIdentityProvider> identityProviders) {
            var registeredServicesData = new JObject();
            if (_orchardServices.WorkContext.CurrentUser != null) {
                var context = new Dictionary<string, object> { { "Content", _orchardServices.WorkContext.CurrentUser.ContentItem } };
                foreach (var provider in identityProviders) {
                    var val = provider.GetRelatedId(context);
                    if (string.IsNullOrWhiteSpace(val.Key) == false) {
                        registeredServicesData.Add(val.Key, JToken.FromObject(val.Value));
                    }
                }
            }
            return registeredServicesData;
        }
        public Response GetUserResponse(String message, IEnumerable<IIdentityProvider> identityProviders) {
            var registeredServicesData = GetUserIdentityProviders(identityProviders);
            //var json = registeredServicesData.ToString();
            return GetResponse(ResponseType.Success, message, registeredServicesData);
        }
        /// <summary>
        /// Returns the tenant path
        /// </summary>
        public string TenantPath { get { return _tenantPath; } }

        /// <summary>
        /// Returns the media path in the format C:\orchard\media\default\
        /// </summary>
        public string StorageMediaPath { get { return _storageMediaPath; } }

        /// <summary>
        /// Returns the media path in the format ~/Media/Default/
        /// </summary>
        public string VirtualMediaPath { get { return _virtualMediaPath; } }

        /// <summary>
        /// Returns the media path in the format /Orchard/Media/Default/
        /// </summary>
        public string PublicMediaPath { get { return _publicMediaPath; } }

        public void DisableFeature(string featureId) {
            var features = _moduleService.GetAvailableFeatures().ToDictionary(m => m.Descriptor.Id, m => m);
            if (features.ContainsKey(featureId) && features[featureId].IsEnabled) {
                _moduleService.DisableFeatures(new string[] { featureId });
            }
        }

        public void EnableFeature(string featureId) {
            var features = _moduleService.GetAvailableFeatures().ToDictionary(m => m.Descriptor.Id, m => m);

            if (features.ContainsKey(featureId) && !features[featureId].IsEnabled) {
                _moduleService.EnableFeatures(new string[] { featureId },true);
            }
        }

        public bool FeatureIsEnabled(string featureId) {
            var features = _moduleService.GetAvailableFeatures().ToDictionary(m => m.Descriptor.Id, m => m);
            return (features.ContainsKey(featureId) && features[featureId].IsEnabled);
        }

        public void UpdateStereotypesPermissions(IEnumerable<PermissionStereotype> stereotypes) {
            foreach (var stereotype in stereotypes) {
                //get role corresponding to the stereotype
                var role = _roleService.GetRoleByName(stereotype.Name);
                if (role == null) {
                    //create new role
                    _roleService.CreateRole(stereotype.Name);
                    role = _roleService.GetRoleByName(stereotype.Name);
                }
                //merge permissions into the role
                var stereotypePermissionsNames = (stereotype.Permissions ?? Enumerable.Empty<Permission>()).Select(x => x.Name);
                var currentPermissionsNames = role.RolesPermissions.Select(x => x.Permission.Name);
                var distinctPerrmissionsNames = currentPermissionsNames
                    .Union(stereotypePermissionsNames)
                    .Distinct();
                //if we added permissions we update the role
                var additionalPermissionsNames = distinctPerrmissionsNames.Except(currentPermissionsNames);
                if (additionalPermissionsNames.Any()) {
                    //we have new permissions to add to this role
                    foreach (var permissionName in additionalPermissionsNames) {
                        _roleService.CreatePermissionForRole(role.Name, permissionName);
                    }
                }
            }
        }
        public void StoreInspectExpandoFields(List<ContentPart> listpart, string key, object value) {
            var fields = listpart.SelectMany(x => x.Fields.Where(f => f.Name == key));
            if (fields != null) {
                var fieldObj = fields.FirstOrDefault();
                if (fieldObj != null) {
                    // provo a registrare il dato in uno dei fields
                    // non posso fare questo
                    //      fieldObj.GetType().GetProperty("Value").SetValue(fieldObj, value, null);
                    // perchè non regge il tipo nullabile
                    var theContentItem = listpart.ElementAt(0).ContentItem;
                    string tipofield = fieldObj.GetType().Name;
                    if (tipofield == typeof(EnumerationField).Name) {
                        RegistraValoreEnumerator(fieldObj, "SelectedValues", value);
                    } else if (tipofield == typeof(DateTimeField).Name) {
                        RegistraValore(fieldObj, "DateTime", value);
                    } else if (tipofield == typeof(ContentPickerField).Name || tipofield == typeof(MediaLibraryPickerField).Name) {
                        RegistraValoreContentPickerField(fieldObj, "Ids", value);
                    } else if (tipofield == typeof(TaxonomyField).Name) {
                        var taxobase = _taxonomyService.GetTaxonomyByName(fieldObj.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"]);

                        List<TaxoVM> second = null;
                        if(value is List<TaxoVM>) {
                            second = (List<TaxoVM>)value;
                        } else {
                            second = ConvertToVM((List<dynamic>)value);
                        }

                        List<Int32> ElencoCategorie = second.Select(x => x.Id).ToList();
                        List<TermPart> ListTermPartToAdd = new List<TermPart>();
                        if (_taxonomyService.GetTerm(ElencoCategorie.FirstOrDefault()) == null && ElencoCategorie.Count > 0)
                            throw new Exception("Field " + key + " Taxonomy term with id=" + ElencoCategorie[0].ToString() + " not exist");
                        else {
                            #region [ Tassonomia in Lingua ]
                            foreach (Int32 idtermine in ElencoCategorie) {
                                var term = _taxonomyService.GetTerm(idtermine);
                                if (term != null) {
                                    var taxo_sended_user = _taxonomyService.GetTaxonomy(_taxonomyService.GetTerm(idtermine).TaxonomyId);
                                    TermPart termine_selezionato = taxo_sended_user.Terms.Where(x => x.Id == idtermine).FirstOrDefault();

                                    if (theContentItem.As<LocalizationPart>() == null || theContentItem.ContentType == "User") { // se il contenuto non ha localization oppure è user salvo il mastercontent del termine
                                        var termTranslations = _localizationServices.GetLocalizations(term).Select(x=>x.As<TermPart>()); //get all translations for the term
                                        ListTermPartToAdd.Add(term); //adds the original term
                                        ListTermPartToAdd.AddRange(termTranslations); // adds the translations of term
                                    } else { // se il contenuto ha localization e non è user salvo il termine come mi viene passato
                                            // TODO: testare pertinenza della lingua Contenuto in italianao=>termine in italiano
                                        TermPart toAdd = termine_selezionato;
                                        if (ListTermPartToAdd.Contains(toAdd) == false) {
                                            ListTermPartToAdd.Add(toAdd);
                                        }
                                    }
                                }
                            }
                            ListTermPartToAdd = ListTermPartToAdd.Distinct().ToList(); //removes duplicated
                            #endregion [ Tassonomia in Lingua ]
                        }
                        _taxonomyService.UpdateTerms(theContentItem, ListTermPartToAdd, fieldObj.Name);
                    } else if (tipofield == typeof(LinkField).Name) {
                        LinkVM second = null;
                        if(value is LinkVM) {
                            second = (LinkVM)value;
                        } else {
                            second = ConvertToLinkVM(value);
                        }
                        RegistraValore(fieldObj, "Value", second.Url);
                        RegistraValore(fieldObj, "Text", second.Text);
                    } else {
                        RegistraValore(fieldObj, "Value", value);
                    }
                }
            } else {
                //provo a registrare il dato nella proprieta del current user
                if (!(key.IndexOf(".") > 0) && key.ToLower() != "contenttype" && key.ToLower() != "id" && key.ToLower() != "language") {
                    throw new Exception("Field " + key + " not in contentitem");
                }
            }
        }
        private void RegistraValoreEnumerator(object obj, string key, object value) {
            ListMode listmode = ((dynamic)obj).PartFieldDefinition.Settings.GetModel<EnumerationFieldSettings>().ListMode;
            if (listmode != ListMode.Listbox && listmode != ListMode.Checkbox) {
                key = "Value";
            }
            var property = obj.GetType().GetProperty(key);
            if (property != null) {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                //   ListMode listmode = ((dynamic)obj).PartFieldDefinition.Settings.GetModel<EnumerationFieldSettings>().ListMode;
                if (listmode != ListMode.Listbox && listmode != ListMode.Checkbox) {
                    try {
                        if (((dynamic)value).Count == 1) {
                            value = ((dynamic)value)[0];
                        }
                    } catch { }
                    RegistraValore(obj, key, value);
                } else {
                    if (t.Name == "String[]") { // caso di enumerationfield con multivalue
                        object safeValue = (value == null) ? null : ((List<object>)value).Select(x => x.ToString()).ToArray();
                        property.SetValue(obj, safeValue, null);
                    }
                }
            }
        }

        private void RegistraValore(object obj, string key, object value) {
            // non posso fare questo
            //      fieldObj.GetType().GetProperty("Value").SetValue(fieldObj, value, null);
            // perchè non regge il tipo nullabile
            var property = obj.GetType().GetProperty(key);
            if (property != null) {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object safeValue = (value == null) ? null : Convert.ChangeType(value, t);
                property.SetValue(obj, safeValue, null);
            }
        }

        private void RegistraValoreContentPickerField(object obj, string key, object value) {
            // Convert ha varie implementazioni e non sa castare il tipo correttamente
            var property = obj.GetType().GetProperty(key);
            if (property != null) {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object safeValue = (value == null) ? null : Convert.ChangeType(((List<object>)value).Select(x => Convert.ToInt32(x)).ToArray(), t);
                property.SetValue(obj, safeValue, null);
            }
        }
        private List<TaxoVM> ConvertToVM(List<dynamic> obj) {
            List<TaxoVM> listatvm = new List<TaxoVM>();
            foreach (dynamic el in obj) {
                TaxoVM newel = new TaxoVM();
                newel.Id = Convert.ToInt32(el);
                listatvm.Add(newel);
            }
            return listatvm;
        }
        private LinkVM ConvertToLinkVM(dynamic obj) {
            var result = new LinkVM();
            if(obj != null) {
                result.Url = obj.Value;
                result.Text = obj.Text;
            }
            return result;
        }
    }
    public class TaxoVM {
        public TaxoVM() {
            testo = "";
            valore = "";
            flag = false;
            child = new List<TaxoVM>();
        }
        public Int32 Id { get; set; }
        public string testo { get; set; }
        public string valore { get; set; }
        public bool flag { get; set; }
        public List<TaxoVM> child { get; set; }
    }
    public class LinkVM {
        public string Url { get; set; }
        public string Text { get; set; }
    }
}