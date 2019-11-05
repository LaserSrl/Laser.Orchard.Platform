using Orchard;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Compilation;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Diagnostics;

namespace Laser.Orchard.StartupConfig.RazorCodeExecution.Services {

    public interface IRazorTemplateManager : ISingletonDependency {

        string RunString(string key, string code, Object model, Dictionary<string, object> dicdvb = null, string Layout = null);

        string RunFile(string localFilePath, RazorModelContext model);
        string CompileFile(string localFilePath);


        //   void AddLayout(string key, string code);

        void StartNewRazorEngine();

        List<string> GetListCached();
        List<string> GetListOldCached();

    }

    public class RazorTemplateManager : IRazorTemplateManager {

        public RazorTemplateManager() {
            listCached = new List<string>();
            listOldCached = new List<string>();
        }

        private List<string> listCached;
        private List<string> listOldCached;
        public List<string> GetListCached() {
            return listCached;
        }

        public List<string> GetListOldCached() {
            return listOldCached;
        }

        public IRazorEngineService RazorEngineServiceStatic {
            get {
                if (_razorEngine == null) {
                    StartNewRazorEngine();
                }
                return _razorEngine;
            }
        }

        public void StartNewRazorEngine() {
            var config = new TemplateServiceConfiguration();
#if DEBUG
            config.Debug = true;
#endif
            config.Namespaces.Add("Orchard");
            config.Namespaces.Add("Orchard.ContentManagement");
            config.Namespaces.Add("Orchard.Caching");
            //config.Namespaces.Add("System.Web.Helpers");
            config.ReferenceResolver = new MyIReferenceResolver();

            _razorEngine = RazorEngineService.Create(config);
            listOldCached.AddRange(listCached);
            listCached = new List<string>();

        }

        private IRazorEngineService _razorEngine;

        //public void AddLayout(string key, string code) {


        //    if (!RazorEngineServiceStatic.IsTemplateCached(key, null)) {
        //        RazorEngineServiceStatic.AddTemplate(key, code);// new LoadedTemplateSource(code, null));
        // //    RazorEngineServiceStatic.Compile(key);
        // //       listCached.Add(key);
        //    }
        //}

        public string RunString(string key, string code, Object model, Dictionary<string, object> dicdvb = null, string Layout = null) {
            DynamicViewBag dvb = new DynamicViewBag();
            if (dicdvb != null)
                foreach (var k in dicdvb.Keys) {
                    dvb.AddValue(k, dicdvb[k]);
                }
            if (!RazorEngineServiceStatic.IsTemplateCached(key, null)) {
                if (!string.IsNullOrEmpty(code)) {
                    if (!string.IsNullOrEmpty(Layout)) {
                        RazorEngineServiceStatic.AddTemplate("Layout" + key, Layout);
                    }
#if DEBUG
                    // key puo' essere un nome ("caso customtemplate") o un file con tutta la path (caso fieldexternal)
                    string defFileName = key;
                    try {
                        defFileName = Path.GetFileName(key);
                    }
                    catch { }
                    if (string.IsNullOrEmpty(defFileName))
                        defFileName = key;
                    defFileName = System.IO.Path.GetTempPath() + defFileName + ".cshtml";
                    code = "@{System.Diagnostics.Debugger.Break();}" + code;
                    File.WriteAllText(defFileName, code);

                    RazorEngineServiceStatic.AddTemplate(key, new LoadedTemplateSource(code, defFileName));
                    // Debugger.Break();

#else
                    RazorEngineServiceStatic.AddTemplate(key, new LoadedTemplateSource(code, null));
#endif
                    RazorEngineServiceStatic.Compile(key, null);
                    listCached.Add(key);
                }
                else
                    return null;
            }

            return RazorEngineServiceStatic.Run(key, null, (Object)model, dvb);
        }

        public string CompileFile(string localFilePath) {
            try {
                string key = "Test" + Guid.NewGuid();
                if (File.Exists(localFilePath)) {
                    DateTime d = System.IO.File.GetLastWriteTime(localFilePath);
                    key += d.ToShortDateString() + d.ToLongTimeString();
                }
                string codeTemplate = "";
                if (!RazorEngineServiceStatic.IsTemplateCached(key, null)) {
                    if (System.IO.File.Exists(localFilePath)) {
                        string myfile2 = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\common.cshtml";
                        codeTemplate = File.ReadAllText(myfile2) + File.ReadAllText(localFilePath);
                        if (!string.IsNullOrEmpty(codeTemplate)) {
                            RazorEngineServiceStatic.AddTemplate(key, new LoadedTemplateSource(codeTemplate, localFilePath));
                            RazorEngineServiceStatic.Compile(key, null);
                            listCached.Add(localFilePath);
                        }
                        else
                            return "";
                    }
                    else
                        return "";
                }
                return "";
            }
            catch (Exception ex) {
                return ex.Message;
            }
        }
        public string RunFile(string localFilePath, RazorModelContext model) {
            string key = localFilePath;
            /*
              if (File.Exists(Path.GetFullPath(localFilePath) + "Reset.txt")) {
                  StartNewRazorEngine();
                  File.Delete(Path.GetFullPath(localFilePath) + "Reset.txt");
              }
              */
            if (File.Exists(localFilePath)) {
                DateTime d = System.IO.File.GetLastWriteTime(localFilePath);
                key += d.ToShortDateString() + d.ToLongTimeString();
            }
            string codeTemplate = "";
            if (!RazorEngineServiceStatic.IsTemplateCached(key, null)) {
                if (System.IO.File.Exists(localFilePath)) {
                    codeTemplate = File.ReadAllText(localFilePath);
                    if (!string.IsNullOrEmpty(codeTemplate)) {
                        RazorEngineServiceStatic.AddTemplate(key, new LoadedTemplateSource(codeTemplate, localFilePath));
                        RazorEngineServiceStatic.Compile(key, null);
                        listCached.Add(localFilePath);
                    }
                    else
                        return "";
                }
                else
                    return "";
            }
            string result = "";
#if DEBUG
            codeTemplate = File.ReadAllText(localFilePath);
            result = RazorEngineServiceStatic.RunCompile(new LoadedTemplateSource(codeTemplate, localFilePath), Guid.NewGuid().ToString(), null, (Object)model);
            // non uso la cache in modo da permettere il debug anche nel caso in cui il razor venga caricato più volte
#else
             result = RazorEngineServiceStatic.Run(key, null, (Object)model);
#endif
            string resultnobr = (result ?? "").Replace("\r\n", "").Replace(" ", "");
            if (!string.IsNullOrEmpty(resultnobr))
                return result;
            else
                return "";
        }


    }


    // implementato IReferenceResolver perchè nel metodo nativo sceglieva la first fra le dll basandosi sul nome
    // implementato in modo che scelga l'ultima versione, modifica dovuta al fatto che ogni tanto il razor andava in errore dicendo che json non esisteva come metodo in system.web.helpers
    // le dll su cui viene fatta la ricerca includone quelle in dependecies e nella gac.
    public class MyIReferenceResolver : IReferenceResolver {
        public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies = null) {

            var allAssembly = CompilerServicesUtility
                  .GetLoadedAssemblies();

            var AssemblyToReference = allAssembly
                    .Where(a => !a.IsDynamic && File.Exists(a.Location) && !a.Location.Contains("CompiledRazorTemplates.Dynamic") && a.FullName.IndexOf("System.Web.Helpers") < 0)//(CompilerServiceBase.DynamicTemplateNamespace))
                       .GroupBy(a => a.GetName().Name).Select(grp => grp.First(y => y.GetName().Version == grp.Max(x => x.GetName().Version))) // only select distinct assemblies based on FullName to avoid loading duplicate assemblies
                       .Select(a => CompilerReference.From(a))
               .Concat(includeAssemblies ?? Enumerable.Empty<CompilerReference>());
            yield return CompilerReference.From(HostingEnvironment.ApplicationPhysicalPath + @"Modules\Laser.Orchard.StartupConfig\bin\System.Web.Helpers.dll");
            foreach (var assembly in AssemblyToReference)
                yield return assembly;

        }
    }
}