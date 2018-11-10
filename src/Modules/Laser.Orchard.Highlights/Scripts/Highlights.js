function openEditWindow(HighlightsId, data) {

    var url = data.baseUrl + "Admin/Contents/"
    var editurl = "<a href=\"" + data.baseUrl + "Admin/Contents/Edit/{id}\">Edit</a>";
    var deleteurl = "<a href=\"" + data.baseUrl + "Admin/Contents/Remove/{id}\" itemprop=\"RemoveUrl UnsafeUrl\">Delete</a>";
    var ritorno = data.returnUrl.substring(1);

    if (HighlightsId > 0) {
        url += "Edit/" + HighlightsId.toString() + "&returnUrl=/OrchardLocal/Admin/Widgets/EditWidget/" + data.partId.toString() + "?returnUrl=/OrchardLocal/Admin/Widgets";
    } else {
        //url += "Create/HighlightsItem?groupId=" + data.partId.toString();
        //url += "Create/HighlightsItem?groupId=" + data.partId.toString() + "?returnUrl=/OrchardLocal/Admin/Widgets";
        //url += "Create/HighlightsItem?groupId=" + data.partId.toString() + "&returnUrl=/Orchard.Web/Admin/Widgets/EditWidget/" + data.partId.toString() + "?returnUrl=/OrchardLocal/Admin/Widgets";
        url += "Create/HighlightsItem?HighlightsGroupId=" + data.partId.toString() + "&" + ritorno;
    }

    // h t t p : / / l ocalhost/Orchard.Web/Admin/Contents/Edit/358?returnUrl=/Orchard.Web/Admin/Widgets/EditWidget/48?returnUrl=%2FOrchard.Web%2FAdmin%2FWidgets

    var newWindow = window.open(url, "_self", "width=980,height=700,status=no,toolbar=no,location=no,menubar=no,resizable=no,scrollbars=yes");

    return false;
}