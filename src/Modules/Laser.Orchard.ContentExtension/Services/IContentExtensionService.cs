using Laser.Orchard.ContentExtension.Models;
using Laser.Orchard.StartupConfig.Handlers;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Roles.Services;
using Orchard.Security.Permissions;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Laser.Orchard.ContentExtension.Services {

    public enum Methods { Get, Post, Delete, Publish };

    public interface IContentExtensionService : IDependency {
        Response StoreInspectExpando(ExpandoObject theExpando, ContentItem TheContentItem);
        bool FileAllowed(string filename);
        /// <summary>
        ///
        /// </summary>
        /// <param name="ContentType"></param>
        /// <param name="method">Get,Post,Delete,Publish</param>
        /// <param name="mycontent">Default null, da valorizzare nel caso in cui voglio testare own permission</param>
        /// <returns></returns>
        bool HasPermission(string ContentType, Methods method, IContent mycontent = null);
    }

    public class ContentExtensionService : IContentExtensionService {
        private string[] allowedFiles = new string[] { "jpg", "png", "gif", "doc", "docx", "xls", "xlsx", "pdf", "mov", "mp4", "mpg", "mpeg", "avi", "3gp", "mp4v", "m4v", "m4a", "aac", "jpeg", "bmp", "wmv", "wav", "mp3" };
        private string[] ProtectedPart = new string[] { "commonpart", "autoroutepart", "userrolespart" };
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IUtilsServices _utilsServices;
        private readonly IContentTypePermissionSettingsService _contentTypePermissionSettingsService;
        private readonly IOrchardServices _orchardServices;
        private readonly IRoleService _roleService;
        private readonly IEnumerable<IDumperHandler> _dumperHandlers;
        public ILogger Log { get; set; }

        public ContentExtensionService(IContentManager contentManager,
            ITaxonomyService taxonomyService,
            IUtilsServices utilsServices,
            IContentTypePermissionSettingsService contentTypePermissionSettingsService,
            IOrchardServices orchardServices,
            IRoleService roleService,
            IEnumerable<IDumperHandler> dumperHandlers) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
            Log = NullLogger.Instance;
            _utilsServices = utilsServices;
            _contentTypePermissionSettingsService = contentTypePermissionSettingsService;
            _orchardServices = orchardServices;
            _roleService = roleService;
            _dumperHandlers = dumperHandlers;
        }

        #region [Content Permission]
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

        private bool TestPermission(string permission, IContent mycontent = null) {
            bool testpermission = false;
            if (!string.IsNullOrEmpty(permission)) {
                Permission Permissiontotest = GetPermissionByName(permission);
                if (Permissiontotest != null) {
                    if (mycontent != null)
                        testpermission = _orchardServices.Authorizer.Authorize(Permissiontotest, mycontent);
                    else
                        testpermission = _orchardServices.Authorizer.Authorize(Permissiontotest);
                }
            }
            return testpermission;
        }

        public bool HasPermission(string ContentType, Methods method, IContent mycontent = null) {
            bool haspermission = false;
            List<ContentTypePermissionRecord> settings = _contentTypePermissionSettingsService.ReadSettings().ListContPermission.Where(x => x.ContentType == ContentType).ToList();
            if (settings != null && settings.Count > 0) {
                // test if exist one record in permission setting that enable user
                foreach (ContentTypePermissionRecord ctpr in settings) {
                    switch (method) {
                        case Methods.Get:
                            if (TestPermission(ctpr.GetPermission, mycontent))
                                return true;
                            break;

                        case Methods.Post:
                            if (TestPermission(ctpr.PostPermission, mycontent))
                                return true;
                            break;

                        case Methods.Publish:
                            if (TestPermission(ctpr.PublishPermission, mycontent))
                                return true;
                            break;

                        case Methods.Delete:
                            if (TestPermission(ctpr.DeletePermission, mycontent))
                                return true;
                            break;
                    }
                }
            } else {
                // test generic permission for contenttype
                switch (method) {
                    case Methods.Get:
                        return TestPermission("ViewContent", mycontent);

                    case Methods.Post:
                        return TestPermission("EditContent", mycontent);

                    case Methods.Publish:
                        return TestPermission("PublishContent", mycontent);

                    case Methods.Delete:
                        return TestPermission("DeleteContent", mycontent);
                }
            }

            return haspermission;
        }

        #endregion [Content Permission]

        public Response StoreInspectExpando(ExpandoObject theExpando, ContentItem theContentItem) {
            try {
                foreach (var kvp in theExpando) {
                    string key = kvp.Key.ToString();
                    string valueType = kvp.Value.GetType().Name;
                    object value = kvp.Value;
                    if (kvp.Value is ExpandoObject) {
                        StoreInspectExpando(kvp.Value as ExpandoObject, theContentItem);
                    }
                    _utilsServices.StoreInspectExpandoFields(theContentItem.Parts.ToList(), key, value);
                }
            } catch (Exception ex) {
                Log.Error("ContentExtension -> ContentExtensionService -> StoreInspectExpando : " + ex.Message + " <Stack> " + ex.StackTrace);
                return (_utilsServices.GetResponse(ResponseType.None, "Error:" + ex.Message));
            }
            return StoreInspectExpandoPart(theExpando, theContentItem);
        }

        private Response StoreInspectExpandoPart(ExpandoObject theExpando, ContentItem TheContentItem) {
            try {
                foreach (var kvp in theExpando) {
                    string key = kvp.Key.ToString();
                    if (key.IndexOf(".") > 0) {
                        string valueType = kvp.Value.GetType().Name;
                        object value = kvp.Value;
                        StoreLikeDynamic(key, value, TheContentItem);
                    }
                }
            } catch (Exception ex) {
                Log.Error("ContentExtension -> ContentExtensionService -> StoreInspectExpandoPart : " + ex.Message + " <Stack> " + ex.StackTrace);
                return _utilsServices.GetResponse(ResponseType.None, ex.Message);
            }
            return (_utilsServices.GetResponse(ResponseType.Success));
        }

        private void StoreLikeDynamic(string key, object value, ContentItem theContentItem) {
            string[] ListProperty = key.Split('.');
            if (!ProtectedPart.Contains(ListProperty[0].ToLower())) {
                foreach(var dumper in _dumperHandlers) {
                    dumper.StoreLikeDynamic(theContentItem, ListProperty, value);
                }
            }
        }

        public bool FileAllowed(string filename) {
            if (filename != null && filename.IndexOf('.') > 0)
                return allowedFiles.Contains(filename.ToLower().Split('.').Last());
            else
                return false;
        }
    }
}