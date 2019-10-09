using System;
using System.Globalization;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    [OrchardFeature("Laser.Orchard.StartupConfig.TinyMceEnhancement")]
    public class TinyMceShapeDisplayEvent : ShapeDisplayEvents {
        private readonly IOrchardServices _orchardServices;
        public TinyMceShapeDisplayEvent(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public override void Displayed(ShapeDisplayedContext context) {
            // TODO aggiungere anche i field che usano TinyMce (verificare se serve)
            if (String.CompareOrdinal(context.ShapeMetadata.Type, "Body_Editor") != 0) {
                return;
            }

            if (!String.Equals(context.Shape.EditorFlavor, "html", StringComparison.InvariantCultureIgnoreCase)) {
                return;
            }
            base.Displayed(context);
            var settings = _orchardServices.WorkContext.CurrentSite.As<TinyMceSiteSettingsPart>();

            var htmlOrig = context.ChildContent.ToHtmlString();
            //html = @"<script>
            //        var lasertoolbar = 'undo redo cut copy paste | bold italic | bullist numlist outdent indent formatselect | alignleft aligncenter alignright alignjustify ltr rtl | ' + mediaPlugins + ' link unlink charmap | code fullscreen';
            //    </script>" + html;
            var shellDescriptor = _orchardServices.WorkContext.Resolve<ShellDescriptor>();
            var mediaPickerEnabled = shellDescriptor.Features.Any(x => x.Name == "Orchard.MediaPicker") ? true : false;
            var mediaLibraryEnabled = shellDescriptor.Features.Any(x => x.Name == "Orchard.MediaLibrary") ? true : false;
            var mediaPlugins = "";
            var mediaToolbar = "";
            if (mediaPickerEnabled) {
                mediaPlugins += ", mediapicker";
                mediaToolbar += " mediapicker";
            }
            if (mediaLibraryEnabled) {
                mediaPlugins += ", medialibrary";
                mediaToolbar += " medialibrary";
            }
            var defaultInit = @"selector: ""textarea.tinymce"",
                        theme: ""modern"",
                        schema: ""html5"",
                        plugins: [""advlist, anchor, autolink, autoresize, charmap, code, colorpicker, contextmenu, directionality, emoticons, fullscreen, hr, image, insertdatetime, link, lists, media, nonbreaking, pagebreak, paste, preview, print, searchreplace, table, template, textcolor, textpattern, visualblocks, visualchars, wordcount" + mediaPlugins + @"""],
                        toolbar: ""undo redo cut copy paste | bold italic | bullist numlist outdent indent formatselect | alignleft aligncenter alignright alignjustify ltr rtl | " + mediaToolbar + @" link unlink charmap | code fullscreen"",
                        convert_urls: false,
                        valid_elements: ""*[*]"",
                        // Shouldn't be needed due to the valid_elements setting, but TinyMCE would strip script.src without it.
                        extended_valid_elements: ""script[type|defer|src|language]"",
                        //menubar: false,
                        //statusbar: false,
                        skin: ""orchardlightgray"",
                        language: language,
                        auto_focus: autofocus,
                        directionality: directionality";
            var defaultSetup = @"setup: function (editor) {
                $(document).bind(""localization.ui.directionalitychanged"", function (event, directionality) {
                    editor.getBody().dir = directionality;
                });

                // If the focused editable area is taller than the window, make the menu and the toolbox sticky-positioned within the editor
                // to help the user avoid excessive vertical scrolling.
                // There is a built-in fixed_toolbar_container option in the TinyMCE, but we can't use it, because it is only
                // available if the selector is a DIV with inline mode.

                editor.on(""focus"", function () {
                    var $contentArea = $(this.contentAreaContainer.parentElement);
                    stickyToolbar($contentArea);
                });

                editor.on(""blur"", function () {
                    var $contentArea = $(this.contentAreaContainer.parentElement);
                    $contentArea.prepend($contentArea.find(""div.mce-toolbar-grp""));
                    $contentArea.prepend($contentArea.find(""div.mce-menubar""));
                    $(""#stickyContainer"").remove();
                    $(""#stickyPlaceholder"").remove();
                });

                function stickyToolbar($contentArea) {
                    var $container = $(""<div/>"", { id: ""stickyContainer"", class: ""container-layout"" });

                    $contentArea.prepend($container);
                    $container.append($contentArea.find(""div.mce-menubar""));
                    $container.append($contentArea.find(""div.mce-toolbar-grp""));

                    var $containerPosition = $container.offset();
                    var $placeholder = $(""<div/>"", { id: ""stickyPlaceholder"" });
                    var isAdded = false;

                    if ($(window).scrollTop() >= $containerPosition.top && !isAdded) {
                        $container.addClass(""sticky-top"");
                        $placeholder.insertBefore($container);
                        $container.width($placeholder.width());
                        $placeholder.height($container.height());
                    }

                    $(window).scroll(function (event) {
                        var $statusbarPosition = $contentArea.find(""div.mce-statusbar"").offset();
                        if ($(window).scrollTop() >= $containerPosition.top && !isAdded) {
                            $container.addClass(""sticky-top"");
                            $placeholder.insertBefore($container);
                            $container.width($placeholder.width());
                            $placeholder.height($container.height());
                            $(window).on(""resize"", function () {
                                $container.width($placeholder.width());
                                $placeholder.height($container.height());
                            });
                            isAdded = true;
                        } else if ($(window).scrollTop() < $containerPosition.top && isAdded) {
                            $container.removeClass(""sticky-top"");
                            $placeholder.remove();
                            $(window).on(""resize"", function () {
                                $container.width(""100%"");
                            });
                            isAdded = false;
                        }
                        if ($(window).scrollTop() >= ($statusbarPosition.top - $container.height())) {
                            $container.hide();
                        } else if ($(window).scrollTop() < ($statusbarPosition.top - $container.height()) && isAdded) {
                            $container.show();
                        }
                    });
                }
            }";
            var init = "";
            if(string.IsNullOrWhiteSpace(settings.InitScript)) {
                init = defaultInit;
            }
            else {
                init = settings.InitScript;
            }
            var html = @"<script type=""text/javascript"">
                $(function() {
                    tinyMCE.init({
                        " + init + @",
                        " + defaultSetup + @"
                    });
                });
                </script>" + htmlOrig;
            context.ChildContent = new HtmlString(html);
        }
    }
}