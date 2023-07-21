using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.FieldStorage;
using Orchard;
using Orchard.Fields.Fields;
using Orchard.Projections.Services;
using Orchard.Projections.Models;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class FieldCreatortHandler : ContentHandler {//IFieldStorageEvents {
        private readonly IOrchardServices _orchardServices;
        private readonly IDraftFieldIndexService _draftFieldIndexService;
        public FieldCreatortHandler(
            IOrchardServices orchardServices,
            IDraftFieldIndexService draftFieldIndexService) {

            _orchardServices = orchardServices;
            _draftFieldIndexService = draftFieldIndexService;

            OnUpdated<CommonPart>((context, CommonPart) => {
                if (CommonPart != null && _orchardServices.WorkContext!=null) {
                    var currentUser = _orchardServices.WorkContext.CurrentUser;
                    if (currentUser != null) {
                        var currentUserId = Convert.ToDecimal((decimal)currentUser.Id);
                        var dynCommonPart = (dynamic)CommonPart;

                        // Set the values for the infoset
                        dynCommonPart.LastModifier.Value = currentUserId;
                        if (dynCommonPart.Creator.Value == null) {
                            dynCommonPart.Creator.Value = currentUserId;
                        }
                        // Set the values in the records for the projections
                        var fieldIndexPart = CommonPart.As<FieldIndexPart>();
                        if (fieldIndexPart != null) {
                            _draftFieldIndexService.Set(fieldIndexPart,
                                "CommonPart", "LastModifier",
                                null, currentUserId, typeof(decimal));
                            if (_draftFieldIndexService.Get<decimal>(fieldIndexPart,
                                    "CommonPart", "Creator", null) == default(decimal)) {
                                _draftFieldIndexService.Set(fieldIndexPart,
                                    "CommonPart", "Creator",
                                    null, currentUserId, typeof(decimal));
                            }
                        }
                    }
                }
            });

            OnCreated<CommonPart>((context, CommonPart) => {
                if (context.ContentItem.As<CommonPart>() != null && _orchardServices.WorkContext != null) {
                    var currentUser = _orchardServices.WorkContext.CurrentUser;
                    if (currentUser != null) {
                        var currentUserId = Convert.ToDecimal((decimal)currentUser.Id);
                        var dynCommonPart = (dynamic)context.ContentItem.As<CommonPart>();
                        // Set the values for the infoset
                        if (dynCommonPart.Creator.Value == null) {
                            dynCommonPart.Creator.Value = currentUserId;
                        }
                        // Don't set the values in the records because, at creation, there is no record, yet.
                    }
                }
            });
        }
    }
}