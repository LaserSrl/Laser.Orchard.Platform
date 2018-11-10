using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Laser.Orchard.GoogleAnalytics {
	public class Permissions : IPermissionProvider {
		public static readonly Permission ConfigureGoogleAnalytics = new Permission { Description = "Configure Google Analytics", Name = "ConfigureGoogleAnalytics" };

		public virtual Feature Feature { get; set; }

		public IEnumerable<Permission> GetPermissions() {
			return new[] { ConfigureGoogleAnalytics };
		}

		public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
			return new[] { new PermissionStereotype { Name = "Administrator", Permissions = new[] {ConfigureGoogleAnalytics} } };
		}
	}
}