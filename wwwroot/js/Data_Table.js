//debugger;

var controller = document.location.pathname.split("/");
var url = controller[0] + "/" + controller[1] + "/" + controller[2];
var Id = $("#Id_Group").val();

//4 create session
var val_Gr = new Array();
if (controller[3] == "Create_Training_Campaign" || controller[3] == "Edit_Campaign") {
    val_Gr = $('select#Gr').val();
}
//var Additional_Employees = new Array();
$("#dismiss").on('click', function () {
    Id_list = [];
});

var method = "/Assigned_Employees_Partial/";
if (controller[3] == "Edit_Campaign") {
    Id = $('#Id').val();
    method = "/Additional_Employees_Partial/";
}

//4 Edit_Group
$("#Add_New_Employees").on('click', function () {
    var Object = {
        Id_Group: Id,
        Users: Id_list
    };
    var Json = JSON.stringify(Object);
    $.ajax({
        type: "POST",
        url: url + "/Add_New_Employees/",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        data: Json
    });
    Id_list = [];

    var table = $('#Assigned_Users').DataTable();
    $('#Assigned_Users').DataTable().ajax.reload();
    $('#Assigned_Users').DataTable().draw();
    table.ajax.reload();
    $('#Assigned_Users').DataTable().ajax.reload();
    $('#Assigned_Users').DataTable().draw();
    table.ajax.reload();
    $('#Assign_Table').modal('hide');
    $('#Assign_Table').on('hidden.bs.modal', function () {
        $('#Assigned_Users').DataTable().ajax.reload();
        table.ajax.reload();
    });
    table.ajax.reload();
    BigTable();
    table.ajax.reload();
    $('#Assign_Table').on('hidden.bs.modal', function () {
        $('#Assigned_Users').DataTable().ajax.reload();
        table.ajax.reload();
    });
});
//4 Edit_Group
function DeleteEmployee(ID) {
    var Object = {
        Id_Group: Id,
        Users: [
            { Id_User: ID }
        ]
    };
    var Json = JSON.stringify(Object);
    $.ajax({
        url: url + "/Delete_Employee/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        data: Json
    });
    //try with functions bigTable
    $('#Assigned_Users').DataTable().ajax.reload();
    $('#Assigned_Users').DataTable().draw();
    var table = $('#Assigned_Users').DataTable()

    $('#Assigned_Users').DataTable().ajax.reload();
    $('#Assigned_Users').DataTable().draw();
    table.ajax.reload();

    table.ajax.reload();
    BigTable();
    table.ajax.reload();
}
//debugger;
function BigTable() {
    if (Id_list.length > 0) {
        $('#Assign_Table').modal('show');
    } else {
        var table = $('#dtBasicExample').DataTable();
        table.destroy();
        var Groups = new Array();
        if (val_Gr.length > 0) {
            for (var i = 0; i < val_Gr.length; i++) {
                Groups.push({
                    Id_Group: val_Gr[i]
                });
            }
        } else {
            Groups.push({
                Id_Group: Id
            });
        }
        var Id_Campaign = $('#Id').val();
        var object = {
            Id: Id_Campaign,
            Training_Groups: Groups,
        }
        var Json = JSON.stringify(object);
        $.ajax({
            type: "POST",
            url: url + '/GetPartial2/',
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            data: Json,
            success: function (dataSet) {
                var return_data = new Array();
                for (var i = 0; i < dataSet.length; i++) {
                    return_data.push({
                        'Personal_Number': dataSet[i].personal_Number,
                        'Branch': dataSet[i].info_Group,
                        'First_Name': dataSet[i].first_Name,
                        'Last_Name': dataSet[i].last_Name,
                        'Assigned': "<input type='checkbox' id='Check_Assigned' class='form-control' onclick='Assign(" + dataSet[i].id_User + ");'value='" + dataSet[i].assigned + "'/>"
                    })
                }
                //creating new table
                $('#dtBasicExample').DataTable({
                    data: return_data,
                    columns: [
                        { 'data': 'Personal_Number' },
                        { 'data': 'Branch' },
                        { 'data': 'First_Name' },
                        { 'data': 'Last_Name' },
                        { 'data': 'Assigned' },
                    ],
                    "columnDefs": [
                        { "orderable": false, "targets": 3 },
                        { "searchable": true, "visible": false, "targets": 1 }
                    ]
                });
            }
        });
    }
}

function AssignedEmployees() {
    //4 Edit_Group
    var table = $('#Assigned_Users').DataTable();
    table.destroy();


    $('#Assigned_Users').DataTable({
        'ajax': {
            "type": "GET",
            "url": url + method + Id,
            "dataSrc": function (data) {
                var return_data = new Array();
                //4 edit_Camp
                var Groups_Content = "";
                for (var i = 0; i < data.length; i++) {
                    if (controller[3] == "Edit_Campaign") {
                        if (data[i].info_Group == "Additional") {
                            return_data.push({
                                'Personal_Number': data[i].personal_Number,
                                'First_Name': data[i].first_Name,
                                'Last_Name': data[i].last_Name,
                                'Delete': "<input type='button' id='Delete_Employee' class='btn btn-danger btn-sm font-weight-bold' value='Delete' onclick='DeleteEmployeeFromCampaign(" + data[i].id_User + ");'/>"
                            })
                        } else {
                            for (var j = 0; j < data[i].groups_In.length; j++) {
                                if (data[i].groups_In[j].name == "Additional") {
                                    Groups_Content += "<span>" + data[i].groups_In[j].name + "</span><input type ='button' id='Delete_Employee' class='btn btn-danger btn-sm font-weight-bold' value='Delete' onclick ='DeleteEmployeeFromCampaign(" + data[i].id_User + ");'/>";
                                } else {
                                    Groups_Content += "<a class='Deco_Link' href='" + url + "/Edit_Group/" + data[i].groups_In[j].id_Group + "'><span>" + data[i].groups_In[j].name + "</span></a>, ";
                                }
                            }
                            return_data.push({
                                'Personal_Number': data[i].personal_Number,
                                'First_Name': data[i].first_Name,
                                'Last_Name': data[i].last_Name,
                                'Delete': Groups_Content
                            })
                            Groups_Content = "";
                        }
                    } else {
                        return_data.push({
                            'Personal_Number': data[i].personal_Number,
                            'First_Name': data[i].first_Name,
                            'Last_Name': data[i].last_Name,
                            'Delete': "<input type='button' id='Delete_Employee' class='btn btn-danger btn-sm font-weight-bold' value='Delete' onclick='DeleteEmployee(" + data[i].id_User + ");'/>"
                        })
                    }
                }
                return return_data;
            }
        },
        "columns": [
            { 'data': 'Personal_Number' },
            { 'data': 'First_Name' },
            { 'data': 'Last_Name' },
            { 'data': 'Delete' },
        ],
        "columnDefs": [
            { "orderable": false, "targets": 3 }
        ]
    });
}
//4 edit camp
function DeleteEmployeeFromCampaign(id) {
    var Object = {
        Id: Id,
        Users_Assigned: [
            { Id_User: id }
        ]
    };
    var Json = JSON.stringify(Object);
    $.ajax({
        url: url + "/Delete_Employee_From_Campaign/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        data: Json
    });
    var table = $('#Assigned_Users').DataTable();
    table.ajax.reload();
    table.ajax.reload();
    BigTable();
    table.ajax.reload();
    //AssignedEmployees();
}
//4 edit camp
$("#Additional_Employees").on('click', function () {
    var Object = {
        Id: Id,
        Users_Assigned: Id_list
    };
    var Json = JSON.stringify(Object);
    $.ajax({
        type: "POST",
        url: url + "/Add_New_Employee_For_Campaign/",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        data: Json
    });
    Id_list = [];

    var table = $('#Assigned_Users').DataTable()
    $('#Assigned_Users').DataTable().ajax.reload();
    $('#Assigned_Users').DataTable().draw();
    table.ajax.reload();
    $('#Assigned_Users').DataTable().ajax.reload();
    $('#Assigned_Users').DataTable().draw();
    table.ajax.reload();

    $('#Assign_Table').modal('hide');
    BigTable();
    table.ajax.reload();
});
$(document).ready(function () {



    $('select#Gr').on('change', function () {
        val_Gr = $('select#Gr').val();
    });

    //4 create_Group 4 create session
    $("#Assign_Employees").on('click', function () {
        BigTable();
        $('#Assign_Table').modal('show');
    });
    if (controller[3] == "Create_Group") {
        BigTable();
    }

    AssignedEmployees();
    //4 Edit_Group
    //$('#Assigned_Users').DataTable({
    //    'ajax': {
    //        "type": "GET",
    //        "url": url + method + Id,
    //        "dataSrc": function (data) {
    //            var return_data = new Array();
    //            //4 edit_Camp
    //            var Groups_Content = "";
    //            for (var i = 0; i < data.length; i++) {
    //                if (controller[3] == "Edit_Campaign") {
    //                    if (data[i].info_Group == "Additional") {
    //                        return_data.push({
    //                            'Personal_Number': data[i].personal_Number,
    //                            'First_Name': data[i].first_Name,
    //                            'Last_Name': data[i].last_Name,
    //                            'Delete': "<input type='button' id='Delete_Employee' class='btn btn-danger btn-sm' value='Delete' onclick='DeleteEmployeeFromCampaign(" + data[i].id_User + ");'/>"
    //                        })
    //                    } else {
    //                        for (var j = 0; j < data[i].groups_In.length; j++) {
    //                            Groups_Content += "<a class='Deco_Link' href='" + url + "/Edit_Group/" + data[i].groups_In[j].id_Group + "'><span>" + data[i].groups_In[j].name + "</span></a>, ";
    //                        }
    //                        return_data.push({
    //                            'Personal_Number': data[i].personal_Number,
    //                            'First_Name': data[i].first_Name,
    //                            'Last_Name': data[i].last_Name,
    //                            'Delete': Groups_Content
    //                        })
    //                        Groups_Content = "";
    //                    }
    //                } else {
    //                    return_data.push({
    //                        'Personal_Number': data[i].personal_Number,
    //                        'First_Name': data[i].first_Name,
    //                        'Last_Name': data[i].last_Name,
    //                        'Delete': "<input type='button' id='Delete_Employee' class='btn btn-danger btn-sm' value='Delete' onclick='DeleteEmployee(" + data[i].id_User + ");'/>"
    //                    })
    //                }
    //            }
    //            return return_data;
    //        }
    //    },
    //    "columns": [
    //        { 'data': 'Personal_Number' },
    //        { 'data': 'First_Name' },
    //        { 'data': 'Last_Name' },
    //        { 'data': 'Delete' },
    //    ],
    //    "columnDefs": [
    //        { "orderable": false, "targets": 3 }
    //    ]
    //});
});

