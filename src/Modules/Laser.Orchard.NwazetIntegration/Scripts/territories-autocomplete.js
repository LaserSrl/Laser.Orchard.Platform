(function ($) {
    /* Helper functions
    **********************************************************************/
    var addTagToForm = function ($wrapper, tag, checked) {
        var modelId = $wrapper
            .data("model-id");
        var territoriesHidden = $("#" + modelId, $wrapper);
        var jsonString = territoriesHidden.val();
        // The json we have in the page may be used for tokenization
        var preventTokens = $wrapper
            .data("prevent-tokenization") == true;
        if (preventTokens) {
            jsonString = jsonString
                .replace(/{{/g, "{")
                .replace(/}}/g, "}");
        }
        var territoriesArray = JSON.parse(jsonString);
        // the array of all tags except the current
        var arr = territoriesArray.filter(function (val) {
            return val.value != tag.value
        });

        if (checked) {
            // add to list
            arr.push(tag);
        } else {
            // remove from list
        }
        var outString = JSON.stringify(arr);
        if (preventTokens) {
            outString = outString
                .replace(/{/g, "{{")
                .replace(/}/g, "}}");
        }
        territoriesHidden.val(outString);
    };


    /* Event handlers
    **********************************************************************/
    var onTagsChanged = function (tagLabelOrValue, action, tag) {

        if (tagLabelOrValue == null)
            return;
        
        var $input = this.appendTo;
        var $wrapper = $input.parents("fieldset:first");
        var $tagIt = $("ul.tagit", $wrapper);

        if (action == "popped") {
            // removing tag
            addTagToForm($wrapper, tag, false);
        }
        
        var tags = $tagIt.tagit("tags");
        $(tags).each(function (index, tag) {
            addTagToForm($wrapper, tag, true);
        });
        
    };

    var renderAutocompleteItem = function (ul, item) {

        var label = item.label;

        for (var i = 0; i < item.depth; i++) {
            label = "<span class=\"gap\">&nbsp;</span>" + label;
        }

        var li = "<li></li>";

        return $(li)
            .data("item.autocomplete", item)
            .append($("<a></a>").html(label))
            .appendTo(ul);
    };

    /* Initialization
    **********************************************************************/
    $(".territories-editor").each(function () {
        var selectedTerritories = $(this).data("selected-territories");

        var autocompleteUrl = $(this).data("autocomplete-url");

        var $tagit = $("> ul", this).tagit({
            tagSource: function (request, response) {
                var territoriesEditor = $(this.element).parents(".territories-editor");
                $.getJSON(autocompleteUrl,
                    {
                        hierarchyId: territoriesEditor.data("hierarchy-id"),
                        query: request.term
                    },
                    function (data, status, xhr) {
                        response(data);
                });
            },
            initialTags: selectedTerritories,
            triggerKeys: ['enter', 'tab'], // default is ['enter', 'space', 'comma', 'tab'] but we remove comma and space to allow them in the terms
            allowNewTags: false,
            tagsChanged: onTagsChanged,
            caseSensitive: false,
            // at least this many characters (not less than 2 for provinces)
            minLength: 2, 
            sortable: true
        }).data("ui-tagit");

        $tagit.input.autocomplete().data("ui-autocomplete")._renderItem = renderAutocompleteItem;
        $tagit.input.autocomplete().on("autocompletefocus", function (event, ui) {
            event.preventDefault();
        });

    });
})(jQuery);