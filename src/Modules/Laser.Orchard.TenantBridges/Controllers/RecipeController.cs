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
		public ActionResult Execute(string moduleId, string name) {
			if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage modules"))) {
				return new HttpUnauthorizedResult();
			}

			ModuleEntry module = _extensionManager.AvailableExtensions()
				.Where(extensionDescriptor => extensionDescriptor.Id == moduleId && ExtensionIsAllowed(extensionDescriptor))
				.Select(extensionDescriptor => new ModuleEntry { Descriptor = extensionDescriptor }).FirstOrDefault();

			if (module == null) {
				return HttpNotFound();
            }

			Recipe recipe = _recipeHarvester.HarvestRecipes(module.Descriptor.Id).FirstOrDefault(x => !x.IsSetupRecipe && x.Name == name);

			if (recipe == null) {
				return HttpNotFound();
			}

			try {
				_recipeManager.Execute(recipe);
            } catch(Exception ex) {
				Logger.Error(ex, "Error while executing recipe {0} in {1}", moduleId, name);
			}

			return new HttpUnauthorizedResult();
		}

		/// <summary>
		/// Checks whether the module is allowed for the current tenant
		/// </summary>
		private bool ExtensionIsAllowed(ExtensionDescriptor extensionDescriptor) {
			return _shellSettings.Modules.Length == 0 || _shellSettings.Modules.Contains(extensionDescriptor.Id);
		}
	}
}