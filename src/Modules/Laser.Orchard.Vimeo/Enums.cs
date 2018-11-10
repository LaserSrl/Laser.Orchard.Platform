using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Vimeo {
    public enum VerifyUploadResult { CompletedAlready, Complete, Incomplete, StillUploading, NeverExisted, Error }

    public enum VimeoPrivacyViewOption { anybody, nobody, contacts, password, users, unlisted, disable }
    public enum VimeoPrivacyCommentsOption { anybody, nobody, contacts }
    //public enum VimeoPrivacyEmbedOption { _public, _private, _whitelist }

    /// <summary>
    /// Using static objects rather than an enum for this because the values we want (public and private) are
    /// also keywords, and we cannot use them directly in an enum. An alternative to this would be writing a
    /// class with an operator to "translate" enum values.
    /// </summary>
    public sealed class VimeoPrivacyEmbedOption {
        private readonly string name;

        public static readonly VimeoPrivacyEmbedOption Public = new VimeoPrivacyEmbedOption("public");
        public static readonly VimeoPrivacyEmbedOption Private = new VimeoPrivacyEmbedOption("private");
        public static readonly VimeoPrivacyEmbedOption Whitelist = new VimeoPrivacyEmbedOption("whitelist");

        private VimeoPrivacyEmbedOption(string n) {
            name = n;
        }

        public override string ToString() {
            return name;
        }

        public static List<SelectListItem> GetValues() {
            return (new SelectListItem[] { 
                new SelectListItem{Selected=false, Text = Public.ToString(), Value=Public.ToString()},
                new SelectListItem{Selected=false, Text = Private.ToString(), Value=Private.ToString()},
                new SelectListItem{Selected=false, Text = Whitelist.ToString(), Value=Whitelist.ToString()},
            }).ToList();
        }
    }
}