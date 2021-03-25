using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Orchard;
using Orchard.Caching;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using Orchard.Utility.Extensions;

namespace Laser.Orchard.StartupConfig.ShapeTableProviders {
    public class TenantShapeBindingStrategy : IShapeTableProvider {
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IExtensionManager _extensionManager;
        private readonly ICacheManager _cacheManager;
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IEnumerable<IShapeTemplateHarvester> _harvesters;
        private readonly IEnumerable<IShapeTemplateViewEngine> _shapeTemplateViewEngines;
        private readonly IParallelCacheContext _parallelCacheContext;
        private readonly Work<ILayoutAwareViewEngine> _viewEngine;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ShellSettings _shellSettings;
        private readonly RequestContext _requestContext;

        public TenantShapeBindingStrategy(
            IEnumerable<IShapeTemplateHarvester> harvesters,
            ShellDescriptor shellDescriptor,
            IExtensionManager extensionManager,
            ICacheManager cacheManager,
            IVirtualPathMonitor virtualPathMonitor,
            IVirtualPathProvider virtualPathProvider,
            IEnumerable<IShapeTemplateViewEngine> shapeTemplateViewEngines,
            IParallelCacheContext parallelCacheContext,
            Work<ILayoutAwareViewEngine> viewEngine,
            IWorkContextAccessor workContextAccessor,
            ShellSettings shellSettings,
            RequestContext requestContext
            ) {

            _harvesters = harvesters;
            _shellDescriptor = shellDescriptor;
            _extensionManager = extensionManager;
            _cacheManager = cacheManager;
            _virtualPathMonitor = virtualPathMonitor;
            _virtualPathProvider = virtualPathProvider;
            _shapeTemplateViewEngines = shapeTemplateViewEngines;
            _parallelCacheContext = parallelCacheContext;
            _viewEngine = viewEngine;
            _workContextAccessor = workContextAccessor;
            _requestContext = requestContext;
            Logger = NullLogger.Instance;
            _shellSettings = shellSettings;
        }

        public ILogger Logger { get; set; }
        public bool DisableMonitoring { get; set; }

        private static IEnumerable<ExtensionDescriptor> Once(IEnumerable<FeatureDescriptor> featureDescriptors) {
            var once = new ConcurrentDictionary<string, object>();
            return featureDescriptors.Select(fd => fd.Extension).Where(ed => once.TryAdd(ed.Id, null)).ToList();
        }

        public void Discover(ShapeTableBuilder builder) {
            Logger.Information("Start discovering shapes");

            var harvesterInfos = _harvesters.Select(harvester => new { harvester, subPaths = harvester.SubPaths() });

            var availableFeatures = _extensionManager.AvailableFeatures();
            var activeThemes = availableFeatures.Where(FeatureIsTheme);
            var activeThemesExtensions = Once(activeThemes);

            var hits = _parallelCacheContext.RunInParallel(activeThemesExtensions, extensionDescriptor => {
                Logger.Information("Start discovering candidate views filenames");
                var pathContexts = harvesterInfos.SelectMany(harvesterInfo => harvesterInfo.subPaths.Select(subPath => {
                    var appDataPath = @"~/App_Data/Sites";
                    var tenantName = _shellSettings.Name;
                    var alternatesFolderName = "Alternates";

                    var basePath = Path.Combine(appDataPath, tenantName, alternatesFolderName, extensionDescriptor.Id).Replace(Path.DirectorySeparatorChar, '/');
                    var monitorPath = Path.Combine(appDataPath, tenantName, alternatesFolderName).Replace(Path.DirectorySeparatorChar, '/');
                    var virtualPath = Path.Combine(basePath, subPath).Replace(Path.DirectorySeparatorChar, '/');
                    IList<string> fileNames;
                    if (!_virtualPathProvider.DirectoryExists(virtualPath)) {
                        fileNames = new List<string>();
                    }
                    else {
                        fileNames = _cacheManager.Get(virtualPath, true, ctx => {

                            if (!DisableMonitoring) {
                                Logger.Debug("Monitoring virtual path \"{0}\"", virtualPath);
                                ctx.Monitor(_virtualPathMonitor.WhenPathChanges(virtualPath));
                            }

                            return _virtualPathProvider.ListFiles(virtualPath).Select(Path.GetFileName).ToReadOnlyCollection();
                        });
                    }
                    return new { harvesterInfo.harvester, basePath, subPath, virtualPath, fileNames };
                })).ToList();

                Logger.Information("Done discovering candidate views filenames");

                var fileContexts = pathContexts.SelectMany(pathContext => _shapeTemplateViewEngines.SelectMany(ve => {
                    var fileNames = ve.DetectTemplateFileNames(pathContext.fileNames);
                    return fileNames.Select(
                        fileName => new {
                            fileName = Path.GetFileNameWithoutExtension(fileName),
                            fileVirtualPath = Path.Combine(pathContext.virtualPath, fileName).Replace(Path.DirectorySeparatorChar, '/'),
                            pathContext
                        });
                }));

                var shapeContexts = fileContexts.SelectMany(fileContext => {
                    var harvestShapeInfo = new HarvestShapeInfo {
                        SubPath = fileContext.pathContext.subPath,
                        FileName = fileContext.fileName,
                        TemplateVirtualPath = fileContext.fileVirtualPath
                    };
                    var harvestShapeHits = fileContext.pathContext.harvester.HarvestShape(harvestShapeInfo);
                    return harvestShapeHits.Select(harvestShapeHit => new { harvestShapeInfo, harvestShapeHit, fileContext });
                });

                return shapeContexts.Select(shapeContext => new { extensionDescriptor, shapeContext }).ToList();
            }).SelectMany(hits2 => hits2);


            foreach (var iter in hits) {
                // templates are always associated with the namesake feature of module or theme
                var hit = iter;
                var featureDescriptors = iter.extensionDescriptor.Features.Where(fd => fd.Id == hit.extensionDescriptor.Id);
                foreach (var featureDescriptor in featureDescriptors) {
                    Logger.Debug("Binding {0} as shape [{1}] for feature {2}",
                        hit.shapeContext.harvestShapeInfo.TemplateVirtualPath,
                        iter.shapeContext.harvestShapeHit.ShapeType,
                        featureDescriptor.Id);
                    // set priority higher than theme
                    var newfeatureDescriptor = new FeatureDescriptor {
                        Id = featureDescriptor.Id,
                        Category = featureDescriptor.Category,
                        Description = featureDescriptor.Description,
                        Extension = featureDescriptor.Extension,
                        LifecycleStatus = featureDescriptor.LifecycleStatus,
                        Name = featureDescriptor.Name,
                        Priority = featureDescriptor.Priority + 100,
                    };
                    builder.Describe(iter.shapeContext.harvestShapeHit.ShapeType)
                        .From(new Feature { Descriptor = newfeatureDescriptor })
                        .BoundAs(
                            hit.shapeContext.harvestShapeInfo.TemplateVirtualPath,
                            shapeDescriptor => displayContext => Render(shapeDescriptor, displayContext, hit.shapeContext.harvestShapeInfo, hit.shapeContext.harvestShapeHit));
                }
            }
            Logger.Information("Done discovering shapes");
        }

        private bool FeatureIsTheme(FeatureDescriptor fd) {
            return DefaultExtensionTypes.IsTheme(fd.Extension.ExtensionType);
        }

        private IHtmlString Render(ShapeDescriptor shapeDescriptor, DisplayContext displayContext, HarvestShapeInfo harvestShapeInfo, HarvestShapeHit harvestShapeHit) {
            Logger.Information("Rendering template file '{0}'", harvestShapeInfo.TemplateVirtualPath);
            IHtmlString result;

            if (displayContext.ViewContext.View != null) {
                var htmlHelper = new HtmlHelper(displayContext.ViewContext, displayContext.ViewDataContainer);
                result = htmlHelper.Partial(harvestShapeInfo.TemplateVirtualPath, displayContext.Value);
            }
            else {
                // If the View is null, it means that the shape is being executed from a non-view origin / where no ViewContext was established by the view engine, but manually.
                // Manually creating a ViewContext works when working with Shape methods, but not when the shape is implemented as a Razor view template.
                // Horrible, but it will have to do for now.
                result = RenderRazorViewToString(harvestShapeInfo.TemplateVirtualPath, displayContext);
            }

            Logger.Information("Done rendering template file '{0}'", harvestShapeInfo.TemplateVirtualPath);
            return result;
        }

        private IHtmlString RenderRazorViewToString(string path, DisplayContext context) {
            using (var sw = new StringWriter()) {
                var controllerContext = CreateControllerContext();
                var viewResult = _viewEngine.Value.FindPartialView(controllerContext, path, false);

                context.ViewContext.ViewData = new ViewDataDictionary(context.Value);
                context.ViewContext.TempData = new TempDataDictionary();
                context.ViewContext.View = viewResult.View;
                context.ViewContext.RouteData = controllerContext.RouteData;
                context.ViewContext.RequestContext.RouteData = controllerContext.RouteData;
                viewResult.View.Render(context.ViewContext, sw);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return new HtmlString(sw.GetStringBuilder().ToString());
            }
        }

        private ControllerContext CreateControllerContext() {
            var controller = new StubController();
            var httpContext = _workContextAccessor.GetContext().Resolve<HttpContextBase>();
            var requestContext = _workContextAccessor.GetContext().Resolve<RequestContext>();
            var routeData = requestContext.RouteData;

            routeData.DataTokens["IWorkContextAccessor"] = _workContextAccessor;

            if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
                routeData.Values.Add("controller", controller.GetType().Name.ToLower().Replace("controller", ""));

            controller.ControllerContext = new ControllerContext(httpContext, routeData, controller);
            controller.ControllerContext.RequestContext = requestContext;
            return controller.ControllerContext;
        }

        private class StubController : Controller { }
    }
}
