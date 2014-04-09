function popupExpand(id)
{
    document.body.style.overflow = "hidden";

    getExpandedArgumentView(id);
}

function getExpandedArgumentView(id)
{
    $.ajax({
        type: "Post",
        datatype: 'html',
        data: "id=" + id,
        url: "/Spark/GetExpandedArgumentView",
        success: function (data)
        {
            $('.modal-content').html(data);
        }
    });
}

function getCreateArgumentView(bAgree, nSparkId) {
    //$.ajax({
    //    type: "Get",
    //    datatype: 'html',
    //    data: "bAgree=" + bAgree + "&nSparkId=" + nSparkId,
    //    url: "/Spark/CreateArgument",
    //    success: function (data)
    //    {
    //        $('.modal-content').html(data);
    //    }
    //});

    
    //if (argumentContainerVisibility == "collapse")
    //{
    //    document.getElementById('ArgumentsContainer').style.visibility = "visible";
    //    document.getElementById('CreateArgumentsForm').style.visibility = "collapse";
    //}
    //else
    //{
    //    document.getElementById('CreateArgumentsForm').style.visibility = "visible";
    //    document.getElementById('ArgumentsContainer').style.visibility = "collapse";
    //}

    //Find the visibility of the current forms.
    var argumentContainerVisibility = document.getElementById('ArgumentsContainer').style.visibility;
    var argumentContainerVisibility = document.getElementById('CreateArgumentForm').style.visibility;

    //Disable the agree disagree buttons
    document.getElementById('buttonAgree').onclick = "null";
    document.getElementById('buttonDisagree').onclick = "null";

    //Hide the container and show the argument create form.
    document.getElementById('CreateArgumentForm').style.visibility = "visible";
    document.getElementById('ArgumentsContainer').style.visibility = "collapse";
}

function addArgument(isAgree, nSparkId)
{
    getCreateArgumentView(isAgree, nSparkId);
}

function addComment() {

    var btnText = "Add Comment";
    var display = document.getElementById('sparkArgumentAddComment').style.display;
    if (display == 'none' || display == '') {
        display = 'block';
        btnText = 'Close Comment';
    }
    else {
        display = 'none';
        btnText = 'Add Comment';
    }

    document.getElementById('sparkArgumentAddComment').style.display = display;
    $("#btnAddComment").html(btnText);
}