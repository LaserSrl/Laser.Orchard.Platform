(function ($) {
    $.fn.extend({
        helpfullySetTheStars: function () {
            return $(this).each(function () {
                var _this = $(this);
                var forms = _this.find("form");
                if (forms.length != 1) {
                    return _this;
                }

                var clearVote = $("#stars-clear-btn");
                $("#stars-clear-btn").hide();
                $("#stars-clear-btn").on(
                        "click",
                        function (e) {
                            var _clear_this = $(this);

                            OpenDeleteRatingConfirmationDialog();

                            $("#confirmDeleteRatingButton").unbind(".rating").bind("click.rating", function () {
                                _clear_this.addClass("active");
                                var form = _clear_this.closest(".stars-rating").find("form").first();
                                form.find('[name="rating"]')
                                    .children("option").attr("selected", false).end()
                                    .children('option[value="-1"]').attr("selected", true);
                                $.post(
                                    form.attr("action"),
                                    form.serialize()
                                );
                                form = null;
                                var resultDisplay = _clear_this.closest(".stars-current-result").first();
                                var existingUserRating = resultDisplay.attr("class").match(/\bstars-user-rating-\d+\b/);
                                if (existingUserRating && existingUserRating.length > 0) {
                                    resultDisplay.removeClass(existingUserRating[0]);
                                }
                                removeClearVoteUI(_clear_this);

                                $("#reviewsList li.mine").fadeOut();
                                CloseDeleteRatingConfirmationDialog();
                            });

                            e.preventDefault();
                            return false;
                        })
                    .on(
                        "mouseenter",
                        function () { $(this).addClass("mousey"); }
                        )
                    .on(
                        "mouseleave",
                        function () { $(this).removeClass("mousey"); }
                    );

                function addClearVoteUI(fromHere) {
                    //fromHere.find(".stars-current-result").first().append(clearVote);
                    $("#stars-clear-btn").show();
                }

                function removeClearVoteUI(fromHere) {
                    $("#stars-clear-btn").removeClass("mousey").removeClass("active");
                    $("#stars-clear-btn").hide();
                    //fromHere.closest(".stars-current-result").first().children(".stars-clear").removeClass("mousey").removeClass("active").remove();
                }

                _this
                    .click(function () {
                        var _thisStar = $(this);
                        var ratingMatch = _thisStar.attr("class").match(/\bstar-(\d+)\b/);
                        if (!ratingMatch || ratingMatch.length < 2) {
                            return;
                        }

                        var rating = _thisStar.attr("class").match(/\bstar-(\d+)\b/)[1];

                        var form = $(forms.first());
                        form.find('[name="rating"]')
                            .children("option").attr("selected", false).end()
                            .children('option[value="' + rating + '"]').attr("selected", true);
                        $.post(
                            form.attr("action"),
                            form.serialize()
                        );
                        form = null;

                        // not bothering to update the display for a failed vote. first use case to implement might be for a user who's auth session has expired
                        var resultDisplay = _this.find(".stars-current-result").first();
                        var existingUserRating = resultDisplay.attr("class").match(/\bstars-user-rating-\d+\b/);
                        if (existingUserRating && existingUserRating.length > 0) {
                            resultDisplay.removeClass(existingUserRating[0]);
                        }

                        resultDisplay.addClass("stars-user-rating-" + rating)
                        resultDisplay = null;

                        addClearVoteUI(_thisStar);
                    })
                    .find(".a-star")
                        .hover(
                            function () { // mouseenter
                                var _thisStar = $(this);
                                _this.addClass(_thisStar.attr("class").match(/\bstar-\d+\b/)[0]);
                            },
                            function () { // mouseleave
                                var _thisStar = $(this);
                                _this.removeClass(_thisStar.attr("class").match(/\bstar-\d+\b/)[0]);
                            });

                // add the "clear vote" bit
                if (_this.find(".stars-current-result").first().attr("class").match(/\bstars-user-rating-\d+\b/)) {
                    addClearVoteUI(_this);
                }

                return _this;
            });
        }
    });
    $(function () {
        $(".stars-rating").helpfullySetTheStars();
    });
})(jQuery);