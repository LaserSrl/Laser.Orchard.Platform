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
    // we find all subsequent .dropdown-menu and we remove all the "show" cssclass
    // so all the subsequent doprdown menus will be hidden
    sender.find(subsequentSelector).each(function (index) {
        $(this).parents(subsequentSelector).first().find('.show').removeClass('show');
    });
    // we find all sibling .dropdown-menu and we remove all the "show" cssclass
    // so all the sibling doprdown menus will be hidden
    sender.next(subsequentSelector).each(function (index) {
        $(this).parents(subsequentSelector).first().find('.show').removeClass('show');
    });


    // for the first subsequent .dropdownmenu we toggle the "show" cssclass
    var $subMenu = sender.next(subsequentSelector);
    $subMenu.toggleClass('show');

    return false;
}