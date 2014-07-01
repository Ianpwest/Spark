﻿var UpvoteImage = "http://i62.tinypic.com/2yx0v47.png";
var UpvoteHoverImage = "http://i57.tinypic.com/inbn1s.png";
var UpvoteClickedImage = "http://i62.tinypic.com/294tq0y.png";
var DownvoteImage = "http://i59.tinypic.com/igj4aa.png";
var DownvoteHoverImage = "http://i60.tinypic.com/23uwjth.png";
var DownvoteClickedImage = "http://i61.tinypic.com/155qblv.png";

function ChangeImage(nIndex, bIsUpVote, direction) {

    var image = null;

    if (bIsUpVote)
        image = document.getElementById("UpvoteIcon" + nIndex.toString());
    else
        image = document.getElementById("DownvoteIcon" + nIndex.toString());

    switch (image.id) {
        case "UpvoteIcon" +nIndex.toString():
            {
                ToggleUpvote(image, direction);
            }
            break;
        case "DownvoteIcon" +nIndex.toString():
            {
                ToggleDownvote(image, direction);
            }
            break;
    }
}

function ImageClicked(nIndex, bIsUpVote) {
    
    if (nIndex == null || bIsUpVote == null)
        return;

     var imageClicked = null;

     if (bIsUpVote.toString().toLowerCase() == "true")
        imageClicked = document.getElementById("UpvoteIcon" + nIndex.toString());
    else
        imageClicked = document.getElementById("DownvoteIcon" + nIndex.toString());

    switch (imageClicked.id) {
        case "UpvoteIcon" + nIndex.toString():
            {
                //Change the image to the clicked image
                if (imageClicked.src == UpvoteClickedImage) {
                    imageClicked.src = UpvoteImage;
                    break;
                }
                else
                    imageClicked.src = UpvoteClickedImage;

                //Turn back on other arrows events in case they were previously turned off.
                var otherArrow = document.getElementById('DownvoteIcon' + nIndex.toString());
                otherArrow.src = DownvoteImage;
            }
            break;
        case "DownvoteIcon" + nIndex.toString():
            {
                if (imageClicked.src == DownvoteClickedImage) {
                    imageClicked.src = DownvoteImage;
                    break;
                }
                else
                    imageClicked.src = DownvoteClickedImage;

                //Turn back on other arrows events in case they were previously turned off.
                var otherArrow = document.getElementById('UpvoteIcon' + nIndex.toString());
                otherArrow.src = UpvoteImage;
            }
            break;
    }
}

function ToggleUpvote(image, direction) {
    //This image has already been clicked no hover effects.
    if (image.src == UpvoteClickedImage)
        return;

    image.src = direction == 'out' ? UpvoteImage : UpvoteHoverImage;
}

function ToggleDownvote(image, direction) {
    //This image has already been clicked no hover effects.
    if (image.src == DownvoteClickedImage)
        return;

    image.src = direction == 'out' ? DownvoteImage : DownvoteHoverImage;
}

function GoToSpark(sparkID)
{
    //$.ajax({
    //    url: '@Url.Action("SparkContainer", "Spark")',
    //    type: 'GET',
    //    dataType: 'json',
    //    // we set cache: false because GET requests are often cached by browsers
    //    // IE is particularly aggressive in that respect
    //    cache: false,
    //    data: { nSparkId: sparkID },
    //    success: function () {
    //    }
    //});

    //$.ajax({
    //    type: "Get",
    //    datatype: 'html',
    //    data: "nSparkId=" + sparkID,
    //    url: "/Spark/SparkContainer",
    //    success: function (data) {
    //       //Do nothing.
    //    }
    //});
}

function CastSparkVote(nSparkId, bIsUpvote)
{
    var strInputs = nSparkId.toString() + "," + bIsUpvote.toString();
    $.ajax({
        type: "Post",
        datatype: 'json',
        data: "strDataConcat=" + strInputs,
        url: "/Home/CastSparkVote",
        success: function (data) {
            // Change UI to reflect upvote or downvote
            if (data.success)
            {
                if (data.bReverseVote)
                {
                    AddRemoveVoteToSpark(nSparkId.toString(), bIsUpvote, false);
                }
                else
                {
                    AddRemoveVoteToSpark(nSparkId.toString(), bIsUpvote, true);
                    if (!data.bIsNewVote)
                    {
                        AddRemoveVoteToSpark(nSparkId.toString(), !bIsUpvote, false);
                    }
                }
            }
        }
    });
}

function AddRemoveVoteToSpark(nSparkId, bIsUpvote, bIsAdd)
{
    var obj = document.getElementById("HiddenSparkUpvote" + nSparkId);
    var obj2 = document.getElementById("HiddenSparkDownvote" + nSparkId);
    var objDisplay = document.getElementById("VoteCount" + nSparkId);
    var strInner;
    var strInner2;
    strInner = obj.innerHTML;
    strInner2 = obj2.innerHTML;

    var nValue = parseInt(strInner);
    var nValue2 = parseInt(strInner2);
    if (nValue != "NaN" && nValue2 != "NaN")
    {
        var nValueNext = nValue;
        var nValueNext2 = nValue2;
        if (bIsUpvote)
        {
            if (bIsAdd)
                nValueNext++;
            else
                nValueNext--;
        }
        else
        {
            if (bIsAdd)
                nValueNext2++;
            else
                nValueNext2--;
        }

        obj.innerHTML = nValueNext.toString();
        obj2.innerHTML = nValueNext2.toString();

        objDisplay.innerHTML = "(+" + nValueNext.toString() + "/ -" + nValueNext2.toString() + ")";
    }
}

function Filter()
{
    var Category = document.getElementById("Category");
    var SelectedCategory = Category.options[Category.selectedIndex].value;

    var Tag = document.getElementById("Tag");
    var SelectedTag = Tag.options[Tag.selectedIndex].value;

    var SearchText = document.getElementById('Search').value;

    $.ajax({
        type: "Get",
        datatype: 'html',
        data: "strCategory=" + SelectedCategory +  "&strTag=" + SelectedTag +"&strSearchText=" + SearchText,
        url: "/Home/GetFilterResults",
        success: function (data) {
            document.getElementById("TileContainer").innerHTML = data;
        }
    });
}

function getNextPage() {
    // gets all of the spark tiles by using the class name
    var tiles = document.getElementsByClassName("SparkTile");
    var strArray = "";

    for (i = 0; i < tiles.length; i++) {
        var str = tiles[i].id.toString(); // creates a string from the id
        str = str.replace("HomePageSpark", ""); // removes the prefix to give just the spark Id

        strArray += str + ","; // creates a comma delimited string to pass back through ajax.
    }
    // Removes the last comma in the string if it exists.
    if (strArray.lastIndexOf(",") > -1)
        strArray = strArray.substring(0, strArray.lastIndexOf(","));


    $.ajax({
        type: "Post",
        datatype: 'json',
        data: "strSparkIds=" + strArray,
        url: "/Home/GetNextSparks",
        success: function (data) {

            // destringify the return data to generate models

        }
    });
}