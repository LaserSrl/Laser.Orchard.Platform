using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.UI.Admin;
using Orchard.Widgets.Models;


namespace Laser.Orchard.ZoneAlternates {
    public class ZoneAlternatesFactory : ShapeDisplayEvents {

        private string lastZone = "";
        public override void Displaying(ShapeDisplayingContext context) {

            context.ShapeMetadata
            .OnDisplaying(displayedContext => {
                if (displayedContext.ShapeMetadata.Type == "Zone") {
                    lastZone = displayedContext.Shape.ZoneName;
                }

                // We don't want the widget itself, 
                // but the content item that consists of the Widget part (e.g. Parts.Blogs.RecentBlogPosts)
                if (displayedContext.ShapeMetadata.Type != "Widget") {
                    if (displayedContext.Shape.ContentItem is ContentItem) {

                        ContentItem contentItem = displayedContext.Shape.ContentItem;
                        ContentPart contentPart = displayedContext.Shape.ContentPart is ContentPart ? displayedContext.Shape.ContentPart : null;
                        ContentField contentField = displayedContext.Shape.ContentField is ContentField ? displayedContext.Shape.ContentField : null;
                        var displayType = displayedContext.ShapeMetadata.DisplayType;
                        var shapeName = displayedContext.ShapeMetadata.Type;

                        //var route = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values;
                        //var area = (route.ContainsKey("area")) ? route["area"] : null;
                        //var controller = (route.ContainsKey("controller")) ? route["controller"] : null;
                        //var action = (route.ContainsKey("action"))? route["action"]: null;
                        bool isAdmin = false;
                        if (System.Web.HttpContext.Current != null) {
                            isAdmin = AdminFilter.IsApplied(System.Web.HttpContext.Current.Request.RequestContext);
                        }

                        if (contentItem != null && lastZone != "") {

                            // contentItem è un Widget?
                            var zoneName = lastZone;
                            // [ShapeName]-[ZoneName].cshtml: "Parts.Blogs.RecentBlogPosts-myZoneName.cshtml"
                            // [ContentTypeName]-[ZoneName].cshtml: "RecentBlogPosts-myZoneName.cshtml"
                            if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + zoneName)) {
                                displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + zoneName);
                                if (!string.IsNullOrWhiteSpace(displayType) && displayType != "Detail") {
                                    displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + zoneName + "__" + displayType);
                                }
                            }
                            if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentItem.ContentType + "__" + zoneName)) {
                                displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__" + zoneName);
                                if (!string.IsNullOrWhiteSpace(displayType) && displayType != "Detail") {
                                    displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__" + zoneName + "__" + displayType);
                                }
                            }
                            if (contentField != null) {
                                if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentField.Name + "__" + zoneName)) {
                                    displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentField.Name + "__" + zoneName);
                                    if (!string.IsNullOrWhiteSpace(displayType) && displayType != "Detail") {
                                        displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentField.Name + "__" + zoneName + "__" + displayType);
                                    }
                                }
                                if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentItem.ContentType + "__" + contentField.Name + "__" + zoneName)) {
                                    displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__" + contentField.Name + "__" + zoneName);
                                    if (!string.IsNullOrWhiteSpace(displayType) && displayType != "Detail") {
                                        displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__" + contentField.Name + "__" + zoneName + "__" + displayType);
                                    }
                                }
                            }
                        }
                        if (contentItem != null) {
                            if (isAdmin) {
                                if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentItem.ContentType + "__AdminFilter")) {
                                    displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__AdminFilter");
                                    if (!string.IsNullOrWhiteSpace(displayType) && displayType != "Detail") {
                                        displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__AdminFilter" + "__" + displayType);
                                    }
                                }
                            }
                        }
                    }

                }
                else {
                    //It's a widget
                    var contentType = displayedContext.Shape.ContentItem.ContentType;
                    var zoneName = displayedContext.Shape.ContentItem.WidgetPart.Zone;
                    string shapeName = "Widget__" + contentType + "__" + zoneName;
                    // Widget-[ContentTypeName]-[ZoneName].cshtml: "Widget-RecentBlogPosts-myZoneName.cshtml"
                    if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName)) {
                        displayedContext.ShapeMetadata.Alternates.Add(shapeName);
                    }
                }
            });
        }
    }
}