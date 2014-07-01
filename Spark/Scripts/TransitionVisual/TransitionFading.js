/***************************************************************************************************************************************
* This set of javascript functions are used in a defined configuration that allows for elements to be separated into distinct sections *
* These sections can be given a selected status such that it is the only element in the configuration to be shown. This is done by     *
* maintaining the index of the selection and the abstract framework conventions used for this separation.                              *
*                                                                                                                                      *
* The convention requires the use of divs to act as containers. The divs should be given the appropriate ID considering its index.     *
* The ID should be "TransitionElement" + index. Example: TransitionElement0, TransitionElement1.                                       *
* The ID should also follow the index and not skip any values to avoid undesirable transitions.                                        *
* In order to call a transition, use the javascript function SingleTransition(bIsNext), where bIsNext = true for next, false for prev. *
* To transition to a specified index, use the javascript function TransitionTo(nIndex), where nIndex is the zero based index.          *
****************************************************************************************************************************************/

// Update 4/8/2014 (Randall) - Adding functionality that similarly transitions elements vertically.

// Member/Window variables used to keep track of related position of the transition.
var m_nTransitionIndex = 0; // current selected index of the page.
var m_nMaxIndex = 2; // Maximum index, needs to be initialized if not 2

var m_nTransitionVertIndex = 0; // Default
var m_nMaxVertIndex = 1; // Default
var m_nTopStartValue = 0; // Top start value used to position the vertical transition elements at the top of the container.
var m_nTopEndValue = 500; // Top end value that the vertical transition element travels to during a vertical fade away.
var m_nHeightVerticalValue = 700; // Default height of the vertical transition element.

// Horizontal Transitions
function SingleTransition(bIsNext) {
    if (bIsNext) {
        if (m_nTransitionIndex == m_nMaxIndex)
            return;

        fadeAwayLeft("TransitionElement" + m_nTransitionIndex.toString());
        fadeInLeft("TransitionElement" + (m_nTransitionIndex + 1).toString());
        ProgressBar(m_nTransitionIndex, true);
        m_nTransitionIndex++;
    }
    else {
        if (m_nTransitionIndex == 0)
            return;

        fadeAwayRight("TransitionElement" + m_nTransitionIndex.toString());
        fadeInRight("TransitionElement" + (m_nTransitionIndex - 1).toString());
        m_nTransitionIndex--;
        ProgressBar(m_nTransitionIndex, false);
    }
}

function fadeAwayLeft(id) {
    $(document).ready(function () {
        $("#" + id).animate({ opacity: 0, left: "-50%" }, 1000, function () {
            $("#" + id).css({ display: "none" });
        });
    });
}

function fadeAwayRight(id) {
    $(document).ready(function () {
        $("#" + id).animate({ opacity: 0, left: "50%" }, 1000, function () {
            $("#" + id).css({ display: "none" });
        });
    });
}

function fadeInRight(id) {
    $(document).ready(function () {
        $("#" + id).css({ opacity: 0.0, display: "block", left: "-50%" }).animate({ opacity: 1.0, left: "20%" }, 1000);
    });
}

function fadeInLeft(id) {
    $(document).ready(function () {
        $("#" + id).css({ opacity: 0.0, display: "block", left: "50%" }).animate({ opacity: 1.0, left: "20%" }, 1000);
    });
}

function InitializeMaxIndex(nIndexMax) {
    m_nMaxIndex = nIndexMax;
}

// Vertical Transitions

function SingleTransitionVert(bIsNext) {
    if (bIsNext) {
        if (m_nTransitionVertIndex == m_nMaxVertIndex)
            return;

        fadeAwayVertical("TransitionElementVert" + m_nTransitionVertIndex.toString());
        fadeInVertical("TransitionElementVert" + (m_nTransitionVertIndex + 1).toString());
        m_nTransitionVertIndex++;
    }
    else {
        if (m_nTransitionVertIndex == 0)
            return;

        fadeAwayVertical("TransitionElementVert" + m_nTransitionVertIndex.toString());
        fadeInVertical("TransitionElementVert" + (m_nTransitionVertIndex - 1).toString());
        m_nTransitionVertIndex--;
    }
}

function fadeAwayVertical(id) {
    $(document).ready(function () {
        $("#" + id).animate({ top: m_nTopEndValue + "px", opacity: 0, height: "0px" }, 1000, function () {
            $("#" + id).css({ visibility: "hidden" });
            $("#" + id).css({ top: m_nTopStartValue + "px" });
        });
    });
}

function fadeInVertical(id) {
    $(document).ready(function () {
        $("#" + id).css({ opacity: 0.0, visibility: "visible", top: m_nTopEndValue + "px" }).animate({ top: "0px", opacity: 1.0, height: m_nHeightVerticalValue + "px" }, 1000);
    });
}

function InitializeVerticalElements(nTopStartValue, nTopEndValue, nHeightVerticalValue) {
    m_nTopStartValue = nTopStartValue;
    m_nTopEndValue = nTopEndValue;
    m_nHeightVerticalValue = nHeightVerticalValue;
}

function InitializeMaxVertIndex(nIndexMax) {
    m_nMaxVertIndex = nIndexMax;
}


// Specific to the Argument Creation view
function ProgressBar(nIndex, bIsForward) {
    var ol = document.getElementsByClassName("progtrckr")[0];
    var children = ol.children;

    if (bIsForward)
        children[nIndex].className = "progtrckr-done";
    else
        children[nIndex].className = "progtrckr-todo";
}