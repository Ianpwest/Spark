
//0 based index of the number of create indicators to show and use for page transition
InitializeMaxIndex(3);
//InitializeHorizontalElements("25.5%");



//Update the review with the contents of the other sections to display to the user what their completed argument will look like.
function UpdateReview()
{
    var conclusion = "<h2 style=\"text-align:center\">Gist</h2> <br>" + $('#editorConclusion').val();
    var argument = "<h2 style=\"text-align:center\">Argument</h2> <br>" + tinyMCE.get('editorArgument').getContent({ format: 'html' });
    var citations = "<h2 style=\"text-align:center\">References</h2> <br>" + tinyMCE.get('editorCitations').getContent({ format: 'html' });

    //$('#previewHTML').html(conclusion + "<br>" + argument + "<br>" + citations);
    tinyMCE.get('editorReview').setContent(conclusion + "<br>" + argument + "<br>" + citations);
}

//On form submission check that all fields have been filled out appropriately (they all have content except optional sources) 
//and submit
function SubmitForm()
{
    //Make sure the required fields have been filled with at least one character.
    if ($('#editorConclusion').val() == "" || tinyMCE.get('editorArgument').getContent() == "")
    {
        alert("You must fill out the gist and argument section before submitting");
        return;
    }

    $("#CreateArgumentForm").submit();
}

   