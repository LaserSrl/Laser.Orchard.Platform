var WidgetsContainer;
(function (WidgetsContainer) {
    $(function () {
        var widgetPartLayerId = $("#WidgetPart_LayerId");
        var fieldset = widgetPartLayerId.parents("fieldset:first");
        fieldset.hide();
    });
})(WidgetsContainer || (WidgetsContainer = {}));

function HideTechElements() {
    var technicalElements = [
        { id: 'WidgetPart_Position', parent: "fieldset" },
        { id: 'WidgetPart_Name', parent: "fieldset" },
        { id: 'WidgetPart_CssClasses', parent: "fieldset" }
    ]

    for (var index in technicalElements) {
        $('#' + technicalElements[index].id).parents(technicalElements[index].parent + ":first").hide();
    }
}

