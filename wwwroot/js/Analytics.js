new WOW().init();
var chart_Line;

var id_Training_Old;

var today = new Date();
var dd = String(today.getDate()).padStart(2, '0');
var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
var yyyy = today.getFullYear();

today = dd + '.' + mm + '.' + yyyy;

function Points_Chart_Line(user, max, PresentationName) {
    //chart_Line.dispose();
    chart_Line = am4core.create("chartdiv", am4charts.XYChart);
    chart_Line.scrollbarX = new am4core.Scrollbar();
    chart_Line.exporting.menu = new am4core.ExportMenu();
    chart_Line.exporting.menu.align = "left";
    chart_Line.exporting.menu.verticalAlign = "top";
    chart_Line.exporting.backgroundColor = am4core.color("#636363");
    chart_Line.exporting.dataFields = {
        "PersonalNumber": "Personal Number",
        "FullName": "Name",
        "Branch": "Branch",
        "Date": "Date",
        "Result": "Result"
    }
    chart_Line.exporting.filePrefix = "Training_Report_For_" + PresentationName + "_" + today;

    // Add data               
    chart_Line.data = user;

    // Create axes
    var categoryAxis = chart_Line.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "Name";
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.minGridDistance = 30;
    categoryAxis.renderer.labels.template.horizontalCenter = "right";
    categoryAxis.renderer.labels.template.verticalCenter = "middle";
    categoryAxis.renderer.labels.template.rotation = 270;
    categoryAxis.tooltip.disabled = true;
    categoryAxis.renderer.minHeight = 110;

    var valueAxis = chart_Line.yAxes.push(new am4charts.ValueAxis());
    valueAxis.renderer.minWidth = 50;
    valueAxis.min = 0;
    valueAxis.max = max + parseInt(max / 4) + 1;

    // Create series
    var series = chart_Line.series.push(new am4charts.ColumnSeries());
    series.sequencedInterpolation = true;
    series.dataFields.valueY = "Points";
    series.dataFields.categoryX = "Name";
    series.tooltipText = "[{categoryX}: bold]{valueY}[/]";
    series.columns.template.strokeWidth = 0;


    series.tooltip.pointerOrientation = "vertical";

    series.columns.template.column.cornerRadiusTopLeft = 10;
    series.columns.template.column.cornerRadiusTopRight = 10;
    series.columns.template.column.fillOpacity = 0.8;

    // on hover, make corner radiuses bigger
    var hoverState = series.columns.template.column.states.create("hover");
    hoverState.properties.cornerRadiusTopLeft = 0;
    hoverState.properties.cornerRadiusTopRight = 0;
    hoverState.properties.fillOpacity = 1;

    series.columns.template.adapter.add("fill", function (fill, target) {
        return chart_Line.colors.getIndex(target.dataItem.index);
    });

    // Cursor
    chart_Line.cursor = new am4charts.XYCursor();
    $("g[stroke*='#3cabff']").hide();
    $("g[stroke*='url']").hide();
    debugger;
    var result_Name = $('select#Results').val();
    if (result_Name == "") {
        result_Name = 1;
    }
    $('#Results').val(result_Name).change();
}

function PushObject(obj, Array) {
    if (obj.percent > 0) {
        Array.push(obj);
    } else {
        return;
    }
}

function calculate(a, b) {
    var perc = 0;
    if (isNaN(a) || isNaN(b)) {
        perc = 0;
    } else {
        perc = ((b / a) * 100).toFixed(3);
    }
    return perc;
}


function PieChart(users,name) {

    var PassUsers = [];
    var FailUsers = [];
    var NotAttempted = []
    for (var i = 0; i < users.length; i++) {
        if (users[i].email == "---") {
            NotAttempted.push(users[i]);
        } else if (users[i].email == "PASS") {
            PassUsers.push(users[i]);
        } else {
            FailUsers.push(users[i]);
        }
    }


    chart_Line2 = am4core.create("Export", am4charts.XYChart);
    chart_Line2.data = users;
    chart_Line2.scrollbarX = new am4core.Scrollbar();
    chart_Line2.exporting.menu = new am4core.ExportMenu();
    chart_Line2.exporting.menu.align = "right";
    chart_Line2.exporting.menu.verticalAlign = "top";
    chart_Line2.exporting.backgroundColor = am4core.color("#636363");
    chart_Line2.exporting.dataFields = {
        "personal_Number": "Personal Number",
        "last_Name": "Name",
        "info_Login": "Date",
        "email": "Result"
    }
    chart_Line2.exporting.filePrefix = "Training_Participants_Report_For_" + name + "_" + today;

    // Add data               
    //chart_Line2.data = users;

    var chart = am4core.create("Pie_chartdiv", am4charts.PieChart);

    // Add data
    chart.data = [{
        "category": "Failed",
        "value": calculate(users.length, FailUsers.length),
        "color": am4core.color("#f44336")
    }, {
        "category": "Passed",
        "value": calculate(users.length, PassUsers.length),
        "color": am4core.color("#00C851")
    }, {
        "category": "Not Attempted",
        "value": calculate(users.length, NotAttempted.length),
        "color": am4core.color("#b388ff")
    }];


    // Add and configure Series
    var chart_Pie = chart.series.push(new am4charts.PieSeries());
    chart_Pie.dataFields.value = "value";
    chart_Pie.dataFields.category = "category";
    chart_Pie.slices.template.stroke = am4core.color("#fff");
    chart_Pie.slices.template.strokeWidth = 2;
    chart_Pie.slices.template.strokeOpacity = 1;
    chart_Pie.slices.template.propertyFields.fill = "color";

    chart_Pie.exporting.filePrefix = "Training_Participation_Report_For_" + /*PresentationName*/ + "_" + today;

    // This creates initial animation
    chart_Pie.hiddenState.properties.opacity = 1;
    chart_Pie.hiddenState.properties.endAngle = -90;
    chart_Pie.hiddenState.properties.startAngle = -90;

    $("#PieChartContainer").show();
    $("g[stroke*='#3cabff']").hide();
    $("g[stroke*='url']").hide();
}



function Trainings_Results(id_Training_Old) {
    var id_Camp = $('select#Camps').val();
    var id_Training = $('select#Trainings').val();
    var result_Name = $('select#Results').val();

    if ((id_Training_Old != 0 && id_Training_Old != id_Training) || result_Name == "") {
        result_Name = 1;
    }

    if (id_Training != 0) {

        $('#Results').val(result_Name).change();

        var Presentations = new Array();
        Presentations.push({
            Id: id_Training,
            Type: result_Name
        });

        var object = {
            Id: id_Camp,
            Presentations: Presentations
        }

        var Json = JSON.stringify(object);
        $.ajax({
            type: "POST",
            url: url + "/Trainings_Chart/",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            data: Json,
            success: function (result) {

                var user = new Array();
                var max = 0;

                for (var i = 0; i < result[0].users_Assigned.length; i++) {

                    if (max == null || parseInt(result[0].users_Assigned[i].sum_of_Points) > parseInt(max)) {
                        max = result[0].users_Assigned[i].sum_of_Points;
                    }

                    user.push({
                        Id: result[0].users_Assigned[i].id_User,
                        PersonalNumber: result[0].users_Assigned[i].personal_Number,
                        Name: result[0].users_Assigned[i].first_Name,
                        FullName: result[0].users_Assigned[i].last_Name,
                        Points: result[0].users_Assigned[i].sum_of_Points,
                        Date: result[0].users_Assigned[i].info_Login,
                        Result: result[0].users_Assigned[i].email,
                        Branch: result[0].users_Assigned[i].info_Group,
                    })
                }
                let PresentationName = result[0].name;
                Points_Chart_Line(user, max, PresentationName);
            }
        });
    }
}

function AllParticipants(id_Camp, id_Training,name) {

    if (id_Training != 0 && id_Training != "" && id_Training != null) {

        var Presentations = new Array();
        Presentations.push({
            Id: id_Training
        });

        var object = {
            Id: id_Camp,
            Presentations: Presentations
        }
        var Json = JSON.stringify(object);
        $.ajax({
            type: "POST",
            url: url + "/AllParticipants/",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            data: Json,
            success: function (result) {
                return PieChart(result, name);
            }
        });
    }
}

am4core.ready(function () {

    // Themes begin
    am4core.useTheme(am4themes_dark);
    am4core.useTheme(am4themes_animated);
    // Themes end
    //debugger;


    //// Create chart instance
    //chart_Line = am4core.create("chartdiv", am4charts.XYChart);
    //chart_Line.scrollbarX = new am4core.Scrollbar();

    //// Add data
    //chart_Line.data = [{
    //    "country": "Example1",
    //    "visits": 20
    //}, {
    //    "country": "Example2",
    //    "visits": 18
    //}, {
    //    "country": "Example3",
    //    "visits": 18
    //}, {
    //    "country": "Example4",
    //    "visits": 13
    //}, {
    //    "country": "Example13",
    //    "visits": 11
    //}, {
    //    "country": "Example5",
    //    "visits": 11
    //}, {
    //    "country": "Example6",
    //    "visits": 9
    //}, {
    //    "country": "Example7",
    //    "visits": 7
    //}, {
    //    "country": "Example8",
    //    "visits": 6
    //}, {
    //    "country": "Example9",
    //    "visits": 5
    //}, {
    //    "country": "Example10",
    //    "visits": 4
    //}, {
    //    "country": "Example11",
    //    "visits": 4
    //}, {
    //    "country": "Example12",
    //    "visits": 2
    //}];

    //// Create axes
    //var categoryAxis = chart_Line.xAxes.push(new am4charts.CategoryAxis());
    //categoryAxis.dataFields.category = "country";
    //categoryAxis.renderer.grid.template.location = 0;
    //categoryAxis.renderer.minGridDistance = 30;
    //categoryAxis.renderer.labels.template.horizontalCenter = "right";
    //categoryAxis.renderer.labels.template.verticalCenter = "middle";
    //categoryAxis.renderer.labels.template.rotation = 270;
    //categoryAxis.tooltip.disabled = true;
    //categoryAxis.renderer.minHeight = 110;

    //var valueAxis = chart_Line.yAxes.push(new am4charts.ValueAxis());
    //valueAxis.renderer.minWidth = 50;

    //// Create series
    //var series = chart_Line.series.push(new am4charts.ColumnSeries());
    //series.sequencedInterpolation = true;
    //series.dataFields.valueY = "visits";
    //series.dataFields.categoryX = "country";
    //series.tooltipText = "[{categoryX}: bold]{valueY}[/]";
    //series.columns.template.strokeWidth = 0;

    //series.tooltip.pointerOrientation = "vertical";

    //series.columns.template.column.cornerRadiusTopLeft = 10;
    //series.columns.template.column.cornerRadiusTopRight = 10;
    //series.columns.template.column.fillOpacity = 0.8;

    //// on hover, make corner radiuses bigger
    //var hoverState = series.columns.template.column.states.create("hover");
    //hoverState.properties.cornerRadiusTopLeft = 0;
    //hoverState.properties.cornerRadiusTopRight = 0;
    //hoverState.properties.fillOpacity = 1;

    //series.columns.template.adapter.add("fill", function (fill, target) {
    //    return chart_Line.colors.getIndex(target.dataItem.index);
    //});

    //// Cursor
    //chart_Line.cursor = new am4charts.XYCursor();

    // Pie chart
    // Create chart instance









    //// Create chart
    //var chart_Stock = am4core.create("Stock_chartdiv", am4charts.XYChart);
    //chart_Stock.padding(0, 15, 0, 15);
    //chart_Stock.colors.step = 3;

    //// the following line makes value axes to be arranged vertically.
    //chart_Stock.leftAxesContainer.layout = "vertical";

    //// uncomment this line if you want to change order of axes
    ////chart.bottomAxesContainer.reverseOrder = true;

    //var dateAxis = chart_Stock.xAxes.push(new am4charts.DateAxis());
    //dateAxis.renderer.grid.template.location = 0;
    //dateAxis.renderer.ticks.template.length = 8;
    //dateAxis.renderer.ticks.template.strokeOpacity = 0.1;
    //dateAxis.renderer.grid.template.disabled = true;
    //dateAxis.renderer.ticks.template.disabled = false;
    //dateAxis.renderer.ticks.template.strokeOpacity = 0.2;
    //dateAxis.renderer.minLabelPosition = 0.01;
    //dateAxis.renderer.maxLabelPosition = 0.99;
    //dateAxis.minHeight = 30;

    //dateAxis.groupData = true;
    //dateAxis.minZoomCount = 5;

    //// these two lines makes the axis to be initially zoomed-in
    //// dateAxis.start = 0.7;
    //// dateAxis.keepSelection = true;

    //var valueAxis = chart_Stock.yAxes.push(new am4charts.ValueAxis());
    //valueAxis.tooltip.disabled = true;
    //valueAxis.zIndex = 1;
    //valueAxis.renderer.baseGrid.disabled = true;
    //// height of axis
    //valueAxis.height = am4core.percent(65);

    //valueAxis.renderer.gridContainer.background.fill = am4core.color("#000000");
    //valueAxis.renderer.gridContainer.background.fillOpacity = 0.05;
    //valueAxis.renderer.inside = true;
    //valueAxis.renderer.labels.template.verticalCenter = "bottom";
    //valueAxis.renderer.labels.template.padding(2, 2, 2, 2);

    ////valueAxis.renderer.maxLabelPosition = 0.95;
    //valueAxis.renderer.fontSize = "0.8em"

    //var series1 = chart_Stock.series.push(new am4charts.LineSeries());
    //series1.defaultState.transitionDuration = 0;
    //series1.dataFields.dateX = "Date";
    //series1.dataFields.valueY = "Adj Close";
    //series1.dataFields.valueYShow = "changePercent";
    //series1.tooltipText = "{name}: {valueY.changePercent.formatNumber('[#0c0]+#.00|[#c00]#.##|0')}%";
    //series1.name = "MSFT";
    //series1.tooltip.getFillFromObject = false;
    //series1.tooltip.getStrokeFromObject = true;
    //series1.tooltip.background.fill = am4core.color("#fff");
    //series1.tooltip.background.strokeWidth = 2;
    //series1.tooltip.label.fill = series1.stroke;

    //series1.dataSource.url = "https://www.amcharts.com/wp-content/uploads/assets/stock/MSFT.csv";
    //series1.dataSource.parser = new am4core.CSVParser();
    //series1.dataSource.parser.options.useColumnNames = true;
    //series1.dataSource.parser.options.reverse = true;

    //var series2 = chart_Stock.series.push(new am4charts.LineSeries());
    //series2.dataFields.dateX = "Date";
    //series2.dataFields.valueY = "Adj Close";
    //series2.dataFields.valueYShow = "changePercent";
    //series2.tooltipText = "{name}: {valueY.changePercent.formatNumber('[#0c0]+#.00|[#c00]#.##|0')}%";
    //series2.name = "TXN";
    //series2.tooltip.getFillFromObject = false;
    //series2.tooltip.getStrokeFromObject = true;
    //series2.tooltip.background.fill = am4core.color("#fff");
    //series2.tooltip.background.strokeWidth = 2;
    //series2.tooltip.label.fill = series2.stroke;

    //series2.dataSource.url = "https://www.amcharts.com/wp-content/uploads/assets/stock/TXN.csv";
    //series2.dataSource.parser = new am4core.CSVParser();
    //series2.dataSource.parser.options.useColumnNames = true;
    //series2.dataSource.parser.options.reverse = true;

    //var valueAxis2 = chart_Stock.yAxes.push(new am4charts.ValueAxis());
    //valueAxis2.tooltip.disabled = true;
    //// height of axis
    //valueAxis2.height = am4core.percent(35);
    //valueAxis2.zIndex = 3
    //// this makes gap between panels
    //valueAxis2.marginTop = 30;
    //valueAxis2.renderer.baseGrid.disabled = true;
    //valueAxis2.renderer.inside = true;
    //valueAxis2.renderer.labels.template.verticalCenter = "bottom";
    //valueAxis2.renderer.labels.template.padding(2, 2, 2, 2);
    ////valueAxis.renderer.maxLabelPosition = 0.95;
    //valueAxis2.renderer.fontSize = "0.8em";

    //valueAxis2.renderer.gridContainer.background.fill = am4core.color("#000000");
    //valueAxis2.renderer.gridContainer.background.fillOpacity = 0.05;

    //var volumeSeries = chart_Stock.series.push(new am4charts.ColumnSeries());
    //volumeSeries.defaultState.transitionDuration = 0
    //volumeSeries.fill = series1.stroke;
    //volumeSeries.stroke = series1.stroke;
    //volumeSeries.dataFields.dateX = "Date";
    //volumeSeries.dataFields.valueY = "Volume";
    //volumeSeries.yAxis = valueAxis2;
    //volumeSeries.tooltipText = "{name} Volume: {valueY.value}";
    //volumeSeries.name = "MSFT";
    //// volume should be summed
    //volumeSeries.groupFields.valueY = "sum";
    //volumeSeries.tooltip.label.fill = volumeSeries.stroke;

    //volumeSeries.dataSource.url = "https://www.amcharts.com/wp-content/uploads/assets/stock/MSFT.csv";
    //volumeSeries.dataSource.parser = new am4core.CSVParser();
    //volumeSeries.dataSource.parser.options.useColumnNames = true;
    //volumeSeries.dataSource.parser.options.reverse = true;

    //// Cursor
    //chart_Stock.cursor = new am4charts.XYCursor();

    //// Scrollbar
    //var scrollbarX = new am4charts.XYChartScrollbar();
    //scrollbarX.series.push(series1);
    //scrollbarX.marginBottom = 20;
    //chart_Stock.scrollbarX = scrollbarX;
    //scrollbarX.scrollbarChart.xAxes.getIndex(0).minHeight = undefined;


    ///**
    // * Set up external controls
    // */

    //// Date format to be used in input fields
    //var inputFieldFormat = "yyyy-MM-dd";

    //document.getElementById("b1m").addEventListener("click", function () {
    //    resetButtonClass();
    //    var max = dateAxis.groupMax["day1"];
    //    var date = new Date(max);
    //    date.setMonth(date.getMonth() - 1);

    //    dateAxis.zoomToDates(
    //        date,
    //        new Date(max)
    //    );
    //    //this.className = "amcharts-input amcharts-input-selected";
    //});

    //document.getElementById("b3m").addEventListener("click", function () {
    //    resetButtonClass();
    //    var max = dateAxis.groupMax["day1"];
    //    var date = new Date(max);
    //    date.setMonth(date.getMonth() - 3);

    //    dateAxis.zoomToDates(
    //        date,
    //        new Date(max)
    //    );
    //    //this.className = "amcharts-input amcharts-input-selected";
    //});

    //document.getElementById("b6m").addEventListener("click", function () {
    //    resetButtonClass();
    //    var max = dateAxis.groupMax["day1"];
    //    var date = new Date(max);
    //    date.setMonth(date.getMonth() - 6);

    //    dateAxis.zoomToDates(
    //        date,
    //        new Date(max)
    //    );
    //    //this.className = "amcharts-input amcharts-input-selected";
    //});

    //document.getElementById("b1y").addEventListener("click", function () {
    //    resetButtonClass();
    //    var max = dateAxis.groupMax["week1"];
    //    var date = new Date(max);
    //    date.setFullYear(date.getFullYear() - 1);

    //    dateAxis.zoomToDates(
    //        date,
    //        new Date(max)
    //    );
    //    //this.className = "amcharts-input amcharts-input-selected";
    //});

    //document.getElementById("bytd").addEventListener("click", function () {
    //    resetButtonClass();
    //    var date = new Date(dateAxis.max);
    //    date.setMonth(0, 1);
    //    date.setHours(0, 0, 0, 0);
    //    dateAxis.zoomToDates(date, new Date(dateAxis.max));
    //    //this.className = "amcharts-input amcharts-input-selected";
    //});

    //document.getElementById("bmax").addEventListener("click", function () {
    //    resetButtonClass();
    //    dateAxis.zoom({ start: 0, end: 1 });
    //    //this.className = "amcharts-input amcharts-input-selected";
    //});

    //function resetButtonClass() {
    //    var selected = document.getElementsByClassName("amcharts-input-selected");
    //    for (var i = 0; i < selected.length; i++) {
    //        selected[i].className = "amcharts-input";
    //    }
    //}

    //dateAxis.events.on("selectionextremeschanged", function () {
    //    updateFields();
    //});

    //dateAxis.events.on("extremeschanged", updateFields);

    //function updateFields() {
    //    var minZoomed = dateAxis.minZoomed + am4core.time.getDuration(dateAxis.mainBaseInterval.timeUnit, dateAxis.mainBaseInterval.count) * 0.5;
    //    document.getElementById("fromfield").value = chart_Stock.dateFormatter.format(minZoomed, inputFieldFormat);
    //    document.getElementById("tofield").value = chart_Stock.dateFormatter.format(new Date(dateAxis.maxZoomed), inputFieldFormat);
    //}

    //document.getElementById("fromfield").addEventListener("keyup", updateZoom);
    //document.getElementById("tofield").addEventListener("keyup", updateZoom);

    //var zoomTimeout;
    //function updateZoom() {
    //    if (zoomTimeout) {
    //        clearTimeout(zoomTimeout);
    //    }
    //    zoomTimeout = setTimeout(function () {
    //        resetButtonClass();
    //        var start = document.getElementById("fromfield").value;
    //        var end = document.getElementById("tofield").value;
    //        if ((start.length < inputFieldFormat.length) || (end.length < inputFieldFormat.length)) {
    //            return;
    //        }
    //        var startDate = chart_Stock.dateFormatter.parse(start, inputFieldFormat);
    //        var endDate = chart_Stock.dateFormatter.parse(end, inputFieldFormat);

    //        if (startDate && endDate) {
    //            dateAxis.zoomToDates(startDate, endDate);
    //        }
    //    }, 500);
    //}
});
var Presentations = [];
$(document).ready(function () {
    var id_Camp;
    $('select#Camps').on('hidden.bs.select', function () {
        id_Camp = $('select#Camps').val();
        if (id_Camp != 0) {
            $.ajax({
                type: "GET",
                url: url + "/Trainings_Select_Async/" + id_Camp,
                contentType: "application/json;charset=UTF-8",
                dataType: "json",
                //data: id,
                success: function (result) {
                    var content = "";
                    //$('select#Trainings')
                    for (var i = 0; i < result.length; i++) {
                        Presentations.push({
                            id: result[i].id,
                            name: result[i].name
                        });
                        content += "<option value='" + result[i].id + "'>" + result[i].name + " </option >";
                    }
                    $('select#Trainings').html(content);
                    $('select#Trainings').selectpicker('refresh');
                    $('select#Results').selectpicker('val', '0');
                    $("#Trainings_Select").fadeIn('slow');
                    $("#Results_Select").fadeIn('slow');

                }
            });
        }
    });
    debugger;
    $('select#Trainings').on('shown.bs.select', function () {
        id_Training_Old = $('select#Trainings').val();
    });

    $('select#Trainings').on('hidden.bs.select', function () {
        Trainings_Results(id_Training_Old);
        id_Training_Old = $('select#Trainings').val();

        const result = Presentations.find(({ id }) => id == parseInt(id_Training_Old));

        AllParticipants(id_Camp, id_Training_Old, result.name);
    });

    $('select#Results').on('hidden.bs.select', function () {
        Trainings_Results(0);
    });

    //am4core.ready(function () {
    //    //horizontal gant
    //    var chart_Hor = am4core.create("chartdiv_Hor", am4charts.XYChart);
    //    chart_Hor.hiddenState.properties.opacity = 0; // this creates initial fade-in

    //    chart_Hor.paddingRight = 30;
    //    chart_Hor.dateFormatter.inputDateFormat = "yyyy-MM-dd HH:mm";

    //    var colorSet = new am4core.ColorSet();
    //    colorSet.saturation = 0.4;

    //    chart_Hor.data = [
    //        {
    //            name: "John",
    //            fromDate: "2018-01-01 08:00",
    //            toDate: "2018-01-01 10:00",
    //            color: colorSet.getIndex(0).brighten(0)
    //        },
    //        {
    //            name: "John",
    //            fromDate: "2018-01-01 12:00",
    //            toDate: "2018-01-01 15:00",
    //            color: colorSet.getIndex(0).brighten(0.4)
    //        },
    //        {
    //            name: "John",
    //            fromDate: "2018-01-01 15:30",
    //            toDate: "2018-01-01 21:30",
    //            color: colorSet.getIndex(0).brighten(0.8)
    //        },

    //        {
    //            name: "Jane",
    //            fromDate: "2018-01-01 09:00",
    //            toDate: "2018-01-01 12:00",
    //            color: colorSet.getIndex(2).brighten(0)
    //        },
    //        {
    //            name: "Jane",
    //            fromDate: "2018-01-01 13:00",
    //            toDate: "2018-01-01 17:00",
    //            color: colorSet.getIndex(2).brighten(0.4)
    //        },

    //        {
    //            name: "Peter",
    //            fromDate: "2018-01-01 11:00",
    //            toDate: "2018-01-01 16:00",
    //            color: colorSet.getIndex(4).brighten(0)
    //        },
    //        {
    //            name: "Peter",
    //            fromDate: "2018-01-01 16:00",
    //            toDate: "2018-01-01 19:00",
    //            color: colorSet.getIndex(4).brighten(0.4)
    //        },

    //        {
    //            name: "Melania",
    //            fromDate: "2018-01-01 16:00",
    //            toDate: "2018-01-01 20:00",
    //            color: colorSet.getIndex(6).brighten(0)
    //        },
    //        {
    //            name: "Melania",
    //            fromDate: "2018-01-01 20:30",
    //            toDate: "2018-01-01 24:00",
    //            color: colorSet.getIndex(6).brighten(0.4)
    //        },

    //        {
    //            name: "Donald",
    //            fromDate: "2018-01-01 13:00",
    //            toDate: "2018-01-01 24:00",
    //            color: colorSet.getIndex(8).brighten(0)
    //        }
    //    ];

    //    var categoryAxis = chart_Hor.yAxes.push(new am4charts.CategoryAxis());
    //    categoryAxis.dataFields.category = "name";
    //    categoryAxis.renderer.grid.template.location = 0;
    //    categoryAxis.renderer.inversed = true;

    //    var dateAxis = chart_Hor.xAxes.push(new am4charts.DateAxis());
    //    dateAxis.dateFormatter.dateFormat = "yyyy-MM-dd HH:mm";
    //    dateAxis.renderer.minGridDistance = 70;
    //    dateAxis.baseInterval = { count: 1, timeUnit: "minute" };
    //    dateAxis.max = new Date(2018, 0, 1, 24, 0, 0, 0).getTime();
    //    dateAxis.strictMinMax = true;
    //    dateAxis.renderer.tooltipLocation = 0;

    //    var series1 = chart_Hor.series.push(new am4charts.ColumnSeries());
    //    series1.columns.template.width = am4core.percent(80);
    //    series1.columns.template.tooltipText = "{name}: {openDateX} - {dateX}";

    //    series1.dataFields.openDateX = "fromDate";
    //    series1.dataFields.dateX = "toDate";
    //    series1.dataFields.categoryY = "name";
    //    series1.columns.template.propertyFields.fill = "color"; // get color from data
    //    series1.columns.template.propertyFields.stroke = "color";
    //    series1.columns.template.strokeOpacity = 1;

    //    chart_Hor.scrollbarX = new am4core.Scrollbar();

    //});
    //hide links id-967-title  url("#filter-id-986") url("#gradient-id-223")
    $("g[stroke*='#3cabff']").hide();
    $("g[stroke*='url']").hide();
    $("g[stroke*='#3cabff']").hide();
    $("g[stroke*='url']").hide();
    //$("g[filter*='id-']").hide();
    //$('g').each(function () {
    //    if ($(this).attr('aria-labelledby') *= "id-66-title") {
    //        $(this).hide();
    //    }
    //    if ($(this).attr('aria-labelledby') == "id-220-title") {
    //        $(this).hide();
    //    }
    //    if ($(this).attr('aria-labelledby') == "id-295-title") {
    //        $(this).hide();
    //    }
    //    if ($(this).attr('aria-labelledby') == "id-639-title") {
    //        $(this).hide();
    //    }
    //    if ($(this).attr('aria-labelledby') == "id-967-title") {
    //        $(this).hide();
    //    }
    //});
});



