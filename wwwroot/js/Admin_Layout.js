// Get the Sidebar
var mySidebar = document.getElementById("mySidebar");
var controller = document.location.pathname.split("/");
var url = controller[0] + "/" + controller[1] + "/" + controller[2];
// Get the DIV with overlay effect
var overlayBg = document.getElementById("myOverlay");
new WOW().init();
//4 edit page date picker
$('#dates').daterangepicker({
    "autoApply": true,
    "locale": {
        "format": "DD.MM.YYYY",
        "separator": " - ",
        "applyLabel": "Apply",
        "cancelLabel": "Cancel",
        "fromLabel": "From",
        "toLabel": "To",
        "customRangeLabel": "Custom",
        "weekLabel": "W",
        "daysOfWeek": [
            "Su",
            "Mo",
            "Tu",
            "We",
            "Th",
            "Fr",
            "Sa"
        ],
        "monthNames": [
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"
        ],
        "firstDay": 1
    }
});

//$('#daterange').data('daterangepicker').setStartDate('03/01/2014');
//$('#daterange').data('daterangepicker').setEndDate('03/31/2014');

// Toggle between showing and hiding the sidebar, and add overlay effect
function w3_open() {
    if (mySidebar.style.display === 'block') {
        mySidebar.style.display = 'none';
        overlayBg.style.display = "none";
    } else {
        mySidebar.style.display = 'block';
        overlayBg.style.display = "block";
    }
}

// Close the sidebar with the close button
function w3_close() {
    mySidebar.style.display = "none";
    overlayBg.style.display = "none";
}
//debugger;
//4 menu accordion
function Rotate(element) {
    element = "#" + element;
    var all_Rest = $('.Arrow_Icon');
    all_Rest = all_Rest.not(element);

    if (all_Rest.hasClass('Rotate_Arrow')) {
        all_Rest.removeClass('Rotate_Arrow');
    }

    $(element).toggleClass('Rotate_Arrow');

}



//4 CREATE QUESTION
function userTyped(id, e, cont, a) {
    var input = document.getElementById(e);

    if (input.value.length > 0) {
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
//GetAnswers

function GetAnswers(ID) {
    var answers = "#Answers" + ID;
    var TogglerAnswers = "#TogglerAnswers" + ID;
    var Icon = "#Icon_" + ID;

    $(TogglerAnswers).toggle();
    $(Icon).toggleClass('Rotate');

    if ($(answers).is(':empty')) {
        $.ajax({
            url: url + "/GetAnswers/" + ID,
            type: "GET",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            success: function (result) {
                var content = "";

                if (result != null) {
                    var resLen = result.length;
                    var vheight = 5;
                    $(TogglerAnswers).css("min-height", "" + vheight * resLen + "vh");
                    for (var i = 0; i < result.length; i++) {
                        if (result[i].correct_Answer == true) {
                            content += "<p class='green-text lighter-hover' id='Correct_A'>" + result[i].answer; +"</p>"
                        } else {
                            content += "<p class='red-text'>" + result[i].answer; +"</p>"
                        }
                    }
                    $(answers).html(content);
                }
            },
            error: function (errormessage) {
                alert(errormessage.responseText);
            }
        });
        return false;
    }
}

//Trainings_List
function GetAllResults(number, Id_Campaign, Id_Presentation) {

    var TogglerId = "#toggleTr" + number;
    var AllResults = "#AllResults" + number;
    var value = $(AllResults).html();
    $(TogglerId).toggle('fast');
    if (value == '') {

        var Presentations = new Array();
        Presentations.push({
            Id: Id_Presentation
        });

        var object = {
            Id: Id_Campaign,
            Presentations: Presentations
        }

        var Json = JSON.stringify(object);
        $.ajax({
            type: "POST",
            url: url + "/Get_All_Results/",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            data: Json,
            success: function (result) {
                var content = "<div class='row'>";
                for (var i = 0; i < result.length; i++) {
                    content += "<div class='col-md-4 p-3' ><dl class='row'><dt class='col-5'>Attempt date:</dt><dd class='col-7'>" + result[i].attempt_Date + "</dd><dt class='col-5'>Points:</dt><dd class='col-7'>"
                    content += result[i].user_Points + "</dd></dl><div class='border_Bottom'></div></div>";
                }
                content += "</div>";
                $(AllResults).html(content);
            },
            error: function (errormessage) {
                alert(errormessage.responseText);
            }
        });
    }
}

//getDetailsByID
function getDetailsByID(ID) {

    var Author = "#Author" + ID;
    var Create_Date = "#Create_Date" + ID;
    var Modify_Date = "#Modify_Date" + ID;
    var Icon = "#Icon_" + ID;
    var TogglerId = "#toggleTr" + ID;

    $(TogglerId).toggle('fast');
    $(Icon).toggleClass('Rotate');

    if ($(Author).is(':empty')) {

        if (controller[3] == "Campaigns_List") {
            $.ajax({
                url: url + "/Campaign_Details/" + ID,
                type: "GET",
                contentType: "application/json;charset=UTF-8",
                dataType: "json",
                success: function (result) {
                    var Group = "#Group" + ID;
                    var Users = "#Users" + ID;
                    var Presentations = "#Presentations" + ID;
                    var Presentations_Content = "";
                    var Groups_Content = "";
                    var Users_Content = "";

                    $(Author).html(result.author_Name);
                    $(Create_Date).html(result.create_Date);

                    for (var i = 0; i < result.presentations.length; i++) {
                        Presentations_Content += "<a class='Deco_Link' href = '" + url + "/Edit_Training/" + result.presentations[i].id + "'><span>" + result.presentations[i].name + "</span></a>, ";
                    }
                    $(Presentations).html(Presentations_Content);

                    for (var i = 0; i < result.training_Groups.length; i++) {
                        Groups_Content += "<a class='Deco_Link' href='" + url + "/Edit_Group/" + result.training_Groups[i].id_Group + "'><span>" + result.training_Groups[i].name + "</span></a>, ";
                    }
                    $(Group).html(Groups_Content);

                    for (var i = 0; i < result.users_Assigned.length; i++) {
                        Users_Content += "<span>" + result.users_Assigned[i].first_Name + " " + result.users_Assigned[i].last_Name + "</span>, ";
                    }
                    $(Users).html(Users_Content);

                    $(Modify_Date).html(result.last_Modification_Date);
                },
                error: function (errormessage) {
                    alert(errormessage.responseText);
                }
            });

        } else {
            $.ajax({
                url: url + "/Delete_PresentationAJAX/" + ID,
                type: "GET",
                contentType: "application/json;charset=UTF-8",
                dataType: "json",
                success: function (result) {
                    var Type = "#Type" + ID;
                    //var Create_Date = "#Create_Date" + ID;
                    //var Modify_Date = "#Modify_Date" + ID;

                    $(Author).html(result.author_Name);
                    $(Create_Date).html(result.create_Date);
                    $(Type).html(result.type);
                    $(Modify_Date).html(result.last_Modification_Date);
                },
                error: function (errormessage) {
                    alert(errormessage.responseText);
                }
            });
            return false;
        }
    }
}

//Delete modal   Delete_getbyID
function getbyID(ID) {
    if ($("#Name").is(':empty')) {
        $.ajax({
            url: url + "/Delete_PresentationAJAX/" + ID,
            type: "GET",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            success: function (result) {

                if (result.name == "" || result.name == null) {
                    $("#DeleteError").html("Training don't exists in database !!");
                    $("#NoButton").attr("disabled", true);
                    $("#ModelName").css("display", "none");
                    $("#ModelType").css("display", "none");
                    $("#myModalLabel").css("display", "none");
                } else {
                    $("#DeleteError").html("Are you sure you want to delete this training ?");
                    $("#NoButton").attr("disabled", false);
                    $("#ModelName").css("display", "inline");
                    $("#ModelType").css("display", "inline");

                    $('#Name').html(result.name);
                    $('#Author').html(result.author_Name);
                    $('#Create_Date').html(result.create_Date);
                    $('#asp').attr('href', url + "/DeleteConfirmed/" + ID);
                    $('#myModal').modal('show');
                }
            },
            error: function (errormessage) {
                alert(errormessage.responseText);
            }
        });
        return false;
    }
}

function GetForDeleteCampaign(ID) {
    if ($("#Name").is(':empty')) {
        $.ajax({
            url: url + "/Get_For_Delete_Campaign/" + ID,
            type: "GET",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            success: function (result) {
                if (result.name == "" || result.name == null) {
                    $("#DeleteError").html("Campaign don't exists in database !!");
                    $("#NoButton").attr("disabled", true);
                    $("#ModelName").css("display", "none");
                    $("#ModelType").css("display", "none");
                    $("#myModalLabel").css("display", "none");
                } else {
                    $('#Name').html(result.name);
                    $('#Trainings_Modal').html(result.presentations.length);
                    $('#Employees_Modal').html(result.users_Assigned.length);
                    $('#asp').attr('href', url + "/Campaign_DeleteConfirmed/" + ID);
                    $('#myModal').modal('show');
                }
            },
            error: function (errormessage) {
                alert(errormessage.responseText);
            }
        });
    }
}

function Delete_getbyID(ID) {
    if ($("#Name").is(':empty')) {
        $.ajax({
            url: url + "/Delete_QuestionAJAX/" + ID,
            type: "GET",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            success: function (result) {

                if (result.presentation_Name == "" || result.presentation_Name == null) {
                    $("#DeleteError").html("Training don't exists in database !!");
                    $("#NoButton").attr("disabled", true);
                    $("#ModelName").css("display", "none");
                    $("#ModelType").css("display", "none");
                    $("#myModalLabel").css("display", "none");
                }
                $('#Name').html(result.presentation_Name);
                //$('#Url').html(result.url);   DeleteError
                $('#Type_Name').html(result.question);
                $('#asp').attr('href', url + "/Question_DeleteConfirmed/" + ID);

                if (result != null) {
                    $('#myModal').modal('show');
                }
            },
            error: function (errormessage) {
                alert(errormessage.responseText);
            }
        });
        return false;
    }
}

function getInfobyID(ID) {
    if ($("#Name").is(':empty')) {
        $.ajax({
            url: url + "/Delete_Group/" + ID,
            type: "GET",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            success: function (result) {

                if (result.name == "" || result.name == null) {
                    $("#DeleteError").html("Group don't exists in database !!");
                    $("#NoButton").attr("disabled", true);
                    $("#ModelName").css("display", "none");
                    $("#ModelType").css("display", "none");
                    $("#myModalLabel").css("display", "none");
                }
                $('#Name').html(result.name);
                //$('#Url').html(result.url);   DeleteError
                $('#Employees').html(result.users.length);
                $('#asp').attr('href', url + "/Delete_Group_Confirmed/" + ID);

                if (result != null) {
                    $('#myModal').modal('show');
                }
            },
            error: function (errormessage) {
                alert(errormessage.responseText);
            }
        });
        return false;
    }
}

function DeletFilebyName(NAME, TYPE, ID, ALL = "") {
    var dict = {
        "Name": NAME,
        "Id": ID,
        "All": ALL
    };
    const params = new URLSearchParams();
    for (const [key, val] of Object.entries(dict)) {
        params.append(key, val);
    }
    $('#Type_Name').html(TYPE);
    $('#File_Name').html(NAME);
    if (ALL == "") {
        $('#DeleteError').html("Are you sure you want to delete this file ?");
        $('#ModelName').html('File name');
    } else {
        $('#DeleteError').html("Are you sure you want to delete all files ?");
        $('#ModelName').html('Training name');
    }

    $('#asp').attr('href', url + "/File_DeleteConfirmed2/?" + params.toString() + "");
    $('#myModal').modal('show');
}

//function DeleteAllFiles(NAME, TYPE, ID) {
//    $('#DeleteError').html("Are you sure you want to delete all files ?");
//    $('#ModelName').html('Training name');
//    $('#File_Name').html(NAME);
//    $('#Type_Name').html(TYPE);
//    $('#asp').attr('href', url + "/All_Files_DeleteConfirmed/" + NAME + "");
//    $('#myModal').modal('show');
//}

function Clear() {
    $("#myModal p").html("");
}

function photo_Modal(photo_Id, name) {
    var img = "#" + photo_Id;
    var src = $(img).attr('src');

    $("#Photo_On_Modal").prop('src', src);
    $("#Photo_Name").html(name);
    $('#asp_Photo').attr('href', url + "/File_DeleteConfirmed/" + name + "");

    $('#myModal_Photo').modal('show');
}


//4 create session

//4 create_group page and all bigTables
var Id_list = new Array();

function Assign(id) {
    var removeIndex = Id_list.map(function (item) { return item.Id_User; }).indexOf(id);
    if (removeIndex != (-1)) {
        Id_list.splice(removeIndex, 1);
    } else {
        Id_list.push({ Id_User: id });
    }
}

function NoName(string) {
    $("#myModalLabel").html("Something went wrong !!");
    $("#M_Class").removeClass("modal-dialog modal-dialog-centered modal-notify modal-success modal-lg");
    $("#M_Class").addClass("modal-dialog modal-dialog-centered modal-notify modal-danger modal-lg");
    if (string == 'name') {
        $("#ErrorMsg").html("Please write correct name !!");
    } else if (string == 'training') {
        $("#ErrorMsg").html("Please choose a training !!");
    } else if (string == 'dates') {
        $("#ErrorMsg").html("Please choose dates different then default and more than one day !!");
    } else if (string == 'employees') {
        $("#ErrorMsg").html("You didn't choose anyone for this event !!");
    } else if (string == 'Attempts') {
        $("#ErrorMsg").html("Please choose correct attempts value !!");
    }
    if ($("#ErrorMsg:contains(!!)").length > 0) {
        $('#exampleModalCenter').modal('show');
    }
}

var today = new Date();
var dd = String(today.getDate()).padStart(2, '0');
var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
var yyyy = today.getFullYear();

today = dd + '.' + mm + '.' + yyyy;

function preventBack() {
    window.history.forward();
}

//4 quest_list
//var Points_Needed = parseInt($("#Points_Needed").val());
var Question_Count = parseInt($("#Question_Count").html());
//var Percentage = parseFloat((Points_Needed / Question_Count) * 100).toFixed(2);
$("#requirements").on('click', function () {
    var Points_Needed = parseInt($("#Points_Needed").val());
    var Percentage = parseFloat((Points_Needed / Question_Count) * 100).toFixed(2);
    $("#Percentage").html(Percentage.toString() + " %");
    $('#requirements_Modal').modal('show');
    SetPercentage("#Points_Needed", parseInt($("#Question_Count").html()), "#Percentage");
});

function SetPercentage(Points_Needed, Question_Count, Per_Id) {

    $(Points_Needed).bind('keyup change', function () {
        var Points_Needed_INT = parseInt($(Points_Needed).val());
        var Percentage = parseFloat((Points_Needed_INT / Question_Count) * 100);
        if (isNaN(Percentage)) {
            Percentage = 0;
            Percentage = Percentage.toString() + " %";
        } else if (Points_Needed_INT > Question_Count) {
            $(Per_Id).addClass("text-danger font-weight-bold");
            Percentage = "Really " + Points_Needed_INT.toString() + " of " + Question_Count.toString() + " !?!?!";
        } else {
            $(Per_Id).removeClass("text-danger font-weight-bold");
            Percentage = Percentage.toFixed(2).toString() + " %";
        }
        $(Per_Id).html(Percentage);
    });
}

var requirements_List = [];
function checkDropDown() {
    var content = "";
    var val_Pres = $('select#pres').val();
    var Presentations = new Array();
    for (var i = 0; i < val_Pres.length; i++) {
        Presentations.push({
            Id: val_Pres[i]
        });
    }
    if (Presentations.length == 0) {
        $("#requirements_Camp").fadeOut('slow');
    }
    var Json = JSON.stringify(Presentations);
    $.ajax({
        type: "POST",
        url: url + "/Check_Requirements/",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        data: Json,
        success: function (result) {
            if (result.length > 0) {
                $("#requirements_Camp").fadeIn('slow');
                $("#Submit_Session").fadeOut('slow');

                content += "<h3>";
                if (result.length > 1) {
                    content += "There're: ";
                    $("#Modal_Title").html("Set requirements needed to pass these trainings");
                    content += result.length + " requirements";
                } else {
                    content += "There's: ";
                    $("#Modal_Title").html("Set requirements needed to pass that training");
                    content += result.length + " requirement";
                }
                content += " missing, each question has 1 point!</h3><hr id='hr'/>";
                content += "<div id='carouselExampleControls' class='carousel slide' data-ride='carousel'><div class='carousel-inner'>";
                for (var i = 0; i < result.length; i++) {

                    var active = "";
                    var Question_Count = "Question_Count_" + i.toString(),
                        Points_Needed = "Points_Needed_" + i.toString(),
                        Percentage = "Percentage_" + i.toString();

                    requirements_List.push({
                        Id_Presentation: result[i].id,
                        Name: result[i].name,
                        Question_Count: Question_Count,
                        Points_Needed: Points_Needed,
                        Percentage: Percentage
                    });

                    if (i == 0) {
                        active = "active";
                    }
                    //content += "";

                    content += "<div class='carousel-item " + active + "'><h3 class='text-info font-weight-bold'>Training: <i>" + result[i].name + "</i></h3><dl class='row'><dt class='col-sm-6 p-2'>";
                    if (result[i].question_Count > 1) {
                        content += "There're: ";
                    } else {
                        content += "There's: ";
                    }
                    content += "</dt><dd class='col-sm-6 p-2'><span id='" + Question_Count + "'>" + result[i].question_Count + "</span>";
                    if (result[i].question_Count > 1) {
                        content += " questions";
                    } else {
                        content += " question";
                    }
                    content += "</dd><dt class='col-sm-6 p-2 mt-1'>Points needed:</dt><dd class='col-sm-6 p-2'><input type='number' id='" + Points_Needed + "' class='form-control w-50 Points_Needed' min='1' max='" + result[i].question_Count + "' value='0' required />";
                    content += "</dd><dt class='col-sm-6 p-2'>Which gives:</dt><dd id='per' class='col-sm-6 p-2'><span id='" + Percentage + "'></span></dd></dl></div>";
                }
                if (result.length > 1) {
                    content += "<a class='carousel-control-prev h-25' href='#carouselExampleControls' role='button' data-slide='prev'><button class='btn btn-sm btn-info font-weight-bold'>Previous</button></a>";
                    content += "<a class='carousel-control-next h-25' href='#carouselExampleControls' role='button' data-slide='next'><button class='btn btn-sm btn-info font-weight-bold'>Next</button></a>";
                }

                content += "</div><div class='text-danger font-weight-bold' id='error_Msg'></div>";
                $("#requirements_Modal_Pages").html(content);
            }
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}


$(document).ready(function () {
    if (controller[2] != "AdminPages") {
        preventBack();
        setTimeout("preventBack()", 0);
        window.onunload = function () { null };
    }
    if (controller[3] == "Edit_Campaign") {
        $("#Submit_Session").fadeIn('fast');
        //check q_count now !!!!!!!!!!!!!!!!!!!!!!!
        checkDropDown();
    }
    if ($("#collapseOne").hasClass('show')) {
        $("#Arrow_1").addClass('Rotate_Arrow');
    }
    //debugger;
    //$('.collapse').on('shown.bs.collapse', function () {

    //    $(this).parent().find('.Arrow_Icon').removeClass('Rotate_Arrow');
    //});
    //debugger;
    //$('.collapse').on('hidden.bs.collapse', function () {

    //    $(this).parent().find('.Arrow_Icon').add('Rotate_Arrow');
    //});

    //4 q_list & camp
    //$("#cancel_Req").click(function () {
    //    $(".Points_Needed").val(0);
    //});

    //debugger;
    //4 create camp % quest list, OPTIMIZE !!!!!!!!!!
    $("#requirements_Submit").on('click', function () {
        var object = [];
        if (controller[3] == "Questions_List") {
            var points = parseInt($("#Points_Needed").val()),
                Question_Count = parseInt($("#Question_Count").html());
            if (points > Question_Count || points == 0) {
                return;
            } else {
                var id = $("#P_Id").val();
                object.push({
                    Id: id,
                    Points_Required: points,
                    Question_Count: Question_Count
                });
            }
        } else {
            var content = "";
            for (var i = 0; i < requirements_List.length; i++) {

                var points = "#" + requirements_List[i].Points_Needed;
                points = parseInt($(points).val());

                var Question_Count = "#" + requirements_List[i].Question_Count;
                Question_Count = parseInt($(Question_Count).html());

                if (points > Question_Count || points == 0) {
                    content += "<i>" + requirements_List[i].Name + ", </i>";
                } else {
                    object.push({
                        Id: requirements_List[i].Id_Presentation,
                        Points_Required: points,
                        Question_Count: Question_Count
                    });
                }
            }
            if (content.length > 0) {
                content = "Please correct requirements for: " + content;
                content.slice(0, -2);
                $("#error_Msg").html(content);
                return;
            }
        }

        var Json = JSON.stringify(object);
        $.ajax({
            type: "POST",
            url: url + "/Set_Requirements/",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            data: Json
        });
        if (controller[3] == "Questions_List") {
            $("#Points_Needed").val(points);
        } else {
            $("#requirements_Modal").modal('hide');
            $("#requirements_Camp").fadeOut('slow');
            $("#Submit_Session").fadeIn('slow');
        }
    });

    //4 Create_Question and edit

    $("#Create_Question").submit(function (event) {
        var checked = $(".form-group input[type='checkbox']").filter(':checked');
        if (checked.length > 0) {
            $("#ErrorMsg").html("");
            return;
        }
        $("#ErrorMsg").html("Please choose at least one correct answer !!");
        $("#myModalLabel").html("Something went wrong !!");
        $("#M_Class").removeClass("modal-dialog modal-dialog-centered modal-notify modal-success modal-lg");
        $("#M_Class").addClass("modal-dialog modal-dialog-centered modal-notify modal-danger modal-lg");
        event.preventDefault();
        if ($("#ErrorMsg:contains(Please choose at least one correct answer !!)").length > 0) {
            $('#exampleModalCenter').modal('show');
        }
    });


    $('select#pres').on('hidden.bs.select', function () {
        checkDropDown();
    });


    $("#requirements_Camp_Button").on('click', function () {
        for (var i = 0; i < requirements_List.length; i++) {

            var Points_Needed = "#" + requirements_List[i].Points_Needed;
            var Per_Id = "#" + requirements_List[i].Percentage;
            var Question_Count = "#" + requirements_List[i].Question_Count;
            Question_Count = parseInt($(Question_Count).html());

            SetPercentage(Points_Needed, Question_Count, Per_Id);

            Points_Needed = parseInt($(Points_Needed).val());
            var Percentage = parseFloat((Points_Needed / Question_Count) * 100).toFixed(2);

            $(Per_Id).html(Percentage.toString() + " %");
        }

        $('#requirements_Modal').modal('show');
        $('.carousel').carousel({
            interval: 9999999
        });
        $('.carousel').carousel('pause');
    });



    //var date_Range = $("#dates").val();
    $("#Submit_Session").on('click', function () {

        var table = $('#Assigned_Users').DataTable();

        var id_Campaign = 0;
        var date_Range = today + " - " + today;
        $("#ErrorMsg").html("");
        var val_Pres = $('select#pres').val();
        var val_Gr = $('select#Gr').val();

        if (controller[3] == "Edit_Campaign") {
            id_Campaign = $('#Id').val();
        }

        if (($("#Name_Session").val() == "") || ($("#Name_Session").val() == null)) {
            NoName('name');
            return;
        } else if (val_Pres.length == 0) {
            NoName('training');
            return;
        } else if ($("#dates").val() == date_Range) {
            NoName('dates');
            return;
        } else if ((!table.data().count()) && val_Gr.length == 0 && Id_list.length == 0) {
            NoName('employees');
            return;
        } else if ($("#Attempts").val() <= 0 || $("#Attempts").val() == null || $("#Attempts").val() == "" || $("#Attempts").val() > 99) {
            NoName('Attempts');
            return;
        } else {
            var name = $("#Name_Session").val();
            date_Range = $("#dates").val();
            var attempts = $("#Attempts").val();
            //val_Pres = $('select#pres').val();
            var Presentations = new Array();
            for (var i = 0; i < val_Pres.length; i++) {
                Presentations.push({
                    Id: val_Pres[i]
                });
            }

            //val_Gr = $('select#Gr').val();
            var Groups = new Array();
            for (var i = 0; i < val_Gr.length; i++) {
                Groups.push({
                    Id_Group: val_Gr[i]
                });
            }

            var object = {
                Id: id_Campaign,
                Name: name,
                Presentations: Presentations,
                Training_Groups: Groups,
                Users_Assigned: Id_list,
                Date_Range: date_Range,
                Attempts: attempts
            }

            var Json = JSON.stringify(object);
            $.ajax({
                type: "POST",
                url: url + "/Create_Training_Campaign/",
                contentType: "application/json;charset=UTF-8",
                dataType: "json",
                data: Json,
                success: function (status) {

                    $("#ErrorMsg").html(status);
                    if (status.includes('already') || status.includes('Please')) {
                        $("#myModalLabel").html("Something went wrong !!");
                        $("#M_Class").removeClass("modal-dialog modal-dialog-centered modal-notify modal-success modal-lg");
                        $("#M_Class").addClass("modal-dialog modal-dialog-centered modal-notify modal-danger modal-lg");
                    } else {
                        $("#myModalLabel").html("Well done !!");
                        $("#M_Class").removeClass("modal-dialog modal-dialog-centered modal-notify modal-danger modal-lg");
                        $("#M_Class").addClass("modal-dialog modal-dialog-centered modal-notify modal-success modal-lg");

                        if (controller[3] == "Edit_Campaign") {
                            table.ajax.reload();
                            BigTable();
                        } else {
                            $("#Attempts").val('');
                            $("#Name_Session").val('');
                            $("#pres").val('');
                            $("#Gr").val('');
                            $("#dates").val(today + ' - ' + today);
                            $('#dates').data('daterangepicker').setStartDate(today);
                            $('#dates').data('daterangepicker').setEndDate(today);
                            $('.selectpicker').selectpicker('deselectAll');
                            $('.selectpicker').selectpicker('val', '');
                            $("input").prop('checked', false);
                            //  $("#Submit_Session").fadeOut('slow');
                        }
                        Id_list = [];
                    }
                    if ($("#ErrorMsg:contains(!!)").length > 0) {
                        $('#exampleModalCenter').modal('show');
                    }

                }
            });
            // Id_list = [];
        }
    });
    //4 modal delete
    $('#myModal').on('hidden.bs.modal', function () {
        $("#myModal p").html("");
    });
    //$('#Assign_Table').on('hidden.bs.modal', function () {
    //    Id_list = [];
    //});
    //4 Question_List
    $("#MaterialToggleBlue").on('click', function () {
        var status = false;
        if ($("#MaterialToggleBlue").prop('checked') == true) {
            status = true;
        }
        var id = $("#P_Id").val();
        var object = {
            Id: id,
            Shuffle_Questions: status
        }
        var Json = JSON.stringify(object);
        $.ajax({
            type: "POST",
            url: url + "/Shuffle_Questions/",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            data: Json,
        });
    });
    //4 create_Group page finish it !!!!!!!!!!!!!!!!!!
    $("#Submit_Employees").on('click', function () {
        $("#ErrorMsg").html("");

        if (($("#Name").val() == "") || ($("#Name").val() == null)) {
            NoName();
            return;
        } else {
            var name = $("#Name").val();
            var object = {
                Name: name,
                Users: Id_list
            }
            var Json = JSON.stringify(object);
            $.ajax({
                type: "POST",
                url: url + "/Create_Group/",
                contentType: "application/json;charset=UTF-8",
                dataType: "json",
                data: Json,
                success: function (status) {
                    $("#ErrorMsg").html(status);
                    if (status.includes('already') || status.includes('Please')) {
                        $("#myModalLabel").html("Something went wrong !!");
                        $("#M_Class").removeClass("modal-dialog modal-dialog-centered modal-notify modal-success modal-lg");
                        $("#M_Class").addClass("modal-dialog modal-dialog-centered modal-notify modal-danger modal-lg");
                    } else {
                        $("#myModalLabel").html("Well done !!");
                        $("#M_Class").removeClass("modal-dialog modal-dialog-centered modal-notify modal-danger modal-lg");
                        $("#M_Class").addClass("modal-dialog modal-dialog-centered modal-notify modal-success modal-lg");
                        $("#Name").val('');
                        $("input").prop('checked', false);
                        Id_list = [];
                    }
                    if ($("#ErrorMsg:contains(!!)").length > 0) {
                        $('#exampleModalCenter').modal('show');
                    }
                }
            })
        }
    });

    //$("#Assign_Employees").on('click', function () {
    //    $('#Assign_Table').modal('show');
    //});

    //4 edit page
    new WOW().init();

    $("#New_File_Name_Button").on('click', function () {
        $("#New_File_Name").toggle();
        $("#New_File_Name").val('');
    });

    if ($("#No_Files").length) {
        $("#Select_Type").prop('disabled', false);
        $("#Select_Type option[value='']").attr('selected', 'selected');
        if ($("#Select_Type option[value='']").is(':selected') == true) {
            $("#customFileLang").attr('disabled', true);
        }
    }
    //4 edit page & create_Training
    $("#Select_Type").on('change', function () {
        $("#customFileLang").val('');
        $("#FileLabel").html('Choose files');
        if ($("#Select_Type option[value='jpg']").is(':selected') == true) {
            $("#customFileLang").attr('multiple', true);
        } else {
            $("#customFileLang").attr('multiple', false);
        }
        if ($("#Select_Type option[value='']").is(':selected') == true) {
            $("#customFileLang").attr('disabled', true);
        } else {
            $("#customFileLang").attr('disabled', false);
        }
    });
    //4 edit quest
    $("#Answers_2__Answer").keyup(function () {
        var textArea = document.getElementById("Answers_2__Answer");
        var checkBox = document.getElementById('Answers_2__Correct_Answer');

        if (textArea.value.length == 0) {
            checkBox.checked = false;
            checkBox.disabled = true;
            $("#containerCheck3").hide("slow");
        } else {
            checkBox.disabled = false;
            $("#containerCheck3").show("slow");
        }
    });

    $("#Answers_3__Answer").keyup(function () {
        var textArea = document.getElementById("Answers_3__Answer");
        var checkBox = document.getElementById('Answers_3__Correct_Answer');

        if (textArea.value.length == 0) {
            checkBox.checked = false;
            checkBox.disabled = true;
            $("#containerCheck4").hide("slow");

        } else {
            checkBox.disabled = false;
            $("#containerCheck4").show("slow");
        }
    });

    $("#AddAnswer").click(function () {
        if ($("#a3").length) {
            if (document.getElementById("a3").hidden == false) {
                $("#a4").show();
                document.getElementById("a4").hidden = false;
                $("#Answer_4").html('');

            } else {
                $("#a3").show();
                document.getElementById("a3").hidden = false;
                $("#Answer_3").html('');
            }
        } else {
            if ($("#Answers_2__Answer").length) {
                $("#a4").show();
                document.getElementById("a4").hidden = false;
                $("#Answer_4").html('');
            }
        }
    });

    $("#AddAnswer2").click(function () {
        if ($("#a3").length) {
            if (document.getElementById("a3").hidden == false) {
                $("#a4").show('slow');
                document.getElementById("a4").hidden = false;
                $("#Answer_4").html('');

            } else {
                $("#a3").show('slow');
                document.getElementById("a3").hidden = false;
                $("#Answer_3").html('');
            }
        } else {
            if ($("#Answers_2__Answer").length) {
                $("#a4").show('slow');
                document.getElementById("a4").hidden = false;
                $("#Answer_4").html('');
            }
        }
    });


    //4 quest list


    //Unify the messages !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 

    //if (($("#ErrorMsg:contains(Exception)").length > 0) || ($("#ErrorMsg:contains(Sorry)").length > 0) || ($("#ErrorMsg:contains(deleted)").length > 0) || ($("#ErrorMsg:contains(Training)").length > 0)
    //    || ($("#ErrorMsg:contains(Please)").length > 0) || ($("#ErrorMsg:contains(exists)").length > 0) || ($("#ErrorMsg:contains(please)").length > 0) || ($("#ErrorMsg:contains(one)").length > 0)) {
    //    $('#exampleModalCenter').modal('show');
    //}

    if (($("#ErrorMsg:contains(Exception)").length > 0) || ($("#ErrorMsg:contains(Training)").length > 0) || ($("#ErrorMsg:contains(!!)").length > 0)) {
        $('#exampleModalCenter').modal('show');
    }
    if (($("#ErrorMsg:contains(added)").length > 0)) {
        $('#exampleModalCenter').modal('show');
        $(".form-control").val('');
    }
    //if (($("#ErrorMsg:contains(Sorry)").length > 0)) {
    //    $('#exampleModalCenter').modal('show');
    //}

    //if (($("#ErrorMsg:contains(deleted)").length > 0)) {
    //    $('#exampleModalCenter').modal('show');
    //}

    //if (($("#ErrorMsg:contains(Training)").length > 0)) {
    //    $('#exampleModalCenter').modal('show');
    //    //$(".form-control").val(''); // in Edit_Training creates an error, check other pages !!!!!!!!!!!!!!!!!!!
    //}

    //if (($("#ErrorMsg:contains(Please)").length > 0)) {
    //    $('#exampleModalCenter').modal('show');
    //    //$(".form-control").val('');
    //}

    //if (($("#ErrorMsg:contains(exists)").length > 0)) {
    //    $('#exampleModalCenter').modal('show');
    //    //$(".form-control").val('');
    //}


    //if (($("#ErrorMsg:contains(please)").length > 0)) {
    //    $('#exampleModalCenter').modal('show');
    //}
    //if (($("#ErrorMsg:contains(one)").length > 0)) {
    //    $('#exampleModalCenter').modal('show');
    //}

    $("#customFileLang").on("change", function () {
        var fileLabel = $("#FileLabel");
        var files = $(this)[0].files;
        if (files.length > 1) {
            fileLabel.html(files.length + " files selected");
        }
        else if (files.length == 1) {
            fileLabel.html(files[0].name);
        }
    });
});
