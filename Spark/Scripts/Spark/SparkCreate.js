//*******************************************************************************************************//
// Contains functions related to the spark creation page.                                                //
//*******************************************************************************************************//

function readUrl(input) {

    if (input.files && input.files[0]) 
    {
        var reader = new FileReader();
        reader.onload = function (e) 
        {
            $('#imgPreview').attr('src', e.target.result);
        }

        reader.readAsDataURL(input.files[0]);
    }
}

// Calls a json object back from the server containing the image and decoding information given a subject Id as input.
function getImage(input)
{
    var option = input.options[input.selectedIndex].value;
    $.ajax({
        type: "Post",
        datatype: 'json',
        data: "nCategoryId=" + option,
        url: "/Spark/GetImage",
        success: function (data) {
             
            var img = document.getElementById("imgPreview");
            img.src = data.message;
        }
    });
}

function updateModelWithTags(input)
{
    var arrayTags = input;
    $.ajax(
        {
            type: "Post",
            datatype: 'json',
            data: "arrayCategories=" + arrayTags,
            url: "/Spark/UpdateModel"
        });
}

// Sets the visibility to visible given the id of an element.
function setVisible(id)
{
    var obj = document.getElementById(id);
    obj.style.setProperty("visibility", "visible");
}

// Animates the object to the left by changing the left property to 0 for the element with the given id. Sets the label associated with this element to hidden.
function animateLeft(id)
{
    $(document).ready(function () {
        $("#" + id).animate({ left: '0' });
    });

    var obj = document.getElementById(id + "lbl");
    obj.style.setProperty("visibility", "hidden");
}

function testCells(input) {
    
    var row = input.parentNode;
    var idSelected = input.id.substring(12);
    var obj = row.cells[1];
    var cellArray = checkTableForSelection("tableAvailableTags");

    if (obj.className == "clickedCell")
    {
        deleteRow(input);

        obj.style.backgroundColor = "transparent";
        obj.className = "";
        input.style.backgroundColor = "transparent";
        input.className = "";
    }
    else if (cellArray.length < 5) {
        obj.style.backgroundColor = "#FF0000";
        obj.className = "clickedCell";
        input.style.backgroundColor = "#FF0000";
        input.className = "clickedCell";

        addRow(obj.innerHTML, idSelected);
    }
}

function checkTableForSelection(id) {
    var tab = document.getElementById(id);
    var rows = tab.rows;
    var returnArray = new Array();
    var count = 0;
    for (var i = 0 ; i < rows.length; i++) {
        var row = rows[i];
        var cell = row.cells[0];
        if (cell.className == "clickedCell") {
            returnArray[count] = cell.innerHTML;
            count++;
        }
    }

    return returnArray;
}

function addRow(dataToAdd, arrayLength) {
    var table = document.getElementById("tableSelectedTags");
    var length = table.tBodies[0].rows.length;
    var row = table.tBodies[0].insertRow(length);
    row.id = "rowAddedId" + arrayLength;
    var cell = row.insertCell(0);
    cell.innerHTML = dataToAdd;

    // Moves the current index up by one when a row is added.
    var index = parseInt(document.getElementById("divHiddenTags").innerHTML);
    index = index + 1;
    document.getElementById("divHiddenTags").innerHTML = index;
    changeTagValues();
}

function deleteRow(tdDeleted) {
    var Id = tdDeleted.id.substring(12);
    var trId = "rowAddedId" + Id;
    var tr = document.getElementById(trId);
    var trIndex = tr.rowIndex;
    var table = document.getElementById("tableSelectedTags");
    table.deleteRow(trIndex);

    // Moves the current index down by one when a row is deleted.
    var index = parseInt(document.getElementById("divHiddenTags").innerHTML);
    index = index - 1;
    document.getElementById("divHiddenTags").innerHTML = index;
    changeTagValues();
}

function getNextIndex() {
    var table = document.getElementById("tableSelectedTags");
    var rows = table.tBodies[0].rows;
    var numArray = new Array();
    for (var i = 0; i < rows.length ; i++) {
        var actualNum = rows[i].id.substring(10);
        numArray[i] = actualNum; numArray[i] + ",";
    }
    counter = 0;
    for (var j = numArray.length; j >= 0 ; j--) {
        var test = $.inArray(j.toString(), numArray);
        if (test == -1)
            counter = j;
    }
    return counter;
}

function changeTagValues()
{
    var table = document.getElementById("tableSelectedTags");
    var rows = table.tBodies[0].rows;
    clearTagValues();
    for (var i = 0; i < rows.length ; i++)
    {
        var row = rows[i];
        var index = row.id.substring(10);
        if (i == 0)
            document.getElementById("Tag1").setAttribute("value", index);
        if (i == 1)
            document.getElementById("Tag2").setAttribute("value", index);
        if (i == 2)
            document.getElementById("Tag3").setAttribute("value", index);
        if (i == 3)
            document.getElementById("Tag4").setAttribute("value", index);
        if (i == 4)
            document.getElementById("Tag5").setAttribute("value", index);
    }
}

function clearTagValues()
{
    document.getElementById("Tag1").setAttribute("value", "-1");
    document.getElementById("Tag2").setAttribute("value", "-1");
    document.getElementById("Tag3").setAttribute("value", "-1");
    document.getElementById("Tag4").setAttribute("value", "-1");
    document.getElementById("Tag5").setAttribute("value", "-1");
}


