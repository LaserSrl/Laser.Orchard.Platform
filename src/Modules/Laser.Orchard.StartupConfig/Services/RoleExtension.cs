using System.Collections.Generic;
using Orchard.Users.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System.Linq;
using Orchard.Roles.Models;
using System;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.StartupConfig.RoleExtension")]
    public class RoleExtension : IRoleExtension {
        private readonly ITransactionManager _transactionManager;

        public RoleExtension(ITransactionManager transactionManager) {
            _transactionManager = transactionManager;
        }
     
        public IList<UserPartRecord> GetUsersByRoles(List<RoleRecord> rolelist) {//List<RoleRecord> listroles) {
            //var users = (from Orchard_Users_UserPartRecord in Session.Linq<Orchard_Users_UserPartRecord>()
            //              where cat.Mother.Id == motherId
            //              select new { cat.birthDate, cat.mother }).ToList();
            string query = "SELECT " +
                                "u.* " +
                            "FROM " +
                                "Orchard_Users_UserPartRecord u " +
                                "INNER JOIN Orchard_Roles_UserRolesPartRecord ur on u.Id = ur.UserId " +
                                "INNER JOIN Orchard_Roles_RoleRecord r on ur.Role_id = r.Id " +
                            "WHERE " +
                                "r.Name <> 'ExcludeRole' " +
                               "and ur.Role_id in (" + System.String.Join(",", Array.ConvertAll(rolelist.ToArray(), i => i.Id.ToString())) + ")";
                               
           var users  = _transactionManager.GetSession()
            .CreateSQLQuery(query )
            .AddEntity(typeof(UserPartRecord))
            .List<UserPartRecord>();
      // List<UserPartRecord>  returningList=new List<UserPartRecord>(users)
    //   List<UserPartRecord>  returningList=new List<UserPartRecord>();
             return (users);
        }
    }
}

