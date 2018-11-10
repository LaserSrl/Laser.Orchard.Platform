//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Orchard.Localization;
//using Orchard.UI.Navigation;

//namespace Belitsoft.Orchard.Faq.Navigation {
//    public class FaqNavigationProvider : INavigationProvider {
//        public FaqNavigationProvider() {
//            T = NullLocalizer.Instance;
//        }

//        public Localizer T { get; set; }

//        public string MenuName {
//            get { return "admin"; }
//        }

//        public void GetNavigation(NavigationBuilder builder) {
//            builder.AddImageSet("faqicons")
//            .Add(item => item
//                .Caption(T("Faq"))
//                .Position("3")
//                .Action("List", "Contents", new { area = "Admin" })
//            );
//        }
//    }
//}