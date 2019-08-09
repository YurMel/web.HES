// selected items
var items = [];

// Check all
$(document).on('change', '#checkbox_all', function () {
    if (this.checked) {
        items = [];
        table.$('input[type="checkbox"]').each(function () {
            this.checked = true;
            items.push(this.id);
        });
    }
    else {
        items = [];
        table.$('input[type="checkbox"]').each(function () {
            this.checked = false;
        });
    }

    if (items.length > 0) {
        $('#btnProfile').removeClass('d-none');
    } else {
        $('#btnProfile').addClass('d-none');
    }
});

// Check single
$(document).on('change', '.table-checkbox', function () {
    items = [];
    table.$('input[type="checkbox"]').each(function () {
        if (this.checked) {
            items.push(this.id);
        }
    });

    if (items.length > 0) {
        $('#btnProfile').removeClass('d-none');
    } else {
        $('#btnProfile').addClass('d-none');
    }
});