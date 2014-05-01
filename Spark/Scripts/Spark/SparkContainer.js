function CastArgumentVote(nArgumentId, bIsUpVote)
{
    var strInputs = nArgumentId.toString() + "," + bIsUpVote.toString();
    $.ajax({
        type: "Post",
        datatype: 'json',
        data: "strDataConcat=" + strInputs,
        url: "/Spark/CastArgumentVote",
        success: function (data) {
            // Change UI to reflect upvote or downvote
            if (data.success)
            {
                if (data.bReverseVote) // If the vote was reveresed, take off a vote on the selected vote type.
                {
                    AddRemoveVoteToArgument(nArgumentId.toString(), bIsUpVote, false);
                }
                else
                {
                    AddRemoveVoteToArgument(nArgumentId.toString(), bIsUpVote, true);
                    if (!data.bNewVote) {
                        AddRemoveVoteToArgument(nArgumentId.toString(), !bIsUpVote, false);
                    }
                }
            }
        }
    });
}

// Calls the functions to add or remove a vote from the interface.
// argumentId = identity of the argument to add or remove, bIsUpVote = which type of vote to manipulate, 
// bIsAdd = true for adding a vote and false for subtracting one.
function AddRemoveVoteToArgument(argumentId, bIsUpvote, bIsAdd)
{
    if (bIsUpvote)
        AddRemoveArgumentUpVote(argumentId, bIsAdd);
    else
        AddRemoveArgumentDownVote(argumentId, bIsAdd);
}

function AddRemoveArgumentUpVote(argumentId, bIsAdd)
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

function AddRemoveArgumentDownVote(argumentId, bIsAdd)
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