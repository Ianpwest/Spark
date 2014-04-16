function popupExpand(id)
{
   // document.body.style.overflow = "hidden";

    getExpandedArgumentView(id);
}

function showExpandedArgument(id)
{
    getExpandedArgumentView(id);
}

function ReturnToContainerView()
{
    //Hide the expanded and the create argument view and show the container view.
    document.getElementById('ArgumentCreate').style.visibility = "collapse";
    document.getElementById('ArgumentsContainer').style.visibility = "visible";
    document.getElementById('ArgumentView').style.visibility = "collapse";
    document.getElementById('ArgumentView').style.display = "none";
}

function GoToCreateArgumentView()
{
    document.getElementById('ArgumentCreate').style.visibility = "visible";
    document.getElementById('ArgumentsContainer').style.visibility = "collapse";
    document.getElementById('ArgumentView').style.visibility = "collapse";
    document.getElementById('ArgumentView').style.display = "inline";
}

function GoToArgumentView()
{
    document.getElementById('ArgumentCreate').style.visibility = "collapse";
    document.getElementById('ArgumentsContainer').style.visibility = "collapse";
    document.getElementById('ArgumentView').style.visibility = "visible";
    document.getElementById('ArgumentView').style.display = "inline";
}

//function getExpandedArgumentView(id) {
//    $.ajax({
//        type: "Post",
//        datatype: 'html',
//        data: "id=" + id,
//        url: "/Spark/GetExpandedArgumentView",
//        success: function (data) {
//            $('.modal-content').html(data);
//        }
//    });
//}
//function getExpandedArgumentView(id) {
//    $.ajax({
//        type: "Post",
//        datatype: 'html',
//        data: "id=" + id,
//        url: "/Spark/GetExpandedArgumentView",
//        success: function (data) {

//            //$('#ArgumentView').html(data);
//            //tinymce.EditorManager.execCommand('mceRemoveControl', true, 'txtAreaReview');
            
//            //document.getElementById("ArgumentView").innerHTML = data;
//            //tinyMCE.get('txtAreaReview').setContent(data);
//            tinyMCE.get('txtAreaReview').setContent(data);

           
//            //$('#ArgumentView').html(data);
//        }
//    });


//    //Hide the container and the create argument view and show the argument view.
//    GoToArgumentView();
//}

function getCreateArgumentView(bAgree, nSparkId) {
    $.ajax({
        type: "Get",
        datatype: 'html',
        data: "bAgree=" + bAgree + "&nSparkId=" + nSparkId,
        url: "/Spark/CreateArgument",
        success: function (data)
        {
            $('#ArgumentCreate').html(data);
        }
    });

    //Disable the agree disagree buttons
    document.getElementById('buttonAgree').onclick = "null";
    document.getElementById('buttonDisagree').onclick = "null";

    //Hide the container and show the argument create form.
    GoToCreateArgumentView();
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