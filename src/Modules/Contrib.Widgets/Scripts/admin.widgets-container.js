var WidgetsContainer;
(function (WidgetsContainer) {
    $(function () {
        var removedWidgets = [];
        $(".add-widget").on("click", function (e) {
            e.preventDefault();
            var hostId = $(this).data("host-id");
            var form = $(this).parents("form:first");
            var fieldset = $(this).parents("fieldset:first");

            var hiddenSubmitSave = $("#hiddenSubmitSave")[0];   
            var attHSS = document.createAttribute("name");       
            attHSS.value = "submit.Save";                           
            hiddenSubmitSave.setAttributeNode(attHSS);   

            var formActionValue = fieldset.find("input[name='submit.Save']");
            var url = $(this).attr("href");
            if(hostId === 0) {
                form.attr("action", url);
            } else {
                formActionValue.val("submit.Save");
                $("input[type='hidden'][name='returnUrl']").val(url);
            }

            form.submit();
        });
        $("div.widgets").on("click", "a.remove-widget", function (e) {
            e.preventDefault();
            if(!confirm($(this).data("confirm"))) {
                return;
            }
            var li = $(this).parents("li:first");
            var widgetId = li.data("widget-id");
            li.remove();
            removedWidgets.push(widgetId);
            $("input[name='removedWidgets']").val(JSON.stringify(removedWidgets));
            updateWidgetPlacementField();
        });
        var updateWidgetPlacementField = function () {
            var widgetPlacementField = $("input[name='widgetPlacement']");
            var data = {
                zones: {
                }
            };
            $("div.widgets ul.widgets").each(function () {
                var zone = $(this).data("zone");
                data.zones[zone] = {
                    widgets: []
                };
                $(this).find("li").each(function () {
                    var widgetId = $(this).data("widget-id");
                    data.zones[zone].widgets.push(widgetId);
                });
            });
            var text = JSON.stringify(data);
            widgetPlacementField.val(text);
        };
        $("div.widgets ul.widgets").sortable({
            connectWith: "div.widgets ul.widgets",
            dropOnEmpty: true,
            placeholder: "sortable-placeholder",
            receive: function (e, ui) {
                updateWidgetPlacementField();
            },
            update: function (e, ui) {
                updateWidgetPlacementField();
            }
        });
        $("#widgetsPlacement legend").expandoControl(function (controller) {
            return controller.nextAll(".expando");
        }, {
            collapse: false,
            remember: true,
            context: "widget-container" 
        });
    });
})(WidgetsContainer || (WidgetsContainer = {}));

