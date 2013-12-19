//*******************************************************************************************************//
// Contains functions related to and page load                                             //
//*******************************************************************************************************//


$(document).ready(function ()
{
    //Display the page header unless it's the portal.
    if ($(".title").text() != "Portal")
    {
        $(".NavigationBarTitleP").text($(".title").text());
    }

});

