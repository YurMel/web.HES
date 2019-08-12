$(document).ready(function () {
    $('.loading').removeClass('d-flex');
    $('.loading').addClass('d-none');

    var toggled = localStorage.getItem('ToggledSidebar');
    if (toggled === 'true') {
        ToggleSidebarClass();
    }

    $('#collapseAudit').on('show.bs.collapse', function () {
        $('#collapseSettings').collapse('hide');
    });

    $('#collapseSettings').on('show.bs.collapse', function () {
        $('#collapseAudit').collapse('hide');
    });
});

function ToggleSidebar() {
    ToggleSidebarClass();

    var toggled = localStorage.getItem('ToggledSidebar');
    if (toggled === 'true') {
        localStorage.setItem('ToggledSidebar', 'false');
    }
    else {
        localStorage.setItem('ToggledSidebar', 'true');
    }
}

function ToggleSidebarClass() {
    $('.custom-sidebar').toggleClass('toggled');
    $('.page_labels').toggleClass('toggled');
    $('.sidebar-settings').toggleClass('toggled');
    $('.icon-arrow').toggleClass('toggled');
    $('.copyright').toggleClass('toggled');
    $('.loading').toggleClass('toggled');
    $('.icon-hideez').toggleClass('toggled');
}