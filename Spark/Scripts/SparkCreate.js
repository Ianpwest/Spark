//*******************************************************************************************************//
// Contains functions related to the spark creation page.                                                //
//*******************************************************************************************************//

function readUrl(input) {

    if (input.files && input.files[0]) 
    {
        var reader = new FileReader();
        reader.onload = function (e) 
        {
            $('#imgPreview').attr('src', e.target.result);
        }

        reader.readAsDataURL(input.files[0]);
    }
}

