﻿using Orchard.UI.Resources;

namespace Laser.Orchard.jQueryPlugins {

    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {

            var manifest = builder.Add();

            //Scripts
            manifest.DefineScript("jQuery_DataTables").SetUrl("jquery.dataTables.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_textcounter").SetUrl("textcounter.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_ImagePicker").SetUrl("image-picker.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_Print").SetUrl("jquery.print.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_Cycle").SetUrl("jquery.cycle.all.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_Cycle2").SetUrl("jquery.cycle2.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_MultiSelect").SetUrl("jquery.multiSelect.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_Tools").SetUrl("jquery.tools.min.js").SetDependencies("jQuery", "jQueryUI_Core", "jQueryMigrate");
            //   manifest.DefineScript("jQuery_MultiSelect").SetUrl("jquery.multiSelect.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_CarouFredSel").SetUrl("jquery.carouFredSel.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_Carousel").SetUrl("jquery.carousel.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_Contactable").SetUrl("jquery.contactable.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_UI_Multiselect_Widget").SetUrl("jquery.ui.multiselect.widget.min.js").SetDependencies("jQuery", "jQueryUI_Widget", "jQueryUI_Core", "jQueryUI_Position");

            manifest.DefineScript("jQuery_Validate_Unobtrusive").SetUrl("jquery.validate.unobtrusive.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_Validate").SetUrl("jquery.validate.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_Validate_Vsdoc").SetUrl("jquery.validate-vsdoc.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_Unobtrusive_Ajax").SetUrl("jquery.unobtrusive-ajax.min.js").SetDependencies("jQuery");

            manifest.DefineScript("jQueryUI_DatePicker_it").SetUrl("jquery.ui.datepicker-it.js").SetDependencies("jQuery", "jQueryUI_DatePicker");
            manifest.DefineScript("jQueryUI_DatePicker_fr").SetUrl("jquery.ui.datepicker-fr.js").SetDependencies("jQuery", "jQueryUI_DatePicker");
            manifest.DefineScript("jQueryUI_DatePicker_en").SetUrl("jquery.ui.datepicker-en.js").SetDependencies("jQuery", "jQueryUI_DatePicker");
            manifest.DefineScript("jQueryUI_DatePicker_de").SetUrl("jquery.ui.datepicker-de.js").SetDependencies("jQuery", "jQueryUI_DatePicker");
            manifest.DefineScript("jQueryUI_DatePicker_es").SetUrl("jquery.ui.datepicker-es.js").SetDependencies("jQuery", "jQueryUI_DatePicker");
            manifest.DefineScript("jQueryUI_DatePicker_ru").SetUrl("jquery.ui.datepicker-ru.js").SetDependencies("jQuery", "jQueryUI_DatePicker");
            manifest.DefineScript("jQuery_NestedModels").SetUrl("custom/jquery.nestedmodels.js").SetDependencies("jQuery");

            manifest.DefineScript("Jssor_Core").SetUrl("Jssor/jssor.core.js").SetDependencies("jQuery");
            manifest.DefineScript("Jssor_Slider").SetUrl("Jssor/jssor.slider.js").SetDependencies("jQuery");
            manifest.DefineScript("Jssor_Utils").SetUrl("Jssor/jssor.utils.js").SetDependencies("jQuery");
            manifest.DefineScript("Jssor_Mini").SetUrl("Jssor/jssor.slider.mini.js").SetDependencies("jQuery");

            manifest.DefineScript("jsTree").SetUrl("jsTree/jstree.js").SetDependencies("jQuery");
            manifest.DefineScript("jsTree_Mini").SetUrl("jsTree/jstree.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jsTreeGrid").SetUrl("jsTree/jstreegrid.js").SetDependencies("jsTree");

            manifest.DefineScript("rcswitcher").SetUrl("rcswitcher.min.js").SetDependencies("jQuery");

            manifest.DefineScript("jqPlot").SetUrl("jqPlot/jquery.jqplot.js").SetDependencies("jQuery");
            manifest.DefineScript("jqPlot_Mini").SetUrl("jqPlot/jquery.jqplot.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jqPlotPieChart").SetUrl("jqPlot/plugins/jqplot.pieRenderer.js").SetDependencies("jqPlot");
            manifest.DefineScript("jqPlotEnhancedPieLegendRenderer").SetUrl("jqPlot/plugins/jqplot.enhancedPieLegendRenderer.js").SetDependencies("jqPlot");

            manifest.DefineScript("jsonViewer").SetUrl("json-browse/jquery.json-browse.js").SetDependencies("jQuery");
            manifest.DefineScript("animsition").SetCdn("https://cdnjs.cloudflare.com/ajax/libs/animsition/4.0.2/js/animsition.min.js", "https://cdnjs.cloudflare.com/ajax/libs/animsition/4.0.2/js/animsition.js").SetDependencies("jQuery");

            manifest.DefineScript("Select2")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.7/js/select2.min.js")
                .SetDependencies("jQuery");

            manifest.DefineScript("ChartJS")
                .SetCdn("https://cdn.jsdelivr.net/npm/chart.js@2.8.0");

            //Styles

            manifest.DefineStyle("jQuery_DataTables").SetUrl("jqDataTable/jquery.dataTables.min.css");
            manifest.DefineStyle("jQuery_ImagePicker_Low").SetUrl("image-picker_low.css");
            manifest.DefineStyle("jQuery_ImagePicker").SetUrl("image-picker.css");
            manifest.DefineStyle("jQuery_MultiSelect").SetUrl("jquery.multiSelect.css");
            manifest.DefineStyle("jQuery_Autocomplete").SetUrl("jquery.autocomplete.css");
            manifest.DefineStyle("jQuery_Cycle").SetUrl("jquery.cycle.css");
            manifest.DefineStyle("jQuery_CarouFredSel_Responsive").SetUrl("jquery.caroufredsel.responsive.css");
            manifest.DefineStyle("jQuery_UI_Multiselect_Widget").SetUrl("jquery.ui.multiselect.widget.css").SetDependencies("jQueryUI_Orchard");
            manifest.DefineStyle("DivTableLayouts").SetUrl("custom/divtablelayouts.css");

            manifest.DefineStyle("Jssor_BannerSlider").SetUrl("Jssor/jssor.bannerslider.css");

            manifest.DefineStyle("jsTree_Default").SetUrl("jsTree/Default/style.css");
            manifest.DefineStyle("jsTree_Default_Mini").SetUrl("jsTree/Default/style.min.css");
            manifest.DefineStyle("jsTree_DefaultDark").SetUrl("jsTree/DefaultDark/style.css");
            manifest.DefineStyle("jsTree_DefaultDark_Mini").SetUrl("jsTree/DefaultDark/style.min.css");

            manifest.DefineStyle("Accordion").SetUrl("accordion.css");

            manifest.DefineStyle("rcswitcher").SetUrl("rcswitcher.min.css");

            manifest.DefineStyle("jqPlot").SetUrl("jqPlot/jquery.jqplot.css");
            manifest.DefineStyle("jqPlot_Mini").SetUrl("jqPlot/jquery.jqplot.min.css");
            manifest.DefineStyle("jsonViewer").SetUrl("json-browse/jquery.json-browse.css");
            manifest.DefineStyle("animsition").SetCdn("https://cdnjs.cloudflare.com/ajax/libs/animsition/4.0.2/css/animsition.css", "https://cdnjs.cloudflare.com/ajax/libs/animsition/4.0.2/css/animsition.min.css");

            manifest.DefineStyle("Select2")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.7/css/select2.min.css");

            // Bootstrap 
            manifest.DefineStyle("BootstrapGlyphicons")
                .SetUrl("bootstrap4-glyphicons/css/bootstrap-glyphicons.min.css", "bootstrap4-glyphicons/css/bootstrap-glyphicons.css");
            manifest.DefineStyle("GlyphiconsFontawesome")
                .SetUrl("bootstrap4-glyphicons/maps/glyphicons-fontawesome.min.css", "bootstrap4-glyphicons/maps/glyphicons-fontawesome.css");

            // Bootstrap Italia
            manifest.DefineStyle("BootstrapItalia")
                .SetUrl("../BootstrapItalia/css/bootstrap-italia.min.css");
            manifest.DefineScript("BootstrapItalia")
                .SetUrl("../BootstrapItalia/js/bootstrap-italia.bundle.min.js");

            manifest.DefineScript("I18Next")
                .SetCdn("https://unpkg.com/i18next@19.9.0/dist/umd/i18next.min.js")
                .SetDependencies("jQuery");
            manifest.DefineScript("PasswordStrength")
                .SetCdn("https://cdn.jsdelivr.net/npm/pwstrength-bootstrap@3.0.9/dist/pwstrength-bootstrap.min.js")
                .SetDependencies("jQuery");

            //FullCalendar
            manifest.DefineScript("FullCalendar")
                .SetUrl("fullcalendar/fullcalendar-5.6.0/lib/main.min.js", "fullcalendar/fullcalendar-5.6.0/lib/main.js").SetVersion("5.6.0");
            manifest.DefineScript("FullCalendar.Locales")
                .SetUrl("fullcalendar/fullcalendar-5.6.0/lib/locales-all.min.js", "fullcalendar/fullcalendar-5.6.0/lib/locales-all.js").SetVersion("5.6.0")
                .SetDependencies("FullCalendar");

            manifest.DefineStyle("FullCalendar")
                .SetUrl("fullcalendar/fullcalendar-5.6.0/main.min.css", "fullcalendar/fullcalendar-5.6.0/main.css").SetVersion("5.6.0");

            // Three.js and Photo Sphere Viewer
            manifest.DefineScript("Three.js")
                .SetUrl("Threejs/r153/three.min.js", "Threejs/r153/three.js");
            manifest.DefineScript("PhotoSphereViewer")
                .SetUrl("../photo-sphere-viewer-5.1.6/core/index.min.js", "../photo-sphere-viewer-5.1.6/core/index.js")
                .SetDependencies("Three.js");
            manifest.DefineStyle("PhotoSphereViewer")
                .SetUrl("../photo-sphere-viewer-5.1.6/core/index.min.css", "../photo-sphere-viewer-5.1.6/core/index.css");

            // Animate.css
            manifest.DefineStyle("AnimateCss")
                .SetUrl("Animate/4.1.1/animate.min.css", "Animate/4.1.1/animate.css");

            // LightGallery 1.4.0
            manifest.DefineStyle("LightGallery")
                .SetUrl("lightgallery/1.4.0/lightgallery.min.css", "lightgallery/1.4.0//lightgallery.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/lightgallery/1.4.0/css/lightgallery.min.css", "https://cdnjs.cloudflare.com/ajax/libs/lightgallery/1.4.0/css/lightgallery.css");
            manifest.DefineStyle("LightGalleryTransitions")
                .SetUrl("lightgallery/1.4.0/lg-transitions.min.css", "lightgallery/1.4.0//lg-transitions.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/lightgallery/1.4.0/css/lg-transitions.min.css", "https://cdnjs.cloudflare.com/ajax/libs/lightgallery/1.4.0/css/lg-transitions.css");
            manifest.DefineScript("LightGallery")
                .SetUrl("lightgallery/1.4.0/lightgallery.min.js", "lightgallery/1.4.0//lightgallery.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/lightgallery/1.4.0/js/lightgallery.min.js", "https://cdnjs.cloudflare.com/ajax/libs/lightgallery/1.4.0/js/lightgallery.js")
                .SetDependencies("jQuery");
        }
    }
}