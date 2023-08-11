$('.dropdown-menu a.dropdown-toggle').on('click', function (e) {
    //we prevent bootstrap.toggle() to be fired
    e.preventDefault();
    e.stopPropagation();
    multiLevelMenu_Click(e, $(this), '.dropdown-menu');
});
$('a.nav-link.dropdown-toggle').on('click', function (e) {
    multiLevelMenu_Click(e, $(this).parent().first(), '.dropdown-menu');
});

function multiLevelMenu_Click(event, sender, subsequentSelector) {
    // sender is the clicked link
    //we find all subsequent .dropdown-menu and we remove all the "show" cssclass
    sender.find(subsequentSelector).each(function (index) {
        $(this).parents(subsequentSelector).first().find('.show').removeClass('show');
    });
    //we find all sibling .dropdown-menu and we remove all the "show" cssclass
    sender.next(subsequentSelector).each(function (index) {
        $(this).parents(subsequentSelector).first().find('.show').removeClass('show');
    });


    // for the first subsequent .dropdownmenu we toggle the "show" cssclass
    var $subMenu = sender.next(subsequentSelector);
    $subMenu.toggleClass('show');

    return false;
    
    if (!sender.next().hasClass('show')) {
        sender.parents('.dropdown-menu').first().find('.show').removeClass('show');
    }
    var $subMenu = sender.next('.dropdown-menu');
    $subMenu.toggleClass('show');


    sender.parents('li.nav-item.dropdown.show').on('hidden.bs.dropdown', function (e) {
        $('.dropdown-submenu .show').removeClass('show');
    });
    return false;
}