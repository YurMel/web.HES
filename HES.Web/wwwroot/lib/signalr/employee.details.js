"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/employeeDetailsHub").build();

connection.start();

connection.on("ReloadPage", function (id) {
    //var table = $('#' + id).DataTable();
    //table.rows().every(function (index, element) {
    //    var row = $(this.node());
    //    var col_name = row.find('td').eq(0).text().trim();
    //    var col_login = row.find('td').eq(3).text().trim();
    //    if (col_name === name && col_login === login) {
    //        row.find("td:eq(5)").text(status);
    //    }
    //});
    var isShown = $('#modalDialog').hasClass('show');
    if (isShown == false) {
        var loc = window.location.href;
        var emp = loc.indexOf(id);
        if (emp != -1) {
            location.reload();
        }
    }
});