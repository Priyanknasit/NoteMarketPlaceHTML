
/* =========================================
              Navigation
============================================ */

function sticky_header() {
    var header_height = jQuery('.home_navbar').innerHeight() / 2;
    var scrollTop = jQuery(window).scrollTop();;
    if (scrollTop > header_height) {
        jQuery('body').addClass('sticky-nav')
        $(".home_navbar .navbar img").attr("src", "../Content/images/logo/logo-blue.png");
    } else {
        jQuery('body').removeClass('sticky-nav')
        $(".home_navbar .navbar img").attr("src", "../Content/images/logo/logo-white.png");
    }
}

jQuery(document).ready(function () {
  sticky_header();
});

jQuery(window).scroll(function () {
  sticky_header();  
});
jQuery(window).resize(function () {
  sticky_header();
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

// Add notes
function disable() {
    document.getElementById("sell-price").disabled = true;
}

function undisable() {
    document.getElementById("sell-price").disabled = false;
}

// user profile

// get profile picture 
$('#upload_profile').click(function () {
    $("#ProfilePicture").click();
});

// add notes

// get display picture
$('#upload_dp').click(function () {
    $("#DisplayPicture").click();
});

// get note 
$('#upload_notes').click(function () {
    $("#UploadNotes").click();
});

// get note preview
$('#NotePreview_img').click(function () {
    $("#NotePreview").click();
});


// Pagination


$(function () {
    var table = $('#inProgresstbl').DataTable({
        'pageLength': 5,
        'dom':'tp',
        'language': {
            'paginate': {
                'previous': '<a aria-hidden="true"> <img src="../Content/images/icons/left-arrow.png" alt="previous"> </a>',
                'next': '<a aria-hidden="true"> <img src="../Content/images/icons/right-arrow.png"> </a>'
            }
        }
    });

    $('.searc_btn').click(function () {
        table.search($(' #searchbox').val()).draw();
    });
});

$(function () {
    var table = $('#inPublishtbl').DataTable({
        'pageLength': 5,
        'dom': 'tp',
        'language': {
            'paginate': {
                'previous': '<a aria-hidden="true"> <img src="../Content/images/icons/left-arrow.png" alt="previous"> </a>',
                'next': '<a aria-hidden="true"> <img src="../Content/images/icons/right-arrow.png" alt="next"> </a>'
            }
        }
    });

    $('.search_publish_btn').click(function () {
        table.search($(' #search_publish_box').val()).draw();
    });
});



