using System.Web.Mvc;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Services;
using Orchard.Security;
using System.Linq;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions.Models;
using Orchard.Modules.Models;
using Orchard.Recipes.Models;
using System;
using Orchard.Mvc.AntiForgery;
using Laser.Orchard.TenantBridges.Security;
using System.Net;

namespace Laser.Orchard.TenantBridges.Controllers {
	[Authorize]
	public class RecipeController : Controller {
		private readonly IRecipeHarvester _recipeHarvester;
		private readonly IRecipeManager _recipeManager;
		private readonly IExtensionManager _extensionManager;
		private readonly ShellSettings _shellSettings;

		public RecipeController(
			IRecipeHarvester recipeHarvester,
			IRecipeManager recipeManager,
			IOrchardServices services,
			IExtensionManager extensionManager,
			ShellSettings shellSettings) {

			_recipeHarvester = recipeHarvester;
			_recipeManager = recipeManager;
			Services = services;
			_extensionManager = extensionManager;
			_shellSettings = shellSettings;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
		}

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }
		public IOrchardServices Services { get; set; }

		[HttpPost]
		[ValidateAntiForgeryTokenOrchard(false)]
		public JsonResult Execute(string moduleId, string name) {
			var user = Services.WorkContext.CurrentUser;
			if (user != null) {
				Logger.Information("User {0} is trying to remotely execute recipe {1} in module {2}.",
					user.UserName, name, moduleId);
            } else {
				Logger.Information("Anonymous user is trying to remotely execute recipe {0} in module {1}.",
					name, moduleId);
			}
			if (!Services.Authorizer.Authorize(ExecuteRemoteRecipesPermission.ExecuteRemoteRecipes, T("Not allowed to remotely execute recipes."))) {
				Logger.Information("User is not allowed to remotely execute recipes.");
				return GetUnauthorizedResult();
			}

			ModuleEntry module = _extensionManager.AvailableExtensions()
				.Where(extensionDescriptor => extensionDescriptor.Id == moduleId && 
					ExtensionIsAllowed(extensionDescriptor))
				.Select(extensionDescriptor => new ModuleEntry { Descriptor = extensionDescriptor })
				.FirstOrDefault();

			if (module == null) {
				Logger.Error("Module {0} not found", moduleId);
				return GetNotFoundResult(T("Module not found"));
            }

			Recipe recipe = _recipeHarvester
				.HarvestRecipes(module.Descriptor.Id)
				.FirstOrDefault(x => !x.IsSetupRecipe && x.Name == name);

			if (recipe == null) {
				Logger.Error("Recipe {0} not found in module {1}", name, moduleId);
				return GetNotFoundResult(T("Recipe not found"));
			}

			try {
				_recipeManager.Execute(recipe);
            } catch(Exception ex) {
				Logger.Error(ex, "Error while executing recipe {0} in {1}", name, moduleId);
				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new JsonResult {
					Data = T("Error while executing recipe {0} in {1}: {2}", name, moduleId, ex.Message)
				};
			}

			Response.StatusCode = (int)HttpStatusCode.OK;
			return new JsonResult {
				Data = "Recipe succesfully executed"
			};
		}

		/// <summary>
		/// Checks whether the module is allowed for the current tenant
		/// </summary>
		private bool ExtensionIsAllowed(ExtensionDescriptor extensionDescriptor) {
			return _shellSettings.Modules.Length == 0 || _shellSettings.Modules.Contains(extensionDescriptor.Id);
		}

		private JsonResult GetUnauthorizedResult() {
			Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			// prevent IIS 7.0 classic mode from handling the 401 itself
			Response.SuppressFormsAuthenticationRedirect = true;
			return new JsonResult {
				Data = T("Not allowed to remotely execute recipes.")
			};
		}

		private JsonResult GetNotFoundResult(LocalizedString errorString) {
			Response.StatusCode = (int)HttpStatusCode.NotFound;
			return new JsonResult {
				Data = errorString
			};
        }
	}
}