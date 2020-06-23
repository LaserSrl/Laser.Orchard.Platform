(function ($) {
    /* Helper functions
    **********************************************************************/
    var createTerritoryCheckbox = function ($wrapper, tag, checked) {
        var $ul = $("ul.territories", $wrapper);
        var namePrefix = $wrapper
            .data("name-prefix");
        if (namePrefix != "") {
            namePrefix = namePrefix + ".";
        }
        var idPrefix = $wrapper
            .data("id-prefix");
        if (idPrefix != "") {
            idPrefix = idPrefix + "_";
        }
        var nextIndex = $("li", $ul).length;
        var id = tag.value; //tag.value is a string for territories
        var checkboxId = idPrefix + "Territories_" + nextIndex + "__IsChecked";
        var checkboxName = namePrefix + "Territories[" + nextIndex + "].IsChecked";
        var checkboxHtml = "<input type=\"checkbox\" value=\""
            + (checked ? "true\" checked=\"checked\"" : "false")
            + " data-territory=\"" + tag.label
            + "\" data-territory-identity=\"" + tag.label.toLowerCase()
            + "\" id=\"" + checkboxId
            + "\" name=\"" + checkboxName + "\" />";
        var inputHtml = checkboxHtml;
        var $li = $("<li>" +
            inputHtml +
            "<input type=\"hidden\" value=\"" + id
                + "\" id=\"" + idPrefix + "Territories_" + nextIndex + "__Id"
                + "\" name=\"" + namePrefix + "Territories[" + nextIndex + "].Id" + "\" />" +
            "<input type=\"hidden\" value=\"" + tag.label
                + "\" id=\"" + idPrefix + "Territories_" + nextIndex + "__Name"
                + "\" name=\"" + namePrefix + "Territories[" + nextIndex + "].Name" + "\" />" +
            "<label class=\"forcheckbox\" for=\"" + checkboxId + "\">" + tag.label + "</label>" +
            "</li>").hide();

        $ul.append($li);
        $li.fadeIn();
    };

    /* Event handlers
    **********************************************************************/
    var onTagsChanged = function (tagLabelOrValue, action, tag) {

        if (tagLabelOrValue == null)
            return;

        var $input = this.appendTo;
        var $wrapper = $input.parents("fieldset:first");
        var $tagIt = $("ul.tagit", $wrapper);
        var territories = $("ul.territories", $wrapper);
        var initialTags = $(".territories-editor", $wrapper)
            .data("selected-territories");
        
        territories.empty();

        var tags = $tagIt.tagit("tags");
        $(tags).each(function (index, tag) {
            createTerritoryCheckbox($wrapper, tag, true);
        });

        // Add any tags that are no longer selected but were initially on page load.
        // These are required to be posted back so they can be removed.
        var removedTags = $.grep(initialTags,
            function (initialTag) {
                return $.grep(tags,
                    function (tag) {
                        return tag.value === initialTag.value
                    }).length === 0
            });
        $(removedTags).each(function (index, tag) {
            createTerrritoryCheckbox($wrapper, tag, false);
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