$(document).ready(function () {
    $(".button").button();
    $(".buttonset").buttonset();

    $("#layout-content").on("orchard-admin-contentpicker-create", "form", function (ev, data) {
        data = data || {};

        var callbackName = "?callback=_contentpickercreate_" + new Date().getTime();
        var cPFName = "&namecpfield=" + data.namecpfield;
        var ctName = "&selectedct=" + data.ctName;
        var url = data.createUrl;

        window.open(url + callbackName + cPFName + ctName, "_blank", "width=1500,height=700");
    });

    $('.divCreateNewButton').each(function () {
        var buttonPrefix = "divCreateNewButton_";

        var buttonId = $(this).attr('id');
        var relatedFieldName = buttonId.substring(buttonId.indexOf(buttonPrefix) + buttonPrefix.length);

        $(this).insertAfter('.content-picker-field[data-field-name=' + relatedFieldName + '] > .button.add');
    });

    mutationObserverCPF();
});

var callbackMutation = function () {
    refreshIdsCreateBtn();
};

var mutationObserverCPF = function () {
    $("table.content-picker").each(function () {
        var config = { attributes: true, childList: true, subtree: true };
        var observer = new MutationObserver(callbackMutation);
        observer.observe(this, config);
    });
}; 

var refreshIdsCreateBtn = function () {
    $('ul.ContentTypeOptions').hide();

    $(".selectContentTypeBtn").each(function () {
        var name = this.dataset.namecpfield;
        var datafieldidSelector = 'fieldset[data-field-name="' + name + '"] > table.content-picker tr';

        if (this.dataset.multiple == "False" && ($(datafieldidSelector).length -1) > 0) {
            $(this).hide();
            $('.content-picker-field[data-field-name=' + name + '] > .button.add').hide();
        }
        else {
            $(this).show();
            $('.content-picker-field[data-field-name=' + name + '] > .button.add').show();
        }
    });
};

function CallParent(data) {
    var oldValue = $('.content-picker-field[data-field-name=' + data.namecpfield + '] > input').val();
    $('.content-picker-field[data-field-name=' + data.namecpfield + '] > input').val(oldValue + ',' + data.idcontent);

    var template = '<tr><td>&nbsp;</td><td><span data-id="{contentItemId}" data-fieldid="@idsFieldId" class="content-picker-item" >{edit-link} {status-text}</span ></td > <td><span data-id="{contentItemId}" class="content-picker-remove button grey">' + data.remove_text + '</span></td></tr > ';
    var editLink = '<a href = "' + data.edit_link + '"> ' + data.title_content + '</a>';

    var status = "";
    if (data.published == 'false') {
        status = " - " + data.not_published;
    }

    var tmpl = template.replace('{contentItemId}', data.idcontent)
        .replace('{edit-link}', editLink)
        .replace('{status-text}', status);

    $('.content-picker-field[data-field-name=' + data.namecpfield + '] > table.items > tbody').append(tmpl);

    $('.content-picker-field[data-field-name=' + data.namecpfield + '] > .content-picker-message').show();
}

var divCreateNewButton = {
    isHoverMenu: false,
    onSelectCTClick: function () {
        var name = this.dataset.namecpfield;

        $('ul.ContentTypeOptions').not($('#ulNewCT_' + name)).hide();

        if ($('#ulNewCT_' + name + ' li').length > 1) {
            $('#ulNewCT_' + name).toggle();

            var btnLeft = $('#divCTNewButton_' + name).offset().left;
            var btnTop = $('#divCTNewButton_' + name).offset().top +
                $('#divCTNewButton_' + name).outerHeight();

            $('#ulNewCT_' + name).css('left', btnLeft).css('top', btnTop);
        } else {
            var onlyElement = $('#ulNewCT_' + name + ' li').first();

            onlyElement.trigger("orchard-admin-contentpicker-create", {
                createUrl: onlyElement.data('createurl'),
                namecpfield: onlyElement.data('namecpfield'),
                ctName: onlyElement.data('ctname')
            });
        }
    },
    CTOptionChoice: function () {
        var idSelector = '#' + this.id;
        var name = this.dataset.namecpfield;
        $('#ulNewCT_' + name).hide();

        $(idSelector).trigger("orchard-admin-contentpicker-create", {
            createUrl: this.dataset.createurl,
            namecpfield: this.dataset.namecpfield,
            ctName: this.dataset.ctname
        });
    }
};
