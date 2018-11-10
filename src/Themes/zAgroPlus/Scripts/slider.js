$(document).ready(function(){
		$('.mp-slider')._TMS({
			show:0,
			pauseOnHover:true,
			prevBu:'.mp-prev',
			nextBu:'.mp-next',
			duration:1000,
			preset: 'random',
			pagination: false,//'.pagination',true,'<ul></ul>'
			pagNums:false,
			slideshow:7000,
			numStatus:false,
			banners:'fade',// fromLeft, fromRight, fromTop, fromBottom
			waitBannerAnimation:false
		})		
 })