function popupExpand(id)
{
    document.body.style.overflow = "hidden";

    getExpandedArgumentView(id);
}

function popupAddArgument(bAgree, nSparkId)
{
   getCreateArgumentView(bAgree, nSparkId);
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

function getCreateArgumentView(bAgree, nSparkId)
{
    $.ajax({
        type: "Get",
        datatype: 'html',
        data: "bAgree=" + bAgree + "&nSparkId=" + nSparkId,
        url: "/Spark/CreateArgument",
        success: function (data)
        {
            $('.modal-content').html(data);
        }
    });
}

function addArgument(isAgree, nSparkId)
{
    popupAddArgument(isAgree, nSparkId);
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
}