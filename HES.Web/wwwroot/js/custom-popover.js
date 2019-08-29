$('.custom-popover').each(function () {
    var currentId = $(this).attr('id');
    $("#show_" + currentId).click(function () {
        $("#" + currentId).fadeToggle("fast");
    });

    $("#show_" + currentId).focusout(function () {
        $("#" + currentId).fadeOut("fast");
    });
});