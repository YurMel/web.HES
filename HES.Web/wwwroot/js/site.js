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

// DataTables initialization
function InitDeviceTasksDT() {
    var table_name = '#deviceTasks';
    var table = $(table_name).DataTable({
        responsive: true,
        "order": [[3, "dsec"]],
        "columnDefs": [
            { "orderable": false, "targets": [0] }
        ]
    });
    var dataTable = $(table_name).dataTable();
    // Search box
    $('#searchbox').keyup(function () {
        dataTable.fnFilter(this.value);
    });
    $('.dataTables_filter').addClass('d-none');
    // Length
    $('#entries_place').html($('.dataTables_length select').removeClass('custom-select-sm form-control-sm'));
    $('.dataTables_length').addClass('d-none');
    // Info
    $('#showing_place').html($('.dataTables_info'));
    // Paginate
    $('#pagination_place').html($('.dataTables_paginate'));
}