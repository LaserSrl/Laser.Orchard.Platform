/*

	1. Flickr Feed
	2. Accordion
	3. Responsive Main Menu
	4. Responsive Blog Pagination
	5. Filterable Portfolio
	6. prettyPhoto Lightbox
	7. Image Hover
	8. Social Links Tooltip
	9. Scroll to Top
	10. Flexslider
	
*/

$(document).ready(function(){

	//---------------------------------------
	// 1. Flickr Feed
    //---------------------------------------
    $("a[rel^='prettyPhoto']").prettyPhoto({ overlay_gallery: false, social_tools:false });

    $("#footer .flickr li").hover(function () {
        $(this).find("img").stop(true, true).animate({ opacity: 0.5 }, 800);
    }, function () {
        $(this).find("img").stop(true, true).animate({ opacity: 1.0 }, 800);
    });


	//$('#footer .flickr').jflickrfeed({
	//	limit: 6,
	//	qstrings: {
	//		id: '52617155@N08'
	//	},
	//	itemTemplate: '<li>'+
	//					'<a rel="prettyPhoto[flickr]" href="{{image}}" title="{{title}}">' +
	//						'<img src="{{image_s}}" alt="{{title}}" />' +
	//					'</a>' +
	//				  '</li>'
	//}, function(data) {
	//	$("a[rel^='prettyPhoto']").prettyPhoto({overlay_gallery: false});

	//	$("#footer .flickr li").hover(function () {						 
	//		$(this).find("img").stop(true, true).animate({ opacity: 0.5 }, 800);
	//	}, function() {
	//		$(this).find("img").stop(true, true).animate({ opacity: 1.0 }, 800);
	//	});
	//});	

	//---------------------------------------
	// 2. Accordion
	//---------------------------------------
	$('#accordions').tabs(    
		'#accordions div.pane', {
		tabs: 'h2',
		effect: 'slide',
		initialIndex: null
	});

	//---------------------------------------
	// 3. Responsive Main Menu
	//---------------------------------------
	var $menu_select = $("<select />");	
	$("<option />", {"selected": "selected", "value": "", "text": "Site Navigation"}).appendTo($menu_select);
	$menu_select.appendTo("#main-menu");
	$("#main-menu ul li a").each(function(){
		var menu_url = $(this).attr("href");
		var menu_text = $(this).text();
		if ($(this).parents("li").length == 2) { menu_text = '- ' + menu_text; }
		if ($(this).parents("li").length == 3) { menu_text = "-- " + menu_text; }
		if ($(this).parents("li").length > 3) { menu_text = "--- " + menu_text; }
		$("<option />", {"value": menu_url, "text": menu_text}).appendTo($menu_select)
	})

	field_id = "#main-menu select";
	$(field_id).change(function()
	{
	   value = $(this).attr('value');
	   window.location = value;	
	});	

	//---------------------------------------
	// 4. Responsive Blog Pagination	
	//---------------------------------------
	var $menu_select2 = $("<select />");	
	$("<option />", {"selected": "selected", "value": "", "text": "Page Navigation"}).appendTo($menu_select2);
	$menu_select2.appendTo(".articles .pagination");
	$(".articles .pagination ul li a").each(function(){
		var menu_url2 = $(this).attr("href");
		var menu_text2 = $(this).text();
		if ($(this).parents("li").length == 2) { menu_text = '- ' + menu_text; }
		if ($(this).parents("li").length == 3) { menu_text = "-- " + menu_text; }
		if ($(this).parents("li").length > 3) { menu_text = "--- " + menu_text; }
		$("<option />", {"value": menu_url2, "text": menu_text2}).appendTo($menu_select2)
	})

	field_id2 = ".articles .pagination select";
	$(field_id).change(function()
	{
	   value2 = $(this).attr('value');
	   window.location = value2;	
	});	

	//---------------------------------------
	// 5. Filterable Portfolio
	//---------------------------------------
	var $portfolioClone = $(".portfolio").clone();
	$(".menu-filterable a").live('click', function(e){
		
		$(".menu-filterable li").removeClass("active");  
		
		var $filterClass = $(this).parent().attr("class");

		if ( $filterClass == "all" ) {
			var $filteredPortfolio = $portfolioClone.find("article");
		} else {
			var $filteredPortfolio = $portfolioClone.find("article[data-type~=" + $filterClass + "]");
		}
		
		$(".portfolio").quicksand( $filteredPortfolio, { 
			duration: 800, 
			easing: 'easeInOutQuint' 
		}, function(){          
			
			$('.portfolio article').hover(function () {
					$(this).find('.image-overlay-bg').stop(true, true).animate({opacity: 1}, 200 ).css({'display': 'block'});
				}, function () {
					$(this).find('.image-overlay-bg').stop(true, true).animate({opacity: 0}, 200 );
				}
			);
			
			$("a[data-rel^='prettyPhoto']").prettyPhoto({
				overlay_gallery: false,
				social_tools: '<div class="pp_social"></div>'
			});

		});					
		
		$(this).parent().addClass("active");
		e.preventDefault();
	});

	//---------------------------------------
	// 6. prettyPhoto Lightbox
	//---------------------------------------
	$("a[data-rel^='prettyPhoto']").prettyPhoto({	
		overlay_gallery: false,
		social_tools: '<div class="pp_social"></div>'
	});

	//---------------------------------------
	// 7. Image Hover
	//---------------------------------------
	$('.portfolio article, .slides li, .portfolio-item, .news-block').hover(function () {
			$(this).find('.image-overlay-bg').stop(true, true).animate({opacity: 1}, 200 ).css({'display': 'block'});
		}, function () {
			$(this).find('.image-overlay-bg').stop(true, true).animate({opacity: 0}, 200 );
		}
	);

	//---------------------------------------
	// 8. Social Links Tooltip
	//---------------------------------------
	$('.social-links a').tooltip();

	//---------------------------------------
	// 9. Scroll to Top
	//---------------------------------------
	$('#scrolltop a').click(function(){
		$('html, body').animate({scrollTop:0}, { duration: 800, easing: 'easeInOutExpo'}); 
		return false; 
	}); 

});

//---------------------------------------
// 10. Flexslider
//---------------------------------------
$(window).load(function(){
	  $('.flexslider').flexslider({
		animation: "slide",
		start: function(slider){
		  $('body').removeClass('loading');
		}
	  });
	  
	  $('.flexslider2').flexslider({
		animation: "fade",
		easing: "easeInOutQuad",
		animationSpeed: 500,
		 controlNav: false, 
		start: function(slider){
		  $('body').removeClass('loading');
		}
	  });
	  
	  $('.portfolio-detail').flexslider({
		animation: "slide",
		 controlNav: false, 
		start: function(slider){
		  $('body').removeClass('loading');
		}
	  });
	  
 });   