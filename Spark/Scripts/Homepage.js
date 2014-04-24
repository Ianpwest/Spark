var UpvoteImage = "http://i62.tinypic.com/2yx0v47.png";
var UpvoteHoverImage = "http://i57.tinypic.com/inbn1s.png";
var UpvoteClickedImage = "http://i62.tinypic.com/294tq0y.png";
var DownvoteImage = "http://i59.tinypic.com/igj4aa.png";
var DownvoteHoverImage = "http://i60.tinypic.com/23uwjth.png";
var DownvoteClickedImage = "http://i61.tinypic.com/155qblv.png";

function ChangeImage(image, direction) {
    switch (image.id) {
        case "UpvoteIcon":
            {
                ToggleUpvote(image, direction);
            }
            break;
        case "DownvoteIcon":
            {
                ToggleDownvote(image, direction);
            }
            break;
    }
}

function ImageClicked(image) {
    //Write some code here to check to see if they are logged in maybe? Or should this just post to the server?
    switch (image.id) {
        case "UpvoteIcon":
            {
                //Change the image to the clicked image
                if (image.src == UpvoteClickedImage) {
                    image.src = UpvoteImage;
                    break;
                }
                else
                    image.src = UpvoteClickedImage;

                //Turn back on other arrows events in case they were previously turned off.
                var otherArrow = document.getElementById('DownvoteIcon');
                otherArrow.src = DownvoteImage;
            }
            break;
        case "DownvoteIcon":
            {
                if (image.src == DownvoteClickedImage) {
                    image.src = DownvoteImage;
                    break;
                }
                else
                    image.src = DownvoteClickedImage;

                //Turn back on other arrows events in case they were previously turned off.
                var otherArrow = document.getElementById('UpvoteIcon');
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