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

// Member/Window variables used to keep track of related position of the transition.
var m_nTransitionIndex = 0; // current selected index of the page.
var m_nMaxIndex = 2; // Maximum index, needs to be initialized if not 2
//var m_nPixelContainerLeftPercent = 0; // current width of containers to use for left and right animation.

function SingleTransition(bIsNext)
{
    if (bIsNext)
    {
        if (m_nTransitionIndex == m_nMaxIndex)
            return;

        fadeAwayLeft("TransitionElement" + m_nTransitionIndex.toString());
        fadeInLeft("TransitionElement" + (m_nTransitionIndex + 1).toString());
        ProgressBar(m_nTransitionIndex, true);
        m_nTransitionIndex++;
    }
    else
    {
        if (m_nTransitionIndex == 0)
            return;

        fadeAwayRight("TransitionElement" + m_nTransitionIndex.toString());
        fadeInRight("TransitionElement" + (m_nTransitionIndex - 1).toString());
        m_nTransitionIndex--;
        ProgressBar(m_nTransitionIndex, false);
    }
}

//function TransitionTo(nIndex)
//{
//    PerformTransitionToIndex(nIndex, m_nTransitionIndex);
//    m_nTransitionIndex = nIndex;
//}

//function PerformTransitionToIndex(nIndex, nPreviousIndex)
//{
//    var strIdNext = "TransitionElement" + nIndex.toString();
//    var strIdPrev = "TransitionElement" + nPreviousIndex.toString();
//    fadeAway(strIdPrev);
//    fadeIn(strIdNext);
//}

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
        $("#" + id).css({ opacity: 0.0, display: "block", left: "-50%" }).animate({ opacity: 1.0, left: "0%" }, 1000);
    });
}

function fadeInLeft(id) {
    $(document).ready(function () {
        $("#" + id).css({ opacity: 0.0, display: "block", left: "50%" }).animate({ opacity: 1.0, left: "0%" }, 1000);
    });
}

function InitializeMaxIndex(nIndexMax)
{
    m_nMaxIndex = nIndexMax;
}

function ProgressBar(nIndex, bIsForward)
{
    var ol = document.getElementsByClassName("progtrckr")[0];
    var children = ol.children;

    if (bIsForward)
        children[nIndex].className = "progtrckr-done";
    else
        children[nIndex].className = "progtrckr-todo";
}

//function InitializeContainerLeftPercent(nPixelWidth) {
//    m_nPixelContainerLeftPercent = nPixelWidth;
//}