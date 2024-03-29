﻿using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    [OrchardFeature("Laser.Orchard.StartupConfig.TinyMceEnhancement")]
    public static class Constants {
        public const string BasePlugins = @"advlist, anchor, autolink, autoresize, charmap, code, colorpicker, contextmenu, directionality, emoticons, fullscreen, hr, image, insertdatetime, link, lists, media, nonbreaking, pagebreak, paste, preview, print, searchreplace, table, template, textcolor, textpattern, visualblocks, visualchars, wordcount, htmlsnippets";
        public const string FrontendBasePlugins = @"advlist, anchor, autolink, autoresize, charmap, code, colorpicker, contextmenu, directionality, emoticons, fullscreen, hr, image, insertdatetime, link, lists, media, nonbreaking, pagebreak, paste, preview, print, searchreplace, table, template, textcolor, textpattern, visualblocks, visualchars, wordcount, htmlsnippets";
        public const string BaseLeftToolbar = @"undo redo cut copy paste | bold italic | bullist numlist outdent indent formatselect | alignleft aligncenter alignright alignjustify ltr rtl | ";
        public const string FrontendBaseLeftToolbar = @"undo redo cut copy paste | bold italic | bullist numlist outdent indent formatselect | alignleft aligncenter alignright alignjustify ltr rtl | ";
        public const string BaseRightToolbar = @" link unlink charmap | code htmlsnippetsbutton fullscreen";
        public const string FrontendBaseRightToolbar = @" link unlink charmap | code htmlsnippetsbutton fullscreen";
        public const string BasePartialInit = @"selector: ""textarea.tinymce"",
            theme: ""modern"",
            schema: ""html5"",
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
        public const string FrontendBasePartialInit = @"selector: ""textarea.tinymce"",
            theme: ""modern"",
            schema: ""html5"",
            convert_urls: false,
            valid_elements: ""*[*]"",
            //menubar: false,
            //statusbar: false,
            skin: ""orchardlightgray"",
            language: language,
            auto_focus: autofocus,
            directionality: directionality";
        public const string DefaultSetup = @"setup: function (editor) {
            $(document).bind(""localization.ui.directionalitychanged"", function (event, directionality) {
                if (editor.getBody() != null) {
                    editor.getBody().dir = directionality;
                }
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
        public const string FrontendDefaultSetup = @"setup: function (editor) {
            $(document).bind(""localization.ui.directionalitychanged"", function (event, directionality) {
                if (editor.getBody() != null) {
                    editor.getBody().dir = directionality;
                }
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
    }
}