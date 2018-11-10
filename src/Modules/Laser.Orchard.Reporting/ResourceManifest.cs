using Orchard.UI.Resources;

namespace Laser.Orchard.Reporting {
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            builder.Add().DefineScript("jqplot").SetUrl("jquery.jqplot.js").SetDependencies("jQueryUI");
            builder.Add().DefineScript("piejqplot").SetUrl("plugins/jqplot.pieRenderer.js").SetDependencies("jqplot");
            builder.Add().DefineScript("barjqplot").SetUrl("plugins/jqplot.barRenderer.js").SetDependencies("jqplot");
            builder.Add().DefineScript("categoryAxisjqplot").SetUrl("plugins/jqplot.categoryAxisRenderer.js").SetDependencies("jqplot");
            builder.Add().DefineScript("pointLabelsjqplot").SetUrl("plugins/jqplot.pointLabels.js").SetDependencies("jqplot");
            builder.Add().DefineScript("donutRendererjqplot").SetUrl("plugins/jqplot.donutRenderer.js").SetDependencies("jqplot");
            builder.Add().DefineScript("canvasAxisTickjqplot").SetUrl("plugins/jqplot.canvasAxisTickRenderer.js").SetDependencies("jqplot");
            builder.Add().DefineScript("canvasTextjqplot").SetUrl("plugins/jqplot.canvasTextRenderer.js").SetDependencies("jqplot");
            builder.Add().DefineScript("bootstrap-table").SetUrl("//cdnjs.cloudflare.com/ajax/libs/bootstrap-table/1.11.1/bootstrap-table.min.js").SetDependencies("bootstrap");
            builder.Add().DefineStyle("bootstrap-table").SetUrl("//cdnjs.cloudflare.com/ajax/libs/bootstrap-table/1.11.1/bootstrap-table.min.css").SetDependencies("bootstrap");
        }
    }
}