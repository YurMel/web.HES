$(document).on('submit', '#departmentForm', function (e) {
    var exists = false;
    var current = $('.orgstructure-popover').val();
    var id = $('#companyId option:selected').val();
    $.ajax({
        url: "/Settings/OrgStructure?handler=JsonDepartment",
        type: "Get",
        async: false,
        data: { id: id },
        success: function (response) {
            $(response).each(function () {
                if (this.name.toUpperCase() === current.trim().toUpperCase()) {
                    exists = true;
                    return;
                }
            });
        }
    });
    if (exists) {
        e.preventDefault();
        $('.orgstructure-popover').popover('toggle');
    }
});

$(document).on('input', '.orgstructure-popover', function () {
    $('.orgstructure-popover').popover('hide');
});