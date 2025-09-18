// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// scripts.js
document.getElementById("menuBtn").addEventListener("click", () => {
    document.getElementById("mobileMenu").classList.toggle("hidden");
});
document.getElementById("year").textContent = new Date().getFullYear();