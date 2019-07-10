$(document).ready(function () {
    $(".button").button();
    $(".buttonset").buttonset();

    //$('#selectContentTypeBtn').button({ icons: { primary: "ui-icon-plusthick" } });
    $('.ContentTypeOptions').toggle();



    $("#layout-content").on("orchard-admin-contentpicker-create", "form", function (ev, data) {
        data = data || {};

        var callbackName = "?callback=_contentpickercreate_" + new Date().getTime();
        var cPFName = "&namecpfield=" + data.namecpfield;
        var url = data.correctUrl + "/Admin/Contents/Create/";

        window.open(url + data.CTName + callbackName + cPFName, "_blank ", "width=1500,height=700");
    });

});

function CallParent(data) {
    alert(" Parent window Alert " + str);
    var template = '<tr><td>&nbsp;</td><td><span data-id="{contentItemId}" data-fieldid=" idsFieldId" class="content-picker-item" > { edit-link }{ status - text }</span ></td > <td><span data-id="{contentItemId}" class="content-picker-remove button grey">' + removeText + '</span></td></tr > ';

        var tmpl = template.replace(/\{contentItemId\}/g, data.id)
        .replace(/\{edit-link\}/g, data.editLink)
        .replace(/\{status-text}/g, data.published ? "" : " - " + notPublishedText);
    var content = $(tmpl);
    $(self).find('table.content-picker tbody').append(content);

    //refreshIds();
    $(self).find('.content-picker-message').show();
}

var divCreateNewButton = {
    isHoverMenu: false,

    onSelectCTClick: function () {
        var name = this.dataset.namecpfield;
        $('#ulNewCT_' + name).toggle();

        var btnLeft = $('#divCTNewButton_' + name).offset().left;
        var btnTop = $('#divCTNewButton_' + name).offset().top +
            $('#divCTNewButton_' + name).outerHeight();
        var btnWidth = $('#divCTNewButton_' + name).outerWidth();
        $('#ulNewCT_' + name).css('left', btnLeft).css('top', btnTop);
    },

    CTOptionChoice: function () {

        var idSelector = '#' + this.id;
        var name = this.dataset.namecpfield;
        $('#ulNewCT_' + name).hide();

        $(idSelector).trigger("orchard-admin-contentpicker-create", {
            CTName: this.textContent,
            correctUrl: this.dataset.correcturl,
            namecpfield: this.dataset.namecpfield
        });
    }
};