$(document).on('submit', '#positionForm', function (e) {
    var exists = false;
    var current = $('.orgstructure-popover').val();
    $.ajax({
        url: "/Settings/Positions?handler=JsonPosition",
        type: "Get",
        async: false,
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