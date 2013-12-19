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

function getImage(input)
{
    document.getElementById("tester").innerHTML = input.getAttribute("value");
    var option = input.options[input.selectedIndex].value;
    $.ajax({
        type: "Post",
        datatype: 'json',
        data: "nCategoryId=" + option,
        url: "/Spark/GetImage",
        success: function (data) {
             
            var img = document.getElementById("imgPreview");
            img.src = data.message;
        }
    });
}

