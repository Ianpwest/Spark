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
    document.getElementById("tester").innerHTML = input.getAttribute("value");
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
    moveLeft("divDescription");
    var row = input.parentNode;
    var obj = row.cells[1];
    var cellArray = checkTableForSelection("tableTagsAvailable");

    if (obj.className == "clickedCell") {
        deleteRow(obj);
        obj.style.backgroundColor = "#FFFFFF";
        obj.className = "";
        input.style.backgroundColor = "#FFFFFF";
        input.className = "";
        obj.id = "";
    }
    else if (cellArray.length < 5) {
        obj.style.backgroundColor = "#FF0000";
        obj.className = "clickedCell";
        input.style.backgroundColor = "#FF0000";
        input.className = "clickedCell";
        var nNextIndex = 0;
        if (cellArray.length > 0)
            nNextIndex = getNextIndex();

        obj.id = "clickedCellId" + nNextIndex;
        addRow(obj.innerHTML, nNextIndex);
    }
    cellArray = checkTableForSelection("tableTagsAvailable");
    if (cellArray.length > 0)
        setVisible("divTagsAdded");
    else
        setHidden("divTagsAdded");
    if (cellArray.length == 5)
        moveLeft("divTagsAvailable");
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
    var table = document.getElementById("tableTagsAdded");
    var length = table.tBodies[0].rows.length;
    var row = table.tBodies[0].insertRow(length);
    row.id = "rowAddedId" + arrayLength;
    var cell = row.insertCell(0);
    cell.innerHTML = dataToAdd;
}

function deleteRow(tdDeleted) {
    var Id = tdDeleted.id.substring(13);
    var trId = "rowAddedId" + Id;
    var tr = document.getElementById(trId);
    var trIndex = tr.rowIndex;
    var table = document.getElementById("tableTagsAdded");
    table.deleteRow(trIndex);
}

function getNextIndex() {
    var table = document.getElementById("tableTagsAdded");
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


