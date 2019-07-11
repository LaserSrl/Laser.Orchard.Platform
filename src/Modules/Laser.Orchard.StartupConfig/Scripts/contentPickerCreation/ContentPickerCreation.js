$(document).ready(function () {
    $(".button").button();
    $(".buttonset").buttonset();

    //$('#selectContentTypeBtn').button({ icons: { primary: "ui-icon-plusthick" } });
    //$('.ContentTypeOptions').toggle();

    $("#layout-content").on("orchard-admin-contentpicker-create", "form", function (ev, data) {
        data = data || {};

        var callbackName = "?callback=_contentpickercreate_" + new Date().getTime();
        var cPFName = "&namecpfield=" + data.namecpfield;
        var url = data.correctUrl + "/Admin/Contents/Create/";

        window.open(url + data.CTName + callbackName + cPFName, "_blank ", "width=1500,height=700");
    });

    $('.divCreateNewButton').each(function () {
        var buttonPrefix = "divCreateNewButton_";

        var buttonId = $(this).attr('id');
        var relatedFieldName = buttonId.substring(buttonId.indexOf(buttonPrefix) + buttonPrefix.length);

        $(this).insertAfter('.content-picker-field[data-field-name=' + relatedFieldName + '] > .button.add');
    });
});

function CallParent(data) {
    alert(" Parent window Alert " + data.idcontent + ", " + data.namecpfiels);


    var oldValue = $('.content-picker-field[data-field-name=' + data.namecpfiels + '] > input').val();
    $('.content-picker-field[data-field-name=' + data.namecpfiels + '] > input').val(oldValue + ',' + data.idcontent);

    var template = '<tr><td>&nbsp;</td><td><span data-id="{contentItemId}" data-fieldid=" idsFieldId" class="content-picker-item" >{edit-link} {status-text}</span ></td > <td><span data-id="{contentItemId}" class="content-picker-remove button grey">' + data.remove_text + '</span></td></tr > ';
    var editLink = '<a href = "' + data.edit_link + '"> ' + data.title_content + '</a>';

    var status = "";
    if (data.published == 'false') {
        status = " - " + data.not_published;
    }

    var tmpl = template.replace('{contentItemId}', data.idcontent)
        .replace('{edit-link}', editLink)
        .replace('{status-text}', status);
    //var content = $(tmpl);

    $('.content-picker-field[data-field-name=' + data.namecpfiels + '] > table.items > tbody').append(tmpl);


    ////refreshIds();
    //$(self).find('.content-picker-message').show();
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