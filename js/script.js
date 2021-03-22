
/* =========================================
              Navigation
============================================ */

function sticky_header() {
    var header_height = $('.home_navbar').innerHeight() / 2;
    var scrollTop = $(window).scrollTop();
    if (scrollTop > header_height) {
        $('body').addClass('sticky-nav');
        $(".home_navbar .navbar img").attr("src", "images/logo/logo-blue.png");
    } else {
        $('body').removeClass('sticky-nav');
        $(".home_navbar .navbar img").attr("src", "images/logo/logo-white.png");
    }
}

$(document).ready(function () {
  sticky_header();
});

$(window).scroll(function () {
  sticky_header();  
});
$(window).resize(function () {
  sticky_header();
});


$("#mobile-nav-icon").click(function(){

  $('body').addClass('sticky-nav');
  $(".home_navbar .navbar img").attr("src", "images/logo/logo-blue.png");
  
});

/*================================
 Password eye icon
==================================*/
$(".toggle-password").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input = $($(this).attr("toggle"));
  if (input.attr("type") == "password") {
    input.attr("type", "text");
  } else {
    input.attr("type", "password");
  }
});



/*---------------*/

var acc = document.getElementsByClassName("accordion");
var i;

for (i = 0; i < acc.length; i++) {
  acc[i].addEventListener("click", function() {
    /* Toggle between adding and removing the "active" class,
    to highlight the button that controls the panel */
    this.classList.toggle("active");

    /* Toggle between hiding and showing the active panel */
    var panel = this.nextElementSibling;
    if (panel.style.display === "block") {
      panel.style.display = "none";
    } else {
      panel.style.display = "block";
    }
  });
}