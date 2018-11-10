using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;


namespace Laser.Orchard.SEO.Handlers {


  [OrchardFeature("Laser.Orchard.Favicon")]
  public class FaviconSettingsPartHandler : ContentHandler {


    public FaviconSettingsPartHandler(IRepository<FaviconSettingsRecord> repository) {
      T = NullLocalizer.Instance;
      Filters.Add(StorageFilter.For(repository));
      Filters.Add(new ActivatingFilter<FaviconSettingsPart>("Site"));
    }


    public Localizer T { get; set; }


    protected override void GetItemMetadata(GetContentItemMetadataContext context) {
      if (context.ContentItem.ContentType != "Site")
        return;
      base.GetItemMetadata(context);
      context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Favicon")));
    }


  }
}