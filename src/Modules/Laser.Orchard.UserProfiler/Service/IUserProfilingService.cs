using Laser.Orchard.UserProfiler.ViewModels;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Service {

    public interface IUserProfilingService : IDependency {

        //    Dictionary<string, int> UpdateUserProfile(int UserId, string text, TextSourceTypeOptions sourceType, int count);
        //    Dictionary<string, int> UpdateUserProfile(int UserId, List<ProfileVM> update);
        Dictionary<string, int> UpdateUserProfile(int UserId, int id);

        Dictionary<string, int> UpdateUserProfile(int UserId, List<int> ids);

        object GetList(int UserId);

        void RefreshTotal(int UserId);
    }
}