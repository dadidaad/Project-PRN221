"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/hub").build();
connection.start().then().catch(function (err) {
    return console.log(err.toString());
});
connection.on("ReloadOrderAdmin", (data) => {
    var tr = '';
    console.log(data.$values);
    $.each(data.$values, (k, v) => {
        var status = '';
        if (v.ShippedDate) {
            status += '<p style="color: green;">Completed</p>'
        }
        if (!v.ShippedDate && !v.Employee && v.RequiredDate) {
            status += `<p style="color:blue">Pending</p><a href="/Admin/Order/Order/CancelOrder?OrderId=${v.OrderId}" style="color:red;">Cancel</a>`
        }
        if (!v.RequiredDate) {
            status += ' <p style="color: red">Canceled</p>';
        }
        tr += `<tr>
                        <td><a href="/Admin/Order/OrderDetail?OrderId=${v.OrderId}">${v.OrderId}</a></td>
                        <td>${formatDate(new Date(v.OrderDate)) }</td>
                        <td>${v.RequiredDate ? formatDate(new Date(v.RequiredDate)) : ""}</td>
                        <td>${v.ShippedDate ? formatDate(new Date(v.ShippedDate)) : ""}</td>
                        <td>${v.Employee ? v.Employee.LastName : ""}</td>
                        <td>${v.Customer.ContactName}</td>
                        <td>${v.Freight.toFixed(2)}</td>
                        <td>
                            ${status}
                        </td>
              </tr>`;
    })
    console.log(tr);
    $("#ordersAdminBody").html(tr);
});

function formatDate(date) {
    var d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();

    if (month.length < 2)
        month = '0' + month;
    if (day.length < 2)
        day = '0' + day;

    return [day, month, year].join('/');
}

connection.on("ReloadAdminProduct", (data) => {
    var tr = '';
    console.log(data.$values);
    $.each(data.$values, (k, v) => {
        tr += `<tr>
                            <td>${v.ProductId}</td>
                            <td>${v.ProductName}</td>
                            <td>${v.UnitPrice.toFixed(2)}</td>
                            <td>${v.QuantityPerUnit}</td>
                            <td>${v.UnitsInStock}</td>
                            <td>${v.Category.CategoryName}</td>
                            <td>${v.Discontinued}</td>
                            <td>
                                <a href="/Admin/Product/Edit?ProductId=${v.ProductId}">Edit</a> &nbsp; | &nbsp;
                                <a href="/Admin/Product/Delete?ProductId=${v.ProductId}">Delete</a>
                            </td>
                        </tr>`;
    })
    console.log(tr);
    $("#productsTable").html(tr);
});

connection.on("ReloadCart", (data) => {
    console.log(data);
    $("#cart-number").html(data);
});
function ajaxAddToCart(productId) {
    $.ajax({
        type: "Get",
        url: `/Account/Cart?ProductId=${productId}&handler=addToCart`,
        error: function (xhr, status, errorThrown) {
            var err = "Status: " + status + " " + errorThrown;
            $('#status').html(err);
        }
    }).done(function (data) {
        $('#status').html('Add success');
    })
}