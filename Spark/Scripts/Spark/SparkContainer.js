function CastArgumentVote(nArgumentId, bIsTrue)
{
    var strInputs = nArgumentId.toString() + "," + bIsTrue.toString();
    $.ajax({
        type: "Post",
        datatype: 'json',
        data: "strDataConcat=" + strInputs,
        url: "/Spark/CastArgumentVote",
        success: function (data) {
            // Change UI to reflect upvote or downvote
            if(data.success)
                AddVoteToSpark(nArgumentId.toString(), bIsTrue);
        }
    });
}

function AddVoteToSpark(argumentId, bIsUpvote)
{
    if (bIsUpvote)
        AddUpVote(argumentId);
    else
        AddDownVote(argumentId);
}

function AddUpVote(argumentId)
{
    var obj = document.getElementById("sparkUpvote" + argumentId);
    var obj2 = document.getElementById("HiddenUpvote" + argumentId);
    var strInner = obj2.innerHTML;

    var nValue = parseInt(strInner);
    if (nValue != "NaN")
    {
        var nValueNext = nValue + 1;
        obj.innerHTML = "+# " + nValueNext.toString();
        obj2.innerHTML = nValueNext.toString();
    }

}

function AddDownVote(argumentId)
{
    var obj = document.getElementById("sparkDownvote" + argumentId);
    var obj2 = document.getElementById("HiddenDownvote" + argumentId);
    var strInner = obj2.innerHTML;

    var nValue = parseInt(strInner);
    if (nValue != "NaN") {
        var nValueNext = nValue - 1;
        obj.innerHTML = "+# " + nValueNext.toString();
        obj2.innerHTML = nValueNext.toString();
    }
}