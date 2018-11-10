using Laser.Orchard.ShortLinks.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.ShortLinks.Services {
    public interface IShortLinksService : IDependency {
       

	   /// <summary>
	   /// 
	   /// </summary>
	   /// <param name="part"></param>
	   /// <returns></returns>
        string GetFullAbsoluteUrl(ContentPart part);
        string GetShortLink(ContentPart part);
        string GetShortLink(string myurl);
    }
}