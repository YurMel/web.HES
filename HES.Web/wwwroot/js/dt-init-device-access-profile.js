// Initialize DataTables
var table_name = '#deviceAccessProfiles';
var table = $(table_name).DataTable({
    responsive: true,
    "columnDefs": [
        { "orderable": false, "targets": [0,5] }
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