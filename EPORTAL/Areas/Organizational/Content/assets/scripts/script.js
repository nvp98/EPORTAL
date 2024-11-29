/*!
    * Start Bootstrap - SB Admin v7.0.4 (https://startbootstrap.com/template/sb-admin)
    * Copyright 2013-2021 Start Bootstrap
    * Licensed under MIT (https://github.com/StartBootstrap/startbootstrap-sb-admin/blob/master/LICENSE)
    */
// 
// Scripts
// 

window.addEventListener('DOMContentLoaded', event => {

    // Toggle the side navigation
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        // Uncomment Below to persist sidebar toggle between refreshes
         //if (localStorage.getItem('sb|sidebar-toggle') === 'true') {
         //    document.body.classList.toggle('sb-sidenav-toggled');
         //}
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
            
        });
    }

});

// $(window).scroll(function(){
//     $('#sidenavAccordion').css({marginTop:0})
//     if ($(window).scrollTop() < 400) {
//      $("#layoutSidenav_nav").addClass("position-relative");
//      $('#layoutSidenav_content').css({paddingLeft:'60px'});  
//   } else {
//      $("#layoutSidenav_nav").removeClass("position-relative");
//      $('#layoutSidenav_content').css({paddingLeft:'300px'});
//   }
//  });
//$("#sidebarToggle").click(function () {
//    console.log("abc");
//    $(this).find("i").toggleClass("fa-bars fa-eye-slash");
//});
$(".pxI").click(function () {
    $(".pxI").css({ width: '100%' });
    $(".nhomI").css({ width: '100%' });
    $(this).css({ width: '120%' });
    $(".showI").hide();
    $('#showI_' + $(this).attr('id')).show();
})

$(".topI").click(function () {
    $(".topI").removeClass("menuTopI");
    $(this).addClass("menuTopI");
})
$(".menuTab").click(function () {
    $(".menuTab").removeClass("menuTabI")
    $(this).addClass("menuTabI");
})
