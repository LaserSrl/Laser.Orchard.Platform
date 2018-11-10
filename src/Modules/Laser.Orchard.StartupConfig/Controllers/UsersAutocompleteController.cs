using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Security;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.Controllers {

    public class UsersAutocompleteController : Controller {
        private readonly IUserSearchService _userSearchService;

        public UsersAutocompleteController(IUserSearchService userSearchService) {
            _userSearchService = userSearchService;
        }

        public ActionResult Search(string searchText) {
            List<SearchResult> elenco = new List<SearchResult>();
            foreach (var usr in _userSearchService.SearchByNameOrEmail(searchText)) {
                elenco.Add(new SearchResult {
                    UserName = usr.UserName,
                    Email = usr.Email
                });
            }
            return Json(elenco);
        }

        public ActionResult SearchRole(string searchText, string UserRole) {
            return Json(_userSearchService.SearchByNameOrEmail(searchText, UserRole).Select(usr => new SearchResult {
                UserName = usr.UserName,
                Email = usr.Email
            }).ToList());
        }

        //**********************************************
        // classe di utility per la creazione del risultato in formato json
        public class SearchResult {
            public string UserName { get; set; }
            public string Email { get; set; }
        }
    }
}