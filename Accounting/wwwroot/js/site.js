// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

let sel1 = document.querySelector('#i1'),
    sel2 = document.querySelector('#i2'),
    sel3 = document.querySelector('#i3')
    

sel1.addEventListener("change", function () {
    if (sel1.value === 'Income') {
        sel3.style.display = 'none';
        sel2.style.display = 'inline-block';
    } else if (sel1.value === 'Debt') {
        sel2.style.display = 'none';
        sel3.style.display = 'inline-block';
    }
});

