// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
window.onload = function () {
    document.getElementById("div1").style.display = 'none';
}


function showMe(box) {
    var chboxs = document.getElementsByName("c1");
    var vis = "none";

    for (var i = 0; i < chboxs.length; i++) {
        if (chboxs[i].checked) {
            vis = "block";
            break;
        }
    }
    document.getElementById(box).style.display = vis;
}