using System.Collections.Generic;
using Orchard;
using Orchard.Users.Models;
using Orchard.Roles.Models;



namespace Laser.Orchard.StartupConfig.Services {
      
    public interface IRoleExtension:IDependency{
        IList<UserPartRecord> GetUsersByRoles(List<RoleRecord> rolelist);//List<RoleRecord> listroles);
    }
}


