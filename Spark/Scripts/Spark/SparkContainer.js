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
            if (data.success)
            {
                AddRemoveVoteToSpark(nArgumentId.toString(), bIsTrue, true);
                if (!data.bIsNewVote)
                {
                    AddRemoveVoteToSpark(nArgumentId.toString(), !bIsTrue, false);
                }
            }
        }
    });
}

function AddRemoveVoteToSpark(argumentId, bIsUpvote, bIsAdd)
{
    if (bIsUpvote)
        AddRemoveUpVote(argumentId, bIsAdd);
    else
        AddRemoveDownVote(argumentId, bIsAdd);
}

function AddRemoveUpVote(argumentId, bIsAdd)
{
    var obj = document.getElementById("sparkUpvote" + argumentId);
    var obj2 = document.getElementById("HiddenUpvote" + argumentId);
    var strInner = obj2.innerHTML;

    var nValue = parseInt(strInner);
    if (nValue != "NaN")
    {
        var nValueNext = nValue;
        if (bIsAdd)
            nValueNext++;
        else
            nValueNext--;

        obj.innerHTML = "+# " + nValueNext.toString();
        obj2.innerHTML = nValueNext.toString();
    }

}

function AddRemoveDownVote(argumentId, bIsAdd)
{
    var obj = document.getElementById("sparkDownvote" + argumentId);
    var obj2 = document.getElementById("HiddenDownvote" + argumentId);
    var strInner = obj2.innerHTML;

    var nValue = parseInt(strInner);
    if (nValue != "NaN")
    {
        var nValueNext = nValue;
        if (bIsAdd)
            nValueNext++;
        else
            nValueNext--;
        obj.innerHTML = "-# " + nValueNext.toString();
        obj2.innerHTML = nValueNext.toString();
    }
}