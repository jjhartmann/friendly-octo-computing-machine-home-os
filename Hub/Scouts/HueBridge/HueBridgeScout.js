jQuery.support.cors = true;

//For parameters passed in on URL
var DEVICEID = "";  //debug if this still works if no homeid is set
var API_USER_NAME = "homeosuser";

//Expect URL to be called with DeviceID parameters, 
$(document).ready(
    function () {
        var qs = getQueryStringArray();
        if (qs.DeviceId !== 'undefined' && qs.DeviceId) {
            DEVICEID = qs.DeviceId;
            UpdateDebugInfo(this, "Device name " + DEVICEID);
        }
        else {
            UpdateDebugInfo(this, "Could not extract DeviceID URL " + window.location);
        }
    }
);

function SetAPIUsername() {
    $("#cameraCreds").hide();  //may need to be hidden
    updateInformationText("Setting API access to the bridge");
    var url2 = "webapp/SetAPIUsername";
    var data2 = '{"uniqueDeviceId": "' + DEVICEID + '","username": "' + API_USER_NAME + '"}';

    new PlatformServiceHelper().MakeServiceCall(url2, data2, SetAPIUsernameCallback);
}

function SetAPIUsernameCallback(context, result) {

    UpdateDebugInfo(context, result);
    if (result[0] == "") {
        UpdateDebugInfo(context, "API username set");  

        GoToFinalSetup(DEVICEID);
    }
    else {
        
        //retry button
        $("#retryButton").show();
        updateInformationText(result[0]);
        //show cancel button
        $("#cButton").show();
    }

}

function updateInformationText(newText) {
    clearInformationText();
    $("#divInformationText").html("<p>" + newText + "</p>");
}

function clearInformationText() {
    $("#divInformationText").html("");
}

function GetInstructionsCallback(context, result) {
    UpdateDebugInfo(context, result);
    if (result[0] == "") {
        updateInformationText(result[1]);
        $("#goButton").show();
    }

    else {
        UpdateDebugInfo(this, "GetInstructionsCallback:" + result[0]);

    }

}

//Wired cameras don't need any network passwords so we go straight to final setup
function WiredCameraSetup() {
    //go to the final setup
    $("#wirelessQuestion").hide();
    GoToFinalSetup(DEVICEID);
}

function RetryButton() {
    $("#retryButton").hide();
    SetAPIUsername();
}
