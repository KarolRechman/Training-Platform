/**
 * JS file for "_Layout" and "_Material_Layout"
 * 
 */

//4 start Page
new WOW().init();
var controller = document.location.pathname.split("/"),
    controller = document.location.pathname.split("/"),
    url = controller[0] + "/" + controller[1] + "/" + controller[2];


var TimerIntervalID;
//4 test page
if (controller[3] == "Test_Page") {
    //4 timer
    var Id_Campaign = getUrlVars()["Id_Campaign"],
        Id_Presentation = getUrlVars()["Id_Presentation"];

    var timeStamp = new Date().getTime(),
        sessionStamp = sessionStorage.getItem('ts'),
        elapsedTime;
    if ((Id_Campaign != sessionStorage.getItem('Id_Campaign')) || (Id_Presentation != sessionStorage.getItem('Id_Presentation'))) {
        sessionStamp = 0;
    }
    if (!sessionStamp) {
        sessionStorage.setItem('ts', timeStamp.toString());
        sessionStamp = timeStamp;
        sessionStorage.setItem('Id_Campaign', Id_Campaign);
        sessionStorage.setItem('Id_Presentation', Id_Presentation);
    }
    else {
        sessionStamp = parseInt(sessionStamp);
    }

    var deadLine = sessionStamp + (1 * 60 * 60 * 1000)+10;

    TimerInterval();
}

function TimerInterval() {
    TimerIntervalID = setInterval(Timer, 1000);
}
function Timer() {
    elapsedTime = deadLine - new Date().getTime();
    $('.Timer').text(get_elapsed_time_string(elapsedTime));
    $('#time').val(get_elapsed_time_string(elapsedTime));
    if (elapsedTime < 1) {
        clearInterval(TimerIntervalID);
        $('.Timer').text("Time's up");
        $('#time').val("00:00:00");
        $("#Answers").submit();
        sessionStorage.removeItem(Id_Campaign);
        sessionStorage.removeItem(Id_Presentation);        
        //setInterval(function () {
        //    window.location.replace(controller[0] + "/" + controller[1] + "/UserPages/Trainings_List");
        //}, 3000);
    }
}

function get_elapsed_time_string(total_seconds) {
    function pretty_time_string(num) {
        return (num < 10 ? "0" : "") + num;
    }

    var hours = Math.floor((total_seconds % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    var minutes = Math.floor((total_seconds % (1000 * 60 * 60)) / (1000 * 60));
    var seconds = Math.floor((total_seconds % (1000 * 60)) / 1000);

    //var hours = Math.floor(total_seconds * 3600);
    //total_seconds = total_seconds * 3600;

    //var minutes = Math.floor(total_seconds * 60);
    //total_seconds = total_seconds * 60;

    //var seconds = Math.floor(total_seconds);

    // Pad the minutes and seconds with leading zeros, if required
    hours = pretty_time_string(hours);
    minutes = pretty_time_string(minutes);
    seconds = pretty_time_string(seconds);

    // Compose the string for display
    var currentTimeString = hours + ":" + minutes + ":" + seconds;

    return currentTimeString;
}
//4 slider
var slideIndex = 1;
showSlides(slideIndex);

function plusSlides(n) {
    showSlides(slideIndex += n);
}

function currentSlide(n) {
    showSlides(slideIndex = n);
}

function showSlides(n) {
    var i;
    var slides = document.getElementsByClassName("mySlides");
    var dots = document.getElementsByClassName("dot");

    if (n > slides.length) {
        slideIndex = 1
    }
    if (n < 1) {
        slideIndex = slides.length
    }
    for (i = 0; i < slides.length; i++) {
        slides[i].style.display = "none";
    }
    for (i = 0; i < dots.length; i++) {
        dots[i].className = dots[i].className.replace(" active", "");
    }

    $(slides[slideIndex - 1]).fadeIn(1100);

    if (dots.length > 0) {
        dots[slideIndex - 1].className += " active";
    }
    if (n == slides.length) {
        $("#Next_Button").css('visibility', 'hidden');
    } else {
        $("#Next_Button").css('visibility', 'visible');
    }
    if (n == 0 || n == 1) {
        $("#Prev_Button").css('visibility', 'hidden');
    } else {
        $("#Prev_Button").css('visibility', 'visible');
    }
}

//4 test page

var divs = $('.checkbox-group');
var checkbox_group = [];

for (var id = 0; id < divs.length; id++) {
    var checkbox_group_checkboxes = $(".checkbox-group." + id + " input[type='checkbox']");
    checkbox_group.push(checkbox_group_checkboxes);
}

function check(j, id) {

    var check_Id = "z" + j + "__Answers_" + id + "__Correct_Answer";

    if (document.getElementById(check_Id).checked == false) {
        document.getElementById(check_Id).checked = true;
    } else {
        document.getElementById(check_Id).checked = false;
    }

    var i, counter = 0;

    for (i = 0; i < checkbox_group.length; i++) {
        if (checkbox_group[i].filter(':checked').length) {
            counter += 1;
        }
    }
    if (counter == i) {
        $("#Sub_Ans").removeClass('btn btn-elegant btn-lg');
        $("#Sub_Ans").addClass('btn btn-green accent btn-lg');
        $("#Sub_Ans").css('visibility', 'visible');
        $("#Sub_Ans").prop('disabled', false);
    } else {
        $("#Sub_Ans").removeClass('btn btn-green accent btn-lg');
        $("#Sub_Ans").addClass('btn btn-elegant btn-lg');
        $("#Sub_Ans").css('visibility', 'hidden');
        $("#Sub_Ans").prop('disabled', true);
    }
}

function Clear_Answers() {
    $(".checkbox-group input[type='checkbox']").prop('checked', false);
}
//mayby usless !!!!!!!!!!!!!!!!!!!!!!!!!!!
//4 CREATE QUESTION
//var close = document.getElementsByClassName("closebtn");
//var i;
////4 CREATE QUESTION
//for (i = 0; i < close.length; i++) {
//    close[i].onclick = function () {
//        var div = this.parentElement;
//        div.style.opacity = "0";
//        setTimeout(function () { div.style.display = "none"; }, 680);
//    }
//}

//4 CREATE QUESTION
function userTyped(id, e, cont) {
    if (e.value.length > 0) {
        document.getElementById(id).disabled = false;
        document.getElementById(cont).hidden = false;
        cont = "#" + cont;
        $(cont).show("slow");
    } else {
        document.getElementById(id).checked = false;
        document.getElementById(id).disabled = true;
        cont = "#" + cont;
        $(cont).hide("slow");
    }
}
//mayby usless !!!!!!!!!!!!!!!!!!!!!!!!!!!!
// 4 CREATE QUESTION   createButton 
//$(window).bind("resize", function () {
//    if ($(this).width() < 575) {
//        $('#first').addClass('d-flex flex-column-reverse');
//    } else {
//        $('#first').removeClass('d-flex flex-column-reverse');
//    }
//}).trigger('resize');

debugger;

function preventBack() {
    window.history.forward();
}
//4 url query variables
function getUrlVars() {
    var vars = [],
        hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

//var stateObj = { foo: "bar" };
//history.pushState(stateObj, "page without extension", "xyz");

$(document).ready(function () {
    //sessionStorage

    //$("#White_Button").mouseenter(function () {
    //    //$("#White_Button").toggleClass('btn-outline-white');
    //    $("#White_Button").addClass('hover_B');
    //});
    //$("#White_Button").mouseleave(function () {
    //    //$("#White_Button").toggleClass('btn-outline-white');
    //    $("#White_Button").removeClass('hover_B');
    //});

    $("#Sub_Ans").on('click', function () {
        sessionStorage.removeItem('ts');
    });

    if (controller[2] != "AdminPages") {
        preventBack();
        setTimeout("preventBack()", 0);
        window.onunload = function () { null };
    }

    //window.location.hash = "no-back-button";
    //window.location.hash = "Again-No-back-button";//again because google chrome don't insert first hash into history
    //window.onhashchange = function () { window.location.hash = "no-back-button"; }

    //function disablePrev() { window.history.forward() }
    //window.onload = disablePrev();
    //window.onpageshow = function (evt) { if (evt.persisted) disableBack() }

    $('#exampleModalCenter').on('hidden.bs.modal', function () {
        window.location.replace(controller[0] + "/" + controller[1] + "/" + "UserPages/" + "Trainings_List");
    });


    // 4 PRESENTATION
    if (($("#myModalLabel:contains(points)").length > 0)) {
        $('#exampleModalCenter').modal('show');
        //$(".form-control").val('');
    }
    if (($("#ErrorMsg:contains(Please)").length > 0)) {
        $('#exampleModalCenter').modal('show');
        //$(".form-control").val('');
    }
    if (($("#BrowserError:contains(Please)").length > 0)) {
        $('#exampleModalCenter').modal('show');
    }
    if (($("#BrowserError:contains(Login)").length > 0)) {
        $("#myModalLabel").html("Something went wrong !!");
        $('#exampleModalCenter').modal('show');
    }

    // 4 PRESENTATION
    $('.carousel').carousel({
        interval: 9999999
    });
    $('.carousel').carousel('pause');
    //not here!!!!!!!!!!!!!!!!!!!!!!!!!
    //4 CREATE QUESTION  
    //if (($("#Bag:contains(try)").length > 0)) {
    //    $("#alert").css("background-color", "#ff9800");
    //    document.getElementById("alert").hidden = false;
    //    if (document.getElementById("alert").hidden == false) {
    //        //document.getElementById("legend").hidden = true;
    //    }
    //}
    ////4 CREATE QUESTION 
    //if (($("#Bag:contains(U)").length > 0)) {
    //    $("#alert").css("background-color", "#4CAF50");
    //    document.getElementById("alert").hidden = false;
    //    if (document.getElementById("alert").hidden == false) {
    //        //document.getElementById("legend").hidden = true;
    //    }
    //}
    ////4 CREATE QUESTION     legend
    //if ($("#Bag:contains(Please)").length > 0) {
    //    if ($("#Answer_3").val().length > 0) {
    //        document.getElementById("Correct_3").disabled = false;
    //        document.getElementById("containerCheck3").hidden = false;
    //        $("#containerCheck3").show();
    //    }
    //    if ($("#Answer_4").val().length > 0) {
    //        document.getElementById("Correct_4").disabled = false;
    //        document.getElementById("containerCheck4").hidden = false;
    //        $("#containerCheck4").show();
    //    }
    //    document.getElementById("alert").hidden = false;
    //    if (document.getElementById("alert").hidden == false) {
    //        //document.getElementById("legend").hidden = true;
    //    }
    //} else {
    //    $("#containerCheck3").hide();
    //    $("#containerCheck4").hide();
    //}


    // 4 PRESENTATION
    $('#collapseExample').on('shown.bs.collapse', function () {
        if (window.matchMedia('(max-width: 600px)').matches) {
            $(".carousel-control-prev").css("height", "20%");
            $(".carousel-control-prev").css("top", "75%");
            $(".carousel-control-next").css("height", "20%");
            $(".carousel-control-next").css("top", "75%");
        } else {
            $(".carousel-control-prev").css("height", "50%");
            $(".carousel-control-prev").css("top", "39%");
            $(".carousel-control-next").css("height", "50%");
            $(".carousel-control-next").css("top", "39%");
        }
    })

    $('#collapseExample').on('hidden.bs.collapse', function () {
        $(".carousel-control-prev").css("height", "90%");
        $(".carousel-control-prev").css("top", "5%");
        $(".carousel-control-next").css("height", "90%");
        $(".carousel-control-next").css("top", "5%");
    })
});


