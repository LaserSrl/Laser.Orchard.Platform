using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security.Permissions;
using Orchard.Environment.Extensions.Models;

namespace Laser.Orchard.FidelityGateway
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission AddPointsToHimself = new Permission { Description = "Add Points To Himself", Name = "AddPointsToHimself" };
        public static readonly Permission AddPointsToOtherUsers = new Permission { Description = "Add Points To Other Users", Name = "AddPointsToOtherUsers" };
        public static readonly Permission GiveRewardToHimself = new Permission { Description = "Give Reward To Himself", Name = "GivePointsToHimself" };
        public static readonly Permission GiveRewardToOtherUsers = new Permission { Description = "Give Reward To Other Users", Name = "GivePointsToOtherUsers" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
            AddPointsToHimself,
            AddPointsToOtherUsers,
            GiveRewardToOtherUsers,
            GiveRewardToHimself,
        };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {            
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {AddPointsToHimself, AddPointsToOtherUsers, GiveRewardToHimself, GiveRewardToOtherUsers } 
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {AddPointsToHimself, AddPointsToOtherUsers, GiveRewardToHimself, GiveRewardToOtherUsers } 
                },
            };
        }
    }
}