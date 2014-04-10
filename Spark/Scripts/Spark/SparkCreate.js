//*******************************************************************************************************//
// Contains functions related to the spark creation page.                                                //
//*******************************************************************************************************//

function readUrl(input) {

    if (input.files && input.files[0]) 
    {
        var reader = new FileReader();
        reader.onload = function (e) 
        {
            $('.imgNewTagImage').attr('src', e.target.result);
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

function setArgumentType(input)
{
    var obj = document.getElementById("ArgEntryType");
    obj.setAttribute("value", input);
}

function getArgumentLayout()
{
    $.ajax({
        type: "Post",
        datatype: 'String',
        data: "strModelInfo=" + getModelString(),
        url: "/Spark/SparkCreateWithArg",
        success: function(data)
        {
            var obj = document.getElementById("divArgumentContainer");
            obj.innerHTML = data;
            fadeCreateButtons();
            fadeIn("divArgumentContainer");

        }

    });
}

function getModelString()
{
    var topic = document.getElementById("Topic").value;
    var description = document.getElementById("Description").value;
    var subjectmatterid = document.getElementById("SubjectMatterId").value;
    var tag1 = document.getElementById("Tag1").getAttribute("value");
    var tag2 = document.getElementById("Tag2").getAttribute("value");
    var tag3 = document.getElementById("Tag3").getAttribute("value");
    var tag4 = document.getElementById("Tag4").getAttribute("value");
    var tag5 = document.getElementById("Tag5").getAttribute("value");
    var argentrytype = document.getElementById("ArgEntryType").getAttribute("value");
    var strConcat = topic + ", " + description + ", " + subjectmatterid + ", " + tag1 + ", " + tag2 + ", " + tag3 + ", " + tag4 + ", " + tag5 + ", " + argentrytype;
    return strConcat;
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

// Animates the object to the left by changing the left property to 0 for the element with the given id. Sets the label associated with this element to hidden.
function fadeAway(id)
{
    $(document).ready(function () {
        //$("#" + id).animate({ top: "500px", opacity: 0, visibility:"hidden" }, 1000);
        $("#" + id).animate({ top: "500px", opacity: 0, height:"0px"}, 1000, function ()
        {
            $("#" + id).css({ visibility: "hidden" });
            $("#" + id).css({ top: "0px" });
        });
    });
}

function fadeCreateButtons()
{
    $(document).ready(function () {
        $(".classSubmitButtons").animate({opacity: 0, height: "0px" }, 1000, function () {
            $("#" + id).css({ visibility: "hidden" });
        });
    });
}

function fadeIn(id)
{
    $(document).ready(function () {
        //$("#" + id).animate({ top: "0px", opacity: 100, visibility:"visible" }, 1000);
        $("#" + id).css({ opacity: 0.0, visibility: "visible", top: "500px" }).animate({ top: "0px", opacity: 1.0, height: "700px" }, 1000);
    });
}

function nextTab()
{
    fadeAway("divMainFirst");
    fadeIn("divMainSecond");
}

function previousTab()
{
    fadeAway("divMainSecond");
    fadeIn("divMainFirst");
}

function selectTag(input) {
    
    var row = input.parentNode;
    var obj1 = row.cells[0]; // first cell in the row
    var obj2 = row.cells[1]; // second cell in the row
    var idSelected = obj1.id.substring(12);
    var cellArray = checkTableForSelection("tableAvailableTags");

    if (obj2.className == "clickedCell")
    {
        deleteRow(obj1);

        obj2.style.backgroundColor = "transparent";
        obj2.className = "nonClickedCells";
        obj1.style.backgroundColor = "transparent";
        obj1.className = "nonClickedCells";
    }
    else if (cellArray.length < 5) {
        obj2.style.backgroundColor = "#C0C0C0";
        obj2.className = "clickedCell";
        obj1.style.backgroundColor = "#C0C0C0";
        obj1.className = "clickedCell";

        addRow(obj2.innerHTML, idSelected);
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
    row.style.textAlign = "center";
    row.style.setProperty("vertical-align", "top");
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

function filterResults(inputString)
{
    var tBody = document.getElementById("tableAvailableTags").tBodies[0];
    var rows = tBody.rows;
    if (inputString == "")
    {
        for(var j = 0; j < rows.length; j++)
            rows[j].style.setProperty("display", "");
        return;
    }

    for(var i = 0; i < rows.length ; i++)
    {
        var cell = rows[i].cells[0];
        var value = cell.innerHTML;
        if(value.indexOf(inputString) == -1)
            rows[i].style.setProperty("display", "none");
        else
            rows[i].style.setProperty("display", "");
    }
}

function popupNewTagWnd()
{
    var data = document.getElementById("divTagPopup").innerHTML;
    $('.modal-content').html(data);
}

function uploadNewTag(input)
{
    var Image = document.getElementsByClassName("imgNewTagImage")[0].src;
    var Name = document.getElementsByClassName("inputNewTagName")[1].value;
    $.ajax({
        type: "Post",
        datatype: 'json',
        data: {strName: Name, strImage: Image},
        url: "/Spark/UploadNewTag",
        success: function (data) {

            var split = data.message.split(",");
            
            var tableTags = document.getElementById("tableAvailableTags");
            var tBody = tableTags.tBodies[0];
            var newRow = tBody.insertRow();
            var firstCell = newRow.insertCell();
            firstCell.style.width = "75%";
            firstCell.onclick = function () { selectTag(firstCell) };
            firstCell.id = "availableTag" + split[0].toString();
            firstCell.innerHTML = split[1];
            
            var secondCell = newRow.insertCell();
            var newImg = document.createElement("img");
            newImg.style.height = "40px"; newImg.style.width = "40px";
            newImg.src = split[2] + "," + split[3]; // Split initially removed commas, adding comma back in between data encoding and data.
            secondCell.appendChild(newImg);

            var modalCloser = document.getElementsByClassName("modal-close")[0];
            modalCloser.click();
        }
    });
}

