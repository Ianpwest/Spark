function toggle(div_id) {
    var el = document.getElementById(div_id);
    if (el.style.display == 'none') { el.style.display = 'block'; }
    else { el.style.display = 'none'; }
}

function blanket_size(popUpDivVar) {
    if (typeof window.innerWidth != 'undefined') {
        viewportheight = window.innerHeight;
    } else {
        viewportheight = document.documentElement.clientHeight;
    }
    if ((viewportheight > document.body.parentNode.scrollHeight) && (viewportheight > document.body.parentNode.clientHeight)) {
        blanket_height = viewportheight;
    } else {
        if (document.body.parentNode.clientHeight > document.body.parentNode.scrollHeight) {
            blanket_height = document.body.parentNode.clientHeight;
        } else {
            blanket_height = document.body.parentNode.scrollHeight;
        }
    }
    var blanket = document.getElementById('blanket');
    blanket.style.height = blanket_height + 'px';
    var popUpDiv = document.getElementById(popUpDivVar);
    popUpDiv_height = blanket_height / 2 - 200;//100 is half popup's height
    popUpDiv.style.top = popUpDiv_height + 'px';
}

function window_pos(popUpDivVar) {
    if (typeof window.innerWidth != 'undefined') {
        viewportwidth = window.innerHeight;
    } else {
        viewportwidth = document.documentElement.clientHeight;
    }
    if ((viewportwidth > document.body.parentNode.scrollWidth) && (viewportwidth > document.body.parentNode.clientWidth)) {
        window_width = viewportwidth;
    } else {
        if (document.body.parentNode.clientWidth > document.body.parentNode.scrollWidth) {
            window_width = document.body.parentNode.clientWidth;
        } else {
            window_width = document.body.parentNode.scrollWidth;
        }
    }
    var popUpDiv = document.getElementById(popUpDivVar);
    window_width = window_width / 2 - 250;//250 is half popup's width
    popUpDiv.style.left = window_width + 'px';
}

function popup(windowname, bAgree) {
    blanket_size(windowname);
    window_pos(windowname);
    toggle('blanket');
    toggle(windowname);

    //clear the window
    $('.popup').html("");

    getExpandedArgumentView($(".sparkExpandAgree a").attr('title'));
}

function popup(windowname, bAgree, nSparkId)
{
    blanket_size(windowname);
    window_pos(windowname);
    toggle('blanket');
    toggle(windowname);

    //clear the window
    $('.popup').html("");

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
            $('.popup').html(data);
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
        success: function (data) {
            $('.popup').html(data);
        }
    });
}

function addArgument(isAgree, nSparkId)
{
    popup('popUpDiv', isAgree, nSparkId);
}