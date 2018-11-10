$(function () {
	var $foreground = $('.foreground'),
		$choice = $('.choice');

	//	gather the thumbnails
	var $thumb = $('.thumb');
	$foreground.children().each(
		function () {
			var src = $(this).attr('src');
			$thumb.append('<img src="' + src.split('/Large/').join('/Small/') + '" />');
	});

	//	duplicate the thumbnails
	for (var a = 0; a < ($foreground.children().length - 1); a++) {
		if ((a + 1) < ($foreground.children().length - 1)) {
			$choice.append('<div class="height-4"></div>');
		}
		$choice.append($thumb.clone());
	}

	//	create large carousel
	$foreground.carouFredSel({
		items: {
			visible: 1,
			width: 417,
			height: 292
		},
		scroll: {
			fx: 'directscroll',
			onBefore: function (data) {
				var oldSrc = data.items.old.attr('src').split('/Large/').join('/Small/'),
					newSrc = data.items.visible.attr('src').split('/Large/').join('/Small/'),
					$t = $thumbs.find('img:first-child[src="' + newSrc + '"]').parent();

				$t.trigger('slideTo', [$('img[src="' + oldSrc + '"]', $t), 'next']);
			}
		}
	});

	// create thumb carousels
	var $thumbs = $('.thumb');
	$thumbs.each(
		function (i) {
			$(this).carouFredSel({
				auto: false,
				scroll: {
					fx: 'directscroll'
				},
				items: {
					start: i + 1,
					visible: 1,
					width: 100,
					height: 70
				}
			});

			//	click the carousel
			$(this).click(
				function () {
					var src = $(this).children().first().attr('src').split('/Small/').join('/Large/');
					$foreground.trigger('slideTo', [$('img[src="' + src + '"]', $foreground), 'next']);
			});
		});
});