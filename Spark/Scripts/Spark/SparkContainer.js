function castSparkVote(nArgumentId, bIsTrue)
{
    var strInputs = nArgumentId.toString() + "," + bIsTrue.toString();
    $.ajax({
        type: "Post",
        datatype: 'json',
        data: "strDataConcat=" + strInputs,
        url: "/Spark/CastSparkVote",
        success: function (data) {
            // Change UI to reflect upvote or downvote
            if (bIsTrue) {

            }
            else
            {

            }
        }
    });
}