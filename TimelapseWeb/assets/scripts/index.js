var Index = function () {
    var isLogin = false;
    var createUserState = false;
    var exitRegister = false;
    var EvercamApi = "https://api.evercam.io/v1";
    var EvercamProxy = "https://evr.cm/";
    var tokenUrl = "http://webapi.camba.tv/v1/auth/token";
    var tokenInfoUrl = "http://astimegoes.by/v1/tokens/";
    var timelapseApiUrl = "http://astimegoes.by/v1/timelapses";
    var utilsApi = "http://astimegoes.by/v1";
    var ApiAction = 'POST';
    var apiContentType = 'application/json; charset=utf-8';
    var loopCount = 1;

    $("#btnAnotherUser").live("click", function () {
        $("#user_email").val("");
        $("#divEmailInput").show();
        $("#divRemember").show();
        $("#divEmail").hide();
        $("#lblEmail").text("");
        $("#user_email").focus();
        $("#btnAnotherUser").hide();
        $("#user_password").val("");
        $("#user_remember_me").attr("checked", false);
    });

    var getParameterByName = function(name, searchString) {
        name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(searchString); //location.search);
        return results == null ? null : decodeURIComponent(results[1].replace(/\+/g, " "));
    };

    var getQueryStringByName = function(name) {
        name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(location.search);
        return results == null ? null : decodeURIComponent(results[1].replace(/\+/g, " "));
    };

    function toTitleCase(str) {
        //return str.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
        return str.charAt(0).toUpperCase() + str.substr(1);
    }

    function autoLogIn(code) {
        var form = document.createElement("form");
        var element1 = document.createElement("input");
        var element2 = document.createElement("input");
        var element3 = document.createElement("input");
        var element4 = document.createElement("input");
        var element5 = document.createElement("input");

        form.method = "POST";
        form.action = "https://dashboard.evercam.io/oauth2/token";

        element1.value = code;
        element1.name = "code";
        form.appendChild(element1);

        element2.value = c4203c3e;
        element2.name = "client_id";
        form.appendChild(element2);

        element2.value = "55e2df6518ec146e0d968d640064017d";
        element2.name = "client_secret";
        form.appendChild(element2);

        element2.value = "http://astimegoes.by";
        element2.name = "redirect_uri";
        form.appendChild(element2);

        element2.value = "authorization_code";
        element2.name = "grant_type";
        form.appendChild(element2);

        document.body.appendChild(form);

        form.submit();
    }

    var format = function(state) {
        if (!state.id) return state.text; // optgroup
        return "<img class='flag' src='assets/img/flags/" + state.id.toLowerCase() + ".png'/>&nbsp;&nbsp;" + state.text;
    };
    
    var handleLoginSection = function () {
        /* ENABLE FOR TESTING ONLY */
        localStorage.setItem("oAuthTokenType", "bearer");
        localStorage.setItem("oAuthToken", "6ec26b9eb6d125c9e9c8fbcc38d3a676");
        localStorage.setItem("tokenExpiry", "315568999");
        localStorage.setItem("timelapseUserId", "shakeelanjum")
        localStorage.setItem("timelapseUsername", "Shakeel Anjum");
        /* --------- END --------- */
        if (localStorage.getItem("oAuthToken") != null && localStorage.getItem("oAuthToken") != undefined) {
            getMyTimelapse();
            getUsersInfo();
        }
        else {
            $("#divTimelapses").html('');
            $(".fullwidthbanner-container").show();

            var stringHash = window.location.hash;
            
            if (stringHash != "")
            {
                var hasToken = getParameterByName('access_token', "?" + stringHash.substring(1));
                
                if (hasToken != "" && hasToken != null) {
                    var hasTokenType = getParameterByName('token_type', window.location.hash);
                    var tokenExpiry = getParameterByName('expires_in', window.location.hash);
                    $.ajax({
                        type: 'POST',
                        url: utilsApi + '/tokeninfo',
                        dataType: 'json',
                        data: { token_endpoint: "https://api.evercam.io/oauth2/tokeninfo?access_token=" + hasToken },
                        ContentType: 'application/x-www-form-urlencoded',
                        success: function (res) {
                            if (res.userid != "null" && res.userid != '') {
                                var userId = res.userid;
                                localStorage.setItem("oAuthTokenType", hasTokenType);
                                localStorage.setItem("oAuthToken", hasToken);
                                localStorage.setItem("tokenExpiry", tokenExpiry);

                                localStorage.setItem("timelapseUserId", userId);
                                localStorage.setItem("timelapseUsername", userId);
                                window.location.hash = '';
                                getUsersInfo();
                                getCameras(false);

                            } else window.location = 'login.html';
                        },
                        error: function (xhr, textStatus) {

                        }
                    });
                } else
                    window.location = 'login.html';
            } else
                window.location = 'login.html';
            
            $(".default-timelapse").show();
            $("#liUsername").hide();
            $("#lnkSignout").hide();
            $(".timelapseContainer").hide();
            $(".responsivenav").removeClass("btn");
            $(".responsivenav").removeClass("btn-navbar");
        }
    }

    var clearPage = function() {
        $(".default-timelapse").html("");
        $(".default-timelapse").hide();
        $("#liUsername").show();
        $("#lnkSignout").show();
        $("#btnNewTimelapse").show();
        $("#divMainContainer").removeClass("container-bg");

        $("#newTimelapse").html("");
        $("#newTimelapse").fadeOut();
    };

    var handleLogout = function() {
        $("#lnkLogout").bind("click", function() {
            localStorage.removeItem("oAuthToken");
            localStorage.removeItem("tokenExpiry");
            localStorage.removeItem("oAuthTokenType");
            localStorage.removeItem("timelapseUserId");
            localStorage.removeItem("timelapseUsername");
            localStorage.removeItem("timelapseCameras");
            localStorage.removeItem("sharedcameras");
            window.location = 'login.html';
        });
    };

    var handleNewTimelapse = function() {
        $("#lnNewTimelapse").bind("click", function() {
            showTimelapseForm();
        });

        $("#lnNewTimelapseCol").bind("click", function() {
            $("#newTimelapse").slideUp(500, function() {
                $("#newTimelapse").html("");
                $("#lnNewTimelapse").show();
                $("#lnNewTimelapseCol").hide();
            });

            ApiAction = 'POST';
            $("#txtCameraCode0").val('');
        });

        $("#lnNewCamera").bind("click", function() {
            showNewCameraForm();
        });
        $("#lnNewCameraCol").bind("click", function() {
            $("#newTimelapse").slideUp(500, function() {
                $("#newTimelapse").html("");
                $("#lnNewCamera").show();
                $("#lnNewCameraCol").hide();
            });
        });
    };

    var showNewCameraForm = function() {
        $.get('NewCameraForm.html', function(data) {
            $("#newTimelapse").html(data);
            $("#lnNewCamera").hide();
            $("#lnNewCameraCol").show();

            $("#lnNewTimelapse").show();
            $("#lnNewTimelapseCol").hide();
            $("#newTimelapse").slideDown(500);
        });
    };

    $(".newTimelapse").live("click", function() {
        showTimelapseForm();
        $("#divLoadingTimelapse").fadeOut();
    });

    var showTimelapseForm = function() {
        $.get('NewTimelapse.html', function(data) {
            $("#newTimelapse").html(data);
            $("#lnNewTimelapse").hide();
            $("#lnNewTimelapseCol").show();
            //Uncomment when fixed add camera functionality
            //$("#lnNewCamera").show();
            //$("#lnNewCameraCol").hide();
            handleFileupload();
            getCameras(true);
            $('.timerange').timepicker({
                minuteStep: 1,
                showSeconds: false,
                showMeridian: false,
                defaultTime: false
            });
            var dates = $(".daterange").datepicker({
                format: 'dd/mm/yyyy',
                minDate: new Date()
            });
            $("#newTimelapse").slideDown(500);
        });
    };

    $('[name="TimeRange"]').live("click", function () {
        var id = $(this).attr("id");
        var dataval = $(this).attr("data-val");
        if (id == "chkTimeRange" + dataval) {
            $("#divTimeRange" + dataval).slideDown();
        }
        else
            $("#divTimeRange" + dataval).slideUp();
    });

    $('[name="DateRange"]').live("click", function () {
        var id = $(this).attr("id");
        var dataval = $(this).attr("data-val");
        if (id == "chkDateRange" + dataval) {
            $("#divDateRange" + dataval).slideDown();
        }
        else
            $("#divDateRange" + dataval).slideUp();
    });

    var handleMyTimelapse = function() {
        $("#lnMyTimelapse").bind("click", function() {
            getMyTimelapse();
        });
    };

    $(".formButtonCancel").live("click", function () {
        var id = $(this).attr("data-val");
        if (id != "0") {
            var code = $("#txtCameraCode" + id).val();
        }
        $("#newTimelapse").slideUp(500, function () {
            $("#newTimelapse").html("");
            $("#lnNewTimelapse").show();
            $("#lnNewTimelapseCol").hide();
        });

        ApiAction = 'POST';
        $("#txtCameraCode0").val('');
        if ($("#divTimelapses").html() == "")
            $("#divLoadingTimelapse").fadeIn();
    });

    $(".formButtonOk").live("click", function () {
        var timelapseId = $(this).attr("data-val");
        $("#divAlert" + timelapseId).removeClass("alert-info").addClass("alert-error");
        if ($("#ddlCameras" + timelapseId).val() == "") {
            $("#divAlert" + timelapseId).slideDown();
            $("#divAlert" + timelapseId + " span").html("Please select camera to continue.");
            return;
        }
        if ($("#txtTitle" + timelapseId).val() == '') {
            $("#divAlert" + timelapseId).slideDown();
            $("#divAlert" + timelapseId + " span").html("Please enter timelapse title.");
            return;
        }
        if ($("#ddlIntervals" + timelapseId).val() == 0) {
            $("#divAlert" + timelapseId).slideDown();
            $("#divAlert" + timelapseId + " span").html("Please select timelapse interval.");
            return;
        }
        var d = new Date();
        var fromDate = d.getDate() + '/' + (d.getMonth()+1) + '/' + d.getFullYear();
        var toDate = fromDate;
        var fromTime = "00:00";
        var toTime = fromTime;
        var dateAlways = true;
        var timeAlways = true;
        if ($("#chkDateRange" + timelapseId).is(":checked")) {
            dateAlways = false;
            fromDate = $("#txtFromDateRange" + timelapseId).val();
            if (fromDate == "")
            {
                $("#divAlert" + timelapseId).slideDown();
                $("#divAlert" + timelapseId + " span").html("Please select from date range.");
                return;
            }
            toDate = $("#txtToDateRange" + timelapseId).val();
            if (toDate == "") {
                $("#divAlert" + timelapseId).slideDown();
                $("#divAlert" + timelapseId + " span").html("Please select to date range.");
                return;
            }
            if (!validateDates(fromDate, toDate, timelapseId)) {
                return;
            }
        }
        if ($("#chkTimeRange" + timelapseId).is(":checked")) {
            timeAlways = false;
            fromTime = $("#txtFromTimeRange" + timelapseId).val();
            if (fromTime == "") {
                $("#divAlert" + timelapseId).slideDown();
                $("#divAlert" + timelapseId + " span").html("Please select from time range.");
                return;
            }
            toTime = $("#txtToTimeRange" + timelapseId).val();
            if (toTime == "") {
                $("#divAlert" + timelapseId).slideDown();
                $("#divAlert" + timelapseId + " span").html("Please select to time range.");
                return;
            }
            if (fromTime == toTime) {
                $("#divAlert" + timelapseId).slideDown();
                $("#divAlert" + timelapseId + " span").html('To time and from time cannot be same.');
                return;
            }
        }
        var timezone = "GMT Standard Time";
        var cams = JSON.parse(localStorage.getItem("timelapseCameras"));
        for (var i = 0; i < cams.cameras.length; i++) {
            if (cams.cameras[i].id == $("#ddlCameras" + timelapseId).val())
                timezone = cams.cameras[i].timezone;
        }
        cams = JSON.parse(localStorage.getItem("sharedcameras"));
        if (cams != null && cams != undefined) {
            for (var i = 0; i < cams.cameras.length; i++) {
                if (cams.cameras[i].id == $("#ddlCameras" + timelapseId).val())
                    timezone = cams.cameras[i].timezone;
            }
        }
        
        var camCode = "/users/" + localStorage.getItem("timelapseUserId");
        if ($("#txtTimelapseId").val() != "") {
            ApiAction = 'PUT';
            apiContentType = "application/x-www-form-urlencoded";
            camCode = "/" + $("#txtCameraCode" + timelapseId).val() + "/users/" + localStorage.getItem("timelapseUserId");
        }
        var o = {
            "camera_eid": $("#ddlCameras" + timelapseId).val(),
            "access_token": localStorage.getItem("oAuthToken"),
            "from_time": fromTime,
            "to_time": toTime,
            "from_date": fromDate,
            "to_date": toDate,
            "title": $("#txtTitle" + timelapseId).val(),
            "time_zone": timezone,
            "enable_md": false,
            "md_thrushold": 0,
            "exclude_dark": false,
            "darkness_thrushold": 0,
            "privacy": 0,
            "is_recording": $("#chkRecordingTimelapse" + timelapseId).is(":checked"),
            "is_date_always": dateAlways,
            "is_time_always": timeAlways,
            "interval": $("#ddlIntervals" + timelapseId).val(),
            "fps": $("#ddlFrameRate" + timelapseId).val()
        };
        
        if ($('.table-striped td.name').html() != undefined) {
            $(".fileupload-buttonbar").hide();
            $("#divAlert" + timelapseId).removeClass("alert-error").addClass("alert-info");
            $("#divAlert" + timelapseId).slideDown();
            $("#divAlert" + timelapseId + " span").html('<img src="assets/img/loader3.gif"/>&nbsp;Saving Twitter Response');
            $('.table-striped td.start button.btn').click();
            setTimeout(function () { checkFileUploadAndSave(timelapseId, camCode, o, $(".progress-success").attr("aria-valuemin")); }, 1000);
        }
        else
            checkFileUploadAndSave(timelapseId, camCode, o, null);
    });

    var fileuploadFinish = function(timelapseId, camCode, o, percentage) {
        checkFileUploadAndSave(timelapseId, camCode, o, percentage);
    };

    var checkFileUploadAndSave = function(timelapseId, camCode, o, percentage) {
        $("#divAlert" + timelapseId).removeClass("alert-error").addClass("alert-info");
        $("#divAlert" + timelapseId).slideDown();
        $("#divAlert" + timelapseId + " span").html('<img src="assets/img/loader3.gif"/>&nbsp;Saving timelapse');

        if (percentage != null && parseInt(percentage) <= 100) {
            setTimeout(function() { fileuploadFinish(timelapseId, camCode, o, $(".progress-success").attr("aria-valuemin")); }, 1000);
            return;
        } else {
            if ($('.table-striped td.preview a').length > 0)
                $("#txtLogoFile").val($('.table-striped td.preview a').attr("href"));
            else if ($("#txtLogoFile").val() == '')
                $("#txtLogoFile").val('-');
        }
        o.watermark_position = $("#ddlWatermarkPos" + timelapseId).val();
        o.watermark_file = $("#txtLogoFile").val();

        $.ajax({
            type: ApiAction,
            url: timelapseApiUrl + camCode,
            data: o,
            dataType: 'json',
            ContentType: apiContentType,//'application/json; charset=utf-8',
            /*beforeSend: function (xhrObj) {
                xhrObj.setRequestHeader("Authorization", "Basic " + localStorage.getItem("oAuthToken"));
            },*/
            success: function(data) {
                $("#divAlert" + timelapseId + " span").html('Timelapse saved.');
                if ($("#txtCameraCode0").val() != "")
                    $("#tab" + $("#txtCameraCode0").val()).remove();
                //if (timelapseId == "0") {
                $("#divTimelapses").prepend(getHtml(data));
                $("#newTimelapse").slideUp(500, function() { $("#newTimelapse").html(""); });
                $("#lnNewTimelapse").show();
                $("#lnNewTimelapseCol").hide();
                if ($(".timelapseContainer").css("display") == "none")
                    $(".timelapseContainer").fadeIn();
                $("#divContainer" + data.id).slideDown(500);
                /*} else {
                    $("#timelapseTitle" + timelapseId).html($("#txtTitle" + timelapseId).val());
                }*/

                ApiAction = 'POST';
                apiContentType = 'application/json; charset=utf-8';
                setTimeout(function() {
                    $("#divAlert" + timelapseId).slideUp();
                }, 6000);
            },
            error: function(xhr, textStatus) {
                $("#divAlert" + timelapseId).removeClass("alert-info").addClass("alert-error");
                $("#divAlert" + timelapseId + " span").html('Timelapse could not be saved.');
            }
        });
    };

    $(".cameraformButtonOk").live("click", function () {
        var caneraSnaps;
        $("#divAlert0").removeClass("alert-info").addClass("alert-error");
        if ($("#txtCameraUniqueId").val() == "") {
            $("#divAlert0").slideDown();
            $("#divAlert0 span").html("Please enter unique camera ID.");
            return;
        }
        if ($("#txtCameraName").val() == '') {
            $("#divAlert0").slideDown();
            $("#divAlert0 span").html("Please enter camera name.");
            return;
        }
        if ($("#txtCameraHost").val() == "") {
            $("#divAlert0").slideDown();
            $("#divAlert0 span").html("Please enter camera host.");
            return;
        }
        if ($("#txtCameraUsername").val() != "" && $("#txtCameraPassword").val() == "") {
            $("#divAlert0").slideDown();
            $("#divAlert0 span").html("Please enter camera Password.");
            return;
        }
        if ($("#txtCameraUsername").val() == "" && $("#txtCameraPassword").val() != "") {
            $("#divAlert0").slideDown();
            $("#divAlert0 span").html("Please enter camera Username.");
            return;
        }
        
        var o = {
            "id": $("#txtCameraUniqueId").val(),
            "name": $("#txtCameraName").val(),
            "is_public": $("#rdPublicCamera").is(":checked"),
            "external_host": $("#txtCameraHost").val()
        };

        if ($("#txtCameraHttpPort").val() != "")
            o.external_http_port = $("#txtCameraHttpPort").val();
        if ($("#txtCameraRtspPort").val() != "")
            o.external_rtsp_port = $("#txtCameraRtspPort").val();

        if ($("#txtCameraUrl").val() != "") 
            o.jpg_url = $("#txtCameraUrl").val();
        if ($("#txtCameraUsername").val() != "" && $("#txtCameraPassword").val() != "") {
            o.cam_username = $("#txtCameraUsername").val();
            o.cam_password = $("#txtCameraPassword").val();
        }

        $("#divAlert0").removeClass("alert-error").addClass("alert-info");
        $("#divAlert0").slideDown();
        $("#divAlert0 span").html('<img src="assets/img/loader3.gif"/>&nbsp;Saving Camera');
        //Evercam.setBasicAuth("", "", localStorage.getItem("oAuthToken"));
        //Evercam.Camera.create(o);
        $.ajax({
            type: "POST",
            url: EvercamApi + "/cameras.json",
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            data: o,
            dataType: 'json',
            ContentType: "application/x-www-form-urlencoded",//'application/json; charset=utf-8',
            beforeSend: function (xhrObj) {
                xhrObj.setRequestHeader("Authorization", localStorage.getItem("oAuthTokenType") + " " + localStorage.getItem("oAuthToken"));
            },
            success: function (data) {
                $("#divAlert0 span").html('Camera saved.');
                $("#txtCameraUniqueId").val('');
                $("#txtCameraName").val('');
                $("#txtCameraEndpoints").val('');
                $("#txtCameraUrl").val('');
                $("#txtCameraUsername").val('');
                $("#txtCameraPassword").val('');
                setTimeout(function () {
                    $("#newTimelapse").slideUp(500, function () { $("#newTimelapse").html(""); });
                    $("#divAlert0").slideUp();
                }, 6000);
            },
            error: function (xhr, textStatus) {
                $("#divAlert0").removeClass("alert-info").addClass("alert-error");
                $("#divAlert0 span").html(xhr.responseJSON.message);
            }
        });
    });

    $(".cameraformButtonCancel").live("click", function () {
        $("#newTimelapse").slideUp(500, function () {
            $("#newTimelapse").html("");
            $("#lnNewCamera").show();
            $("#lnNewCameraCol").hide();
        });
    });

    var validateDates = function(fromDate, toDate, timelapseId) {
        var movieToStr = toDate.split("/");
        var movieFromStr = fromDate.split("/");
        var td = new Date(movieToStr[2], (movieToStr[1] - 1), movieToStr[0]);
        var fd = new Date(movieFromStr[2], (movieFromStr[1] - 1), movieFromStr[0]);
        var currentTime = new Date();
        currentTime.setHours(0);
        currentTime.setMinutes(0);
        currentTime.setSeconds(0);
        currentTime.setMilliseconds(0);

        if ($("#txtTimelapseId").val() == "" && fd < currentTime) {
            $("#divAlert" + timelapseId).slideDown();
            $("#divAlert" + timelapseId + " span").html("From date cannot be less than current time.");
            return false;
        }
        if (td < currentTime) {
            $("#divAlert" + timelapseId).slideDown();
            $("#divAlert" + timelapseId + " span").html("To date cannot be less than current time.");
            return false;
        }
        if (td < fd) {
            $("#divAlert" + timelapseId).slideDown();
            $("#divAlert" + timelapseId + " span").html('To date cannot be less than from date.');
            return false;
        }
        /*if (Date.parse(todate) == Date.parse(fromdate)) {
            $("#lblDateMsg").html("To date and from date cannot be same.");
            $("#lblDateMsg").show();
            return false;
        }
        var diff = (todate - fromdate) / 3600000;
        if (diff > 2) {
            $("#lblDateMsg").html("Cannot create clip longer than two hours.");
            $("#lblDateMsg").show();
            return false;
        }*/
        return true;
    };

    var getMyTimelapse = function() {
        $(".default-timelapse").html("");
        $(".default-timelapse").hide();
        $("#liUsername").show();
        $("#lnkSignout").show();
        $("#btnNewTimelapse").show();
        $("#divMainContainer").removeClass("container-bg");

        $("#newTimelapse").html("");
        $("#newTimelapse").fadeOut();

        $("#displayUsername").html(localStorage.getItem("timelapseUsername"));
        $("#divLoadingTimelapse").fadeIn();
        $("#divLoadingTimelapse").html('<img src="assets/img/loader3.gif" alt="Loading..."/>&nbsp;Fetching Timelapses');
        $.ajax({
            type: 'GET',
            url: timelapseApiUrl + "/users/" + localStorage.getItem("timelapseUserId"),
            dataType: 'json',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            /*beforeSend: function (xhrObj) {
                xhrObj.setRequestHeader("Authorization", "Basic " + localStorage.getItem("oAuthToken"));
            },*/
            success: function(data) {
                if (data.length == 0) {
                    $("#divTimelapses").html('');
                    $("#divLoadingTimelapse").html('You have not created any timelapses. <a href="javascript:;" class="newTimelapse">Click</a> to create one.');
                } else {
                    var count = 1;
                    var html = '';
                    for (var i = 0; i < data.length; i++) {
                        html += getHtml(data[i]);
                    }
                    $("#divTimelapses").html(html);
                    $(".timelapseContainer").fadeIn();
                    $("#divLoadingTimelapse").fadeOut();

                    $('.timerange').timepicker({
                        minuteStep: 1,
                        showSeconds: false,
                        showMeridian: false,
                        defaultTime: false
                    });
                    handleFileupload();
                    var dates = $(".daterange").datepicker({
                        format: 'dd/mm/yyyy',
                        minDate: new Date()
                        /*onSelect: function (selectedDate) {
                            var option = this.id.indexOf("txtFromDateRange") == 0 ? "minDate" : "maxDate",
                                instance = $(this).data("datepicker"),
                                date = $.datepicker.parseDate(instance.settings.dateFormat || $.datepicker._defaults.dateFormat, selectedDate, instance.settings);
                            dates.not(this).datepicker("option", option, date);
                        }*/
                    });
                    $("pre").snippet("html", { style: "whitengrey", clipboard: "assets/scripts/ZeroClipboard.swf", showNum: false });
                }
            },
            error: function(xhr, textStatus) {
                $("#divTimelapses").html('');
                //$("#divLoadingTimelapse").show();
                $("#divLoadingTimelapse").html('You have not created any timelapses. <a href="#">Click</a> to create one.');
            }
        });
    };

    var getTimeLapseStatus = function(status) {
        if (status == 0)
            return "New";
        else if (status == 1)
            return "Recording";
        else if (status == 2)
            return "Failed";
        else if (status == 3)
            return "Scheduled";
        else if (status == 4)
            return "Expired";
        else if (status == 5)
            return "Camera Not Found";
        else if (status == 6)
            return "Paused";
    };

    var getVideoPlayer = function(cameraId, mp4, jpg, timelapseId, logoFile) {
        $.ajax({
            type: "POST",
            crossDomain: true,
            url: EvercamApi + "/cameras/" + cameraId + "/snapshots.json",
            beforeSend: function(xhrObj) {
                xhrObj.setRequestHeader("Authorization", localStorage.getItem("oAuthTokenType") + " " + localStorage.getItem("oAuthToken"));
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function(snapshot) {
                $.ajax({
                    type: "GET",
                    crossDomain: true,
                    url: EvercamApi + "/cameras/" + cameraId + "/snapshots/" + snapshot.snapshots[0].created_at + ".json",
                    beforeSend: function(xhrObj) {
                        xhrObj.setRequestHeader("Authorization", localStorage.getItem("oAuthTokenType") + " " + localStorage.getItem("oAuthToken"));
                    },
                    data: { with_data: true },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function(res) {
                        var html = '<video data-setup="{}" poster="' + res.snapshots[0].data + '" preload="none" controls="" class="video-js vjs-default-skin video-bg-width" id="video-control-' + timelapseId + '">';
                        html += '<source type="video/mp4" src="' + mp4 + '"></source>';
                        html += '</video>';

                        $("#divVideoContainer" + timelapseId).html(html);
                        console.log($('#video-control-' + timelapseId).width());
                    },
                    error: function(xhrc, ajaxOptionsc, thrownErrorc) {
                        loadDefaultImage(jpg, mp4, timelapseId, logoFile);
                    }
                });
            },
            error: function(xhrc, ajaxOptionsc, thrownErrorc) {
                loadDefaultImage(jpg, mp4, timelapseId, logoFile);
            }
        });
    };

    var loadDefaultImage = function(jpg, mp4, timelapseId, logoFile) {
        var img = new Image();
        img.onerror = function (evt) {
            var html = '<video data-setup="{}" poster="assets/img/timelapse.jpg" preload="none" controls="" class="video-js vjs-default-skin video-bg-width" id="video-control-' + timelapseId + '">';
            html += '<source type="video/mp4" src="' + mp4 + '"></source>';
            html += '</video>';
            $("#divVideoContainer" + timelapseId).html(html);
        };
        img.onload = function (evt) {
            var html = '<video data-setup="{}" poster="' + jpg + '" preload="none" controls="" class="video-js vjs-default-skin video-bg-width" id="video-control-' + timelapseId + '">';
            html += '<source type="video/mp4" src="' + mp4 + '"></source>';
            html += '</video>';
            $("#divVideoContainer" + timelapseId).html(html);
        };
        img.src = jpg;
    };

    var getDate = function() {
        var d = new Date();
        return FormatNumTo2(d.getDate()) + ' ' + getFullMonth(d.getMonth()) + ' ' + d.getFullYear() + ' ' + FormatNumTo2(d.getHours()) + ':' + FormatNumTo2(d.getMinutes());
    };

    var getHtml = function(data) {
        var cameraOptions = '';
        var timezone = '';
        var cameraOffline = false;
        var cameraName = '';
        var cams = JSON.parse(localStorage.getItem("timelapseCameras"));
        if (cams != null) {
            for (var i = 0; i < cams.cameras.length; i++) {
                var selected = '';
                if (cams.cameras[i].id == data.camera_id) {
                    timezone = cams.cameras[i].timezone;
                    cameraName = cams.cameras[i].name;
                    selected = 'selected';
                    if (cams.cameras[i].status == 'Offline')
                        cameraOffline = true;
                }
                //if (cams.cameras[i].status == "Active")
                cameraOptions += '<option value="' + cams.cameras[i].id + '" ' + selected + '>' + cams.cameras[i].name + '</option>';
            }
        }
        cams = JSON.parse(localStorage.getItem("sharedcameras"));
        if (cams != null && cams != undefined) {
            for (var i = 0; i < cams.cameras.length; i++) {
                var selected = '';
                if (cams.cameras[i].id == data.camera_id) {
                    timezone = cams.cameras[i].timezone;
                    selected = 'selected';
                    if (cams.cameras[i].status == 'Offline')
                        cameraOffline = true;
                }
                //if (cams.cameras[i].status == "Active")
                cameraOptions += '<option value="' + cams.cameras[i].id + '" ' + selected + '>' + cams.cameras[i].name + '</option>';
            }
        }

        var html = '    <div id="tab' + data.code + '">'; 
        html += '        <div class="header-bg">';
        html += '          <div class="row-fluid box-header-padding" data-val="' + data.id + '">';
        html += '              <div id="timelapseTitle' + data.id + '" class="timelapse-labelhd timelapse-label">' + data.title + '&nbsp;<span class="timelapse-camera-name">' + cameraName + '</span></div>';
        html += '              <div id="timelapseStatus' + data.code + '" class="timelapse-recordhd timelapse-label-status text-right">';
        //if (data.status == 1)
        //    html += '               <div class="timelapse-recording"></div>';
        //else if (data.status == 3 || data.status == 2 || data.status == 0)
        //    html += '               <div class="timelapse-paused"></div>';
        html += getTimeLapseStatus(data.status);
        html += '               </div>';
        html += '          </div>';
        //html += '          <br />';
        html += '          <div id="divContainer' + data.id + '" class="row-fluid box-content-padding hide">';
        html += '              <div id="divVideoContainer' + data.id + '" class="span6">';
        //html += '                  <iframe style="width:100%; border:0;height:360px;" src="loadvideo.html?id=' + data.camera_id + '&mp4=' + data.mp4_url + '&webm=' + data.webm_url + '&jpg=' + data.jpg_url + '" frameborder="0" allowfullscreen></iframe>';

        getVideoPlayer(data.camera_id, data.mp4_url, data.jpg_url, data.id, data.watermark_file);
        html += '                  <video data-setup="{}" preload="none" controls="" class="video-js vjs-default-skin video-bg-width" id="vde4b3u05e9y">';
        html += '                   <source type="video/mp4" src="' + data.mp4_url + '"></source>';
        html += '                  </video>';

        html += '              </div>';
        html += '              <div class="span6">';
        html += '                  <table class="tbl-tab" cellpadding="0" cellspacing="0">';
        html += '                      <thead>';
        html += '                          <tr>';
        html += '                              <th class="tbl-hd2"><a class="tab-a block' + data.id + ' selected-tab" href="javascript:;" data-ref="#stats' + data.id + '" data-val="' + data.id + '">Stats</a></th>';
        html += '                              <th class="tbl-hd1"><a class="tab-a block' + data.id + '" href="javascript:;" data-ref="#embedcode' + data.id + '" data-val="' + data.id + '">Embed Code</a></th>';
        html += '                              <th class="tbl-hd2"><a class="tab-a block' + data.id + '" href="javascript:;" data-ref="#option' + data.id + '" data-val="' + data.id + '">Options</a></th>';
        html += '                              <th class="tbl-hd3"><a class="tab-a2 block' + data.id + '" href="javascript:;" data-ref="#setting' + data.id + '" data-val="' + data.id + '">Settings&nbsp;&nbsp;<i class="icon-cog"></i></a></th>';
        html += '                          </tr>';
        html += '                       </thead>';
        html += '                       <tbody>';
        html += '                           <tr><td colspan="4" height="10px"></td></tr>';
        html += '                               <tr>';
        html += '                                   <td id="cameraCode' + data.id + '" colspan="4">';

        html += '                                       <div id="stats' + data.id + '" class="row-fluid active">';
        html += '                                         <div class="timelapse-content-box">';
        html += '                                           <table class="table table-full-width" style="margin-bottom:0px;">';
        html += '                                           <tr><td class="span2">Total Snapshots: </td><td class="span2" id="tdSnapCount' + data.code + '">' + data.snaps_count + '</td><td style="width:25px;text-align:right;" align="right"><img id="imgRef' + data.id + '" style="cursor:pointer;height:27px;" data-val="' + data.code + '" class="refreshStats" src="assets/img/refres-tile.png" alt="Refresh Stats" title="Refresh Stats"></td></tr>';
        html += '                                           <tr><td class="span2">Timelapse Length: </td><td class="span3" colspan="2"  id="tdDuration' + data.code + '">' + data.duration + '</td></tr>';
        html += '                                           <tr><td class="span2">MP4 File Size: </td><td class="span3" colspan="2"  id="tdFileSize' + data.code + '">' + data.file_size + '</td></tr>';
        html += '                                           <tr><td class="span2">Playback Speed: </td><td class="span3" colspan="2">' + data.fps + ' fps</td></tr>';
        html += '                                           <tr><td class="span2">Resolution: </td><td class="span3" colspan="2"  id="tdResolution' + data.code + '">' + (data.snaps_count == 0 ? '640x480' : data.resolution) + 'px</td></tr>';
        html += '                                           <tr><td class="span2">Created At: </td><td class="span3" colspan="2"  id="tdCreated' + data.code + '">' + (data.created_date) + '</td></tr>'; //data.snaps_count == 0 ? getDate() : 
        html += '                                           <tr><td class="span2">Last Snapshot At: </td><td class="span3" colspan="2"  id="tdLastSnapDate' + data.code + '">' + (data.snaps_count == 0 ? '---' : data.last_snap_date) + '</td></tr>';
        html += '                                           <tr><td class="span2">Camera Timezone: </td><td class="span3" colspan="2"  id="tdTimezone' + data.code + '">' + timezone + '</td></tr>';
        html += '                                           <tr><td class="span2">Timelapse Status: </td><td class="span3" colspan="2"  id="tdStatus' + data.code + '">' + (data.status_tag == null ? (data.status == 1 ? 'Now recording...' : '') : data.status_tag) + '</td></tr></table>';
        html += '                                       </div></div>';

        html += '                                       <div id="embedcode' + data.id + '" class="row-fluid hide">';
        html += '                                           <pre id="code' + data.code + '" class="pre-width">&lt;video class="video-js vjs-default-skin video-bg-width" controls preload="none"';
        html += ' poster="' + data.jpg_url + '" data-setup="{}"&gt;<br/>';
        html += '&lt;source src="' + data.mp4_url + '" type="video/mp4" /&gt;<br/>';
        //html += '&lt;source src="' + data.webm_url + '" type="video/webm" /&gt;<br/>';
        html += '&lt;/video&gt;</pre>';
        html += '                                       </div>';

        html += '                                       <div id="setting' + data.id + '" class="row-fluid hide">';
        html += '                                         <div class="timelapse-content-box padding14">';
        html += '                                           <form class="form-horizontal" style="margin-bottom:0px;">';
        html += '                                               <div class="control-group">';
        html += '                                                   <label class="controlLabel">Title</label>';
        html += '                                                   <div class="Controls">';
        html += '                                                       <input type="text" id="txtTitle' + data.id + '" value="' + data.title + '" class="span7 m-wrap white" placeholder="Timelapse Title">';
        html += '                                                   </div>';
        html += '                                               </div>';
        html += '                                               <div class="control-group">';
        html += '                                                   <label class="controlLabel">Interval</label>';
        html += '                                                   <div class="Controls">';
        html += '                                                       <select disabled id="ddlCameras' + data.id + '" class="span7 m-wrap hide"><option value="0">Select Camera</option>';
        html += cameraOptions;
        html += '                                                       </select>';
        html += '                                                       <select id="ddlIntervals' + data.id + '" class="span7 m-wrap">';
        html += '                                                           <option value="0" ' + (data.interval == "0" ? "selected" : "") + '>Select Interval</option>';
        html += '                                                           <option value="1" ' + (data.interval == "1" ? "selected" : "") + '>1 Frame Every 1 min</option>';
        html += '                                                           <option value="5" ' + (data.interval == "5" ? "selected" : "") + '>1 Frame Every 5 min</option>';
        html += '                                                           <option value="15" ' + (data.interval == "15" ? "selected" : "") + '>1 Frame Every 15 min</option>';
        html += '                                                           <option value="30" ' + (data.interval == "30" ? "selected" : "") + '>1 Frame Every 30 min</option>';
        html += '                                                           <option value="60" ' + (data.interval == "60" ? "selected" : "") + '>1 Frame Every 1 hour</option>';
        html += '                                                           <option value="360" ' + (data.interval == "360" ? "selected" : "") + '>1 Frame Every 6 hours</option>';
        html += '                                                           <option value="720" ' + (data.interval == "720" ? "selected" : "") + '>1 Frame Every 12 hours</option>';
        html += '                                                           <option value="1440" ' + (data.interval == "1440" ? "selected" : "") + '>1 Frame Every 24 hours</option>';
        html += '                                                       </select>';
        html += '                                                   </div>';
        html += '                                               </div>';

        html += '                                               <div class="control-group">';
        html += '                                                   <label class="controlLabel">Watermark Pos.</label>';
        html += '                                                   <div class="Controls">';
        html += '                                                       <select id="ddlLogoPositions' + data.id + '" class="span7 m-wrap">';
        html += '                                                           <option value="-1" ' + (data.watermark_position == "0" ? "selected" : "") + '>Select Logo Position</option>';
        html += '                                                           <option value="0" ' + (data.watermark_position == "1" ? "selected" : "") + '>Top Left</option>';
        html += '                                                           <option value="1" ' + (data.watermark_position == "5" ? "selected" : "") + '>Top Right</option>';
        html += '                                                           <option value="2" ' + (data.watermark_position == "15" ? "selected" : "") + '>Bottom Left</option>';
        html += '                                                           <option value="3" ' + (data.watermark_position == "30" ? "selected" : "") + '>Bottom Right</option>';
        html += '                                                       </select>';
        html += '                                                       <input type="hidden" id="txtLogoFile' + data.id + '" value="' + data.watermark_file + '"/>';
        html += '                                                   </div>';
        html += '                                               </div>';

        html += '                                               <div class="control-group">';
        html += '                                                   <label class="controlLabel">Watermark</label>';
        html += '                                                   <div class="Controls">';
        html += '                                                       ';
        html += '                                                       <select disabled id="ddlFrameRate' + data.id + '" class="span7 m-wrap hide">';
        html += '                                                           <option value="1" ' + (data.fps == "1" ? "selected" : "") + '>1 fps</option>';
        html += '                                                           <option value="2" ' + (data.fps == "2" ? "selected" : "") + '>2 fps</option>';
        html += '                                                           <option value="3" ' + (data.fps == "3" ? "selected" : "") + '>3 fps</option>';
        html += '                                                           <option value="4" ' + (data.fps == "4" ? "selected" : "") + '>4 fps</option>';
        html += '                                                           <option value="5" ' + (data.fps == "5" ? "selected" : "") + '>5 fps</option>';
        html += '                                                           <option value="6" ' + (data.fps == "6" ? "selected" : "") + '>6 fps</option>';
        html += '                                                           <option value="7" ' + (data.fps == "7" ? "selected" : "") + '>7 fps</option>';
        html += '                                                           <option value="8" ' + (data.fps == "8" ? "selected" : "") + '>8 fps</option>';
        html += '                                                           <option value="9" ' + (data.fps == "9" ? "selected" : "") + '>9 fps</option>';
        html += '                                                           <option value="10" ' + (data.fps == "10" ? "selected" : "") + '>10 fps</option>';
        html += '                                                           <option value="11" ' + (data.fps == "11" ? "selected" : "") + '>11 fps</option>';
        html += '                                                           <option value="12" ' + (data.fps == "12" ? "selected" : "") + '>12 fps</option>';
        html += '                                                           <option value="13" ' + (data.fps == "13" ? "selected" : "") + '>13 fps</option>';
        html += '                                                           <option value="14" ' + (data.fps == "14" ? "selected" : "") + '>14 fps</option>';
        html += '                                                           <option value="15" ' + (data.fps == "15" ? "selected" : "") + '>15 fps</option>';
        html += '                                                       </select>';
        html += '                                                   </div>';
        html += '                                               </div>';
        html += '                                               <div id="divRecording' + data.id + '" class="control-group1">';
        html += '                                                   <label class="controlLabel">Recording</label>';
        html += '                                                   <div class="Controls">';
        html += '                                                       <div class="row-fluid padding-top3">';
        html += '                                                           <label class="radio span2">';
        html += '                                                               <input id="chkRecordingTimelapse' + data.id + '" name="recording" type="radio" value="a" ' + (data.is_recording ? "checked" : "") + ' />&nbsp;Active';
        html += '                                                           </label>';
        html += '                                                           <label class="radio span2">';
        html += '                                                               <input id="chkPausedTimelapse' + data.id + '" name="recording" type="radio" value="p" ' + (!data.is_recording ? "checked" : "") + ' />&nbsp;Paused';
        html += '                                                           </label>';
        html += '                                                       </div>';
        html += '                                                   </div>';
        html += '                                               </div>';

        html += '                                               <div class="control-group1">';
        html += '                                                   <label class="controlLabel">Date Range</label>';
        html += '                                                   <div class="Controls">';
        html += '                                                       <div class="row-fluid padding-top3">';
        html += '                                                           <label class="radio span2">';
        html += '                                                               <input id="chkDateRangeAlways' + data.id + '" name="DateRange" type="radio" value="a" ' + (data.is_date_always ? 'checked' : "") + ' data-val="' + data.id + '"/>&nbsp;Always';
        html += '                                                           </label>';
        html += '                                                           <label class="radio span2">';
        html += '                                                               <input id="chkDateRange' + data.id + '" name="DateRange" type="radio" value="p" ' + (!data.is_date_always ? 'checked' : "") + ' data-val="' + data.id + '"/>&nbsp;Range';
        html += '                                                           </label>';
        html += '                                                       </div>';
        html += '                                                       <div id="divDateRange' + data.id + '" class="row-fluid- ' + (data.is_date_always ? "hide" : "") + '">';
        html += '                                                           <label class="controlLabel"></label>';
        html += '                                                           <input type="text" id="txtFromDateRange' + data.id + '" class="span1 small m-wrap white daterange" placeholder="From Date" readonly value="' + (!data.is_date_always ? data.from_date.substring(0, 10) : "") + '">&nbsp;';
        html += '                                                           <input type="text" id="txtToDateRange' + data.id + '" class="span1 small m-wrap white daterange" placeholder="To Date" readonly value="' + (!data.is_date_always ? data.to_date.substring(0, 10) : "") + '">';
        html += '                                                       </div>';
        html += '                                                   </div>';
        html += '                                               </div>';

        html += '                                               <div class="control-group">';
        html += '                                                   <label class="controlLabel">Time Range</label>';
        html += '                                                   <div class="Controls">';
        html += '                                                       <div class="row-fluid padding-top3">';
        html += '                                                           <label class="radio span2">';
        html += '                                                               <input id="chkTimeRangeAlways' + data.id + '" name="TimeRange" type="radio" value="a" ' + (data.is_time_always ? 'checked' : "") + ' data-val="' + data.id + '"/>&nbsp;Always';
        html += '                                                           </label>';
        html += '                                                           <label class="radio span2">';
        html += '                                                               <input id="chkTimeRange' + data.id + '" name="TimeRange" type="radio" value="p" ' + (!data.is_time_always ? 'checked' : "") + ' data-val="' + data.id + '"/>&nbsp;Range';
        html += '                                                           </label>';
        html += '                                                       </div>';
        html += '                                                       <div id="divTimeRange' + data.id + '" class="row-fluid- ' + (data.is_time_always ? "hide" : "") + '">';
        html += '                                                           <label class="controlLabel"></label>';
        html += '                                                           <input type="text" id="txtFromTimeRange' + data.id + '" class="span1 small m-wrap white timerange" placeholder="From Time" readonly value="' + (!data.is_time_always ? data.from_date.substring(11, 16) : "") + '">&nbsp;';
        html += '                                                           <input type="text" id="txtToTimeRange' + data.id + '" class="span1 small m-wrap white timerange" placeholder="To Time" readonly value="' + (!data.is_time_always ? data.to_date.substring(11, 16) : "") + '">';
        html += '                                                       </div>';
        html += '                                                   </div>';
        html += '                                               </div>';

        html += '                                               <div class="control-group" style="margin-bottom:5px;">';
        html += '                                                   <label class="controlLabel"></label>';
        html += '                                                   <div class="Controls">';
        html += '                                                       <button type="button" class="btn formButtonOk" data-val="' + data.id + '"><i class="icon-ok"></i> Save</button>';
        html += '                                                       <button data-val="' + data.id + '" type="button" class="btn formButtonCancel">Cancel</button>';
        html += '                                                       <input type="hidden" id="txtCameraCode' + data.id + '" value="' + data.code + '"/>';
        html += '                                                   </div>';
        html += '                                               </div>';
        html += '                                               <div class="control-group1">';
        html += '                                               <div class="Controls">';
        html += '                                                   <div id="divAlert' + data.id + '" class="alert alert-error hide" style="margin-bottom:2px;">';
        html += '                                                       <button class="close" data-dismiss="alert"></button>';
        html += '                                                       <span>Enter any email and password.</span>';
        html += '                                                   </div>';
        html += '                                               </div>';
        html += '                                           </div>';
        html += '                                       </form>';
        html += '                                   </div></div>';

        html += '                                       <div id="option' + data.id + '" class="row-fluid hide">';
        html += '                                         <div class="timelapse-content-box padding14">';
        html += '                                           <div style="padding-bottom:7px;"><ul class="list-style-none"><li style="width:5%;float:left;"><a target="_blank" rel="nofollow" title="' + data.title + '" href="' + data.mp4_url + '" download="' + data.title + '" class="commonLinks-icon" data-action="d" data-url="' + data.mp4_url + '" data-val="' + data.code + '"><img src="assets/img/download.png" /></a></li><li style="width: 95%; float: left; padding: 1px 0px 0px 4px;">&nbsp;<a target="_blank" rel="nofollow" title="' + data.title + '" href="' + data.mp4_url + '" download="' + data.title + '" class="commonLinks-icon" data-action="d" data-url="' + data.mp4_url + '" data-val="' + data.code + '">Download Video as MP4</a></li></ul><div class="clearfix"></div></div>';
        html += '                                           <div><ul class="list-style-none"><li style="width: 5%; float: left; padding-left: 3px;"><a href="javascript:;" class="commonLinks-icon" data-action="r" data-val="' + data.code + '"><img src="assets/img/delete.png" /></a></li><li style="width: 95%; float: left; padding: 2px 0px 0px 2px;">&nbsp;&nbsp;<a href="javascript:;" class="commonLinks-icon" data-action="r" data-val="' + data.code + '">Delete Timelapse</a></li></ul><div class="clearfix"></div></div>';
        html += '                                       </div></div>';

        html += '                                   </td>';
        html += '                               </tr>';
        html += '                           </tbody>';
        html += '                       </table>';
        html += '                   </div>';
        html += '               </div>';
        html += '           </div><br /></div>';
        //***********************************************************************************************************
        if (data.snaps_count == 0)
            setTimeout(function() {
                reloadStats(data.code, null);
            }, 1000 * 60);
        return html;
    };

    $(".tab-a2").live("click", function () {
        var clickedTab = $(this);
        var id = clickedTab.attr("data-val");
        $.ajax({
            type: "GET",
            crossDomain: true,
            url: timelapseApiUrl + "/" + id,
            beforeSend: function (xhrObj) {
                //xhrObj.setRequestHeader("Authorization", localStorage.getItem("oAuthTokenType") + " " + localStorage.getItem("oAuthToken"));
            },
            //data: { include_shared: true },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (res) {
                $.get('NewTimelapse.html', function (data) {
                    $("#newTimelapse").html(data);
                    $("#lnNewTimelapse").hide();
                    $("#lnNewTimelapseCol").show();
                    //Uncomment when fixed add camera functionality
                    //$("#lnNewCamera").show();
                    //$("#lnNewCameraCol").hide();
                    handleFileupload();
                    getCameras(true, res.camera_id);
                    
                    $("#newTimelapse").slideDown(500);
                    $("#txtTimelapseId").val(id);
                    $("#txtTitle0").val(res.title);
                    $("#ddlIntervals0").val(res.interval);
                    $("#ddlFrameRate0").val(res.fps);
                    $("#txtCameraCode0").val(res.code)
                    $("#ddlFrameRate0").attr("disabled", "disabled");
                    $("#ddlCameras0").attr("disabled", "disabled");
                    $("#ddlWatermarkPos0").val(res.watermark_position);
                    if (res.watermark_file != null && res.watermark_file != '') {
                        $("#txtLogoFile").val(res.watermark_file);
                        $("#imgWatermarkLogo").attr('src', res.watermark_file);
                        $("#imgWatermarkLogo").show();
                        $(".fileinput-button span").html("Change file...");
                    }
                    var fDt = new Date(res.from_date);
                    var tDt = new Date(res.to_date);
                    if (!res.is_time_always) {
                        $("#chkTimeRange0").attr("checked", "checked");
                        $("#divTimeRange0").slideDown();
                        //var d = new Date(res.from_date);
                        $("#txtFromTimeRange0").val(FormatNumTo2(fDt.getHours()) + ":" + FormatNumTo2(fDt.getMinutes()));//res.from_date.substring(ind, ind + 5)); //11+5=16
                        //d = new Date(res.to_date);
                        $("#txtToTimeRange0").val(FormatNumTo2(tDt.getHours()) + ":" + FormatNumTo2(tDt.getMinutes()));//res.to_date.substring(ind, ind + 5));
                    }
                    if (!res.is_date_always) {
                        $("#chkDateRange0").attr("checked", "checked");
                        $("#divDateRange0").slideDown();
                        $("#txtFromDateRange0").val(FormatNumTo2(fDt.getDate()) + "/" + FormatNumTo2(fDt.getMonth() + 1) + "/" + fDt.getFullYear());
                        $("#txtToDateRange0").val(FormatNumTo2(tDt.getDate()) + "/" + FormatNumTo2(tDt.getMonth() + 1) + "/" + tDt.getFullYear());
                    }

                    jQuery('html,body').animate({
                        scrollTop: jQuery('body').offset().top
                    }, 'slow');
                    $('.timerange').timepicker({
                        minuteStep: 1,
                        showSeconds: false,
                        showMeridian: false,
                        defaultTime: false
                    });
                    var dates = $(".daterange").datepicker({
                        format: 'dd/mm/yyyy',
                        minDate: new Date()
                    });
                });
            },
            error: function (xhrc, ajaxOptionsc, thrownErrorc) { }
        });
    });

    $(".uploadWatermark").live("click", function () {
        $("#fulDialog").dialog({
            overlay: { backgroundColor: "white", opacity: 0 },
            show: { effect: "explode", duration: 300 },
            //height: 480,
            width: 640,
            position: ['center', 20],
            title: "Upload Watermark Logo",
            modal: true,
            create: function (event, ui) {
                $("body").css({ overflow: 'hidden' })
            },
            beforeClose: function (event, ui) {
                $("body").css({ overflow: 'inherit' })
            },
            buttons: [
                {
                    text: "Close",
                    click: function () {
                        $(this).dialog("close");
                    }
                }]
        });
        $(".ui-dialog-titlebar-close").hide();
    });

    var handleFileupload = function() {

        // Initialize the jQuery File Upload widget:
        $('#fileupload').fileupload({
            // Uncomment the following to send cross-domain cookies:
            //xhrFields: {withCredentials: true},
            url: 'assets/plugins/jquery-file-upload/server/php/'
        });
    };

    $(".refreshStats").live("click", function () {
        var img = $(this);
        var code = img.attr("data-val");
        img.attr("src", "assets/img/5-1.gif");
        reloadStats(code, img);
    });

    var reloadStats = function(code, img) {
        $.ajax({
            type: 'GET',
            url: timelapseApiUrl + "/" + code + "/users/" + localStorage.getItem("timelapseUserId"),
            dataType: 'json',
            ContentType: 'application/json; charset=utf-8',
            timeout: 15000,
            success: function(data) {
                if (data.snaps_count == 0 && loopCount < 6) {
                    loopCount++;
                    $("#imgRef" + data.id).attr("src", "assets/img/refres-tile.png");
                    setTimeout(function() {
                        reloadStats(data.code, img);
                    }, 1000 * 60);
                } else if (loopCount > 5) {
                    loopCount = 1;
                    $("#imgRef" + data.id).attr("src", "assets/img/refres-tile.png");
                } else {
                    $("#tdSnapCount" + code).html(data.snaps_count);
                    $("#tdDuration" + code).html(data.duration);
                    $("#tdFileSize" + code).html(data.file_size);
                    $("#tdResolution" + code).html(data.resolution + "px");
                    $("#timelapseStatus" + code).html(getTimeLapseStatus(data.status));
                    if (data.snaps_count != 0) {
                        $("#tdLastSnapDate" + code).html(data.last_snap_date);
                        $("#tdCreated" + code).html(data.created_date);
                    }
                    if (data.status_tag != null)
                        $("#tdStatus" + code).html(data.status_tag);
                    $("#imgRef" + data.id).attr("src", "assets/img/refres-tile.png");
                }
            },
            error: function(xhr, textStatus) {
                if (img != null)
                    img.attr("src", "assets/img/refres-tile.png");
            }
        });
    };

    $(".box-header-padding").live("click", function () {
        var id = $(this).attr("data-val");
        if ($("#divContainer" + id).css("display") == "none")
            $("#divContainer" + id).slideDown(500);
        else
            $("#divContainer" + id).slideUp(500);
    });

    var handleTimelapsesCollapse = function () {
        $("#lnExpandTimelapses").bind("click", function () {
            $(".box-header-padding").each(function () {
                var id = $(this).attr("data-val");
                $("#divContainer" + id).slideDown(500);
            });
            $(this).hide();
            $("#lnCollapseTimelapses").show();
        });

        $("#lnCollapseTimelapses").bind("click", function () {
            $(".box-header-padding").each(function () {
                var id = $(this).attr("data-val");
                $("#divContainer" + id).slideUp(500);
            });
            $(this).hide();
            $("#lnExpandTimelapses").show();
        });
    };

    $(".tab-a").live("click", function () {
        var clickedTab = $(this);
        if (clickedTab.html() == 'Settings')
            return;
        var id = clickedTab.attr("data-val");
        $(".block" + id).removeClass("selected-tab");
        clickedTab.addClass("selected-tab");

        $("#cameraCode" + id + " div.active").fadeOut(100, function() {
            $(this).removeClass("active");
            $(clickedTab.attr("data-ref")).fadeIn(100, function() { $(this).addClass("active"); });
        });
    });

    $('.pre-width1').live("mouseup", function () {
        var self = this;
        setTimeout(function () { self.select(); }, 30);
    });

    var getCameras = function(reload, cameraId) {
        $.ajax({
            type: "GET",
            crossDomain: true,
            url: EvercamApi + "/users/" + localStorage.getItem("timelapseUserId") + "/cameras",
            beforeSend: function(xhrObj) {
                xhrObj.setRequestHeader("Authorization", localStorage.getItem("oAuthTokenType") + " " + localStorage.getItem("oAuthToken"));
            },
            data: { include_shared: true, thumbnail: true },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function(res) {
                localStorage.setItem("timelapseCameras", JSON.stringify(res));
                bindDropDown(reload, cameraId);
            },
            error: function(xhrc, ajaxOptionsc, thrownErrorc) {
            }
        });
    };

    var bindDropDown = function(reload, cameraId) {
        if (reload) {
            var cams = JSON.parse(localStorage.getItem("timelapseCameras"));
            for (var i = 0; i < cams.cameras.length; i++) {
                var css = 'onlinec';
                if (!cams.cameras[i].is_online)
                    css = 'offlinec';
                var isSelect = '';
                if (cameraId == cams.cameras[i].id) {
                    isSelect = 'selected="selected"';
                    loadSelectedCamImage(cameraId);
                }
                if (cams.cameras[i].is_public && cams.cameras[i].proxy_url != null && cams.cameras[i].proxy_url.jpg != undefined)
                    $("#ddlCameras0").append('<option class="' + css + '" data-val="' + cams.cameras[i].proxy_url.jpg + '" ' + isSelect + ' value="' + cams.cameras[i].id + '" >' + cams.cameras[i].name + '</option>');
                else if (!cams.cameras[i].is_public && cams.cameras[i].external != null && cams.cameras[i].external != undefined)
                    $("#ddlCameras0").append('<option class="' + css + '" data-val="' + cams.cameras[i].thumbnail + '" ' + isSelect + ' value="' + cams.cameras[i].id + '" >' + cams.cameras[i].name + '</option>');
            }
            $("#imgCamLoader").hide();
            $("#ddlCameras0").select2({
                placeholder: 'Select Camera',
                allowClear: true,
                formatResult: format,
                formatSelection: format,
                escapeMarkup: function(m) {
                    return m;
                }
            });
        } else
            getMyTimelapse();
    };

    var format = function(state) {
        if (!state.id) return state.text;
        if (state.id == "0") return state.text;
        //return "<img class='flag' src='assets/img/" + state.css + ".png'/>&nbsp;&nbsp;" + state.text;
        if (state.element[0].attributes[1].nodeValue == "null")
            return "<table style='width:100%;'><tr><td style='width:90%;'><img style='width:35px;height:30px;' class='flag' src='assets/img/cam-img-small.jpg'/>&nbsp;&nbsp;" + state.text + "</td><td style='width:10%;' align='right'>" + "<img style='margin-top: -6px;' class='flag' src='assets/img/" + state.css + ".png'/>" + "</td></tr></table>";
        else
            return "<table style='width:100%;'><tr><td style='width:90%;'><img style='width:35px;height:30px;' class='flag' src='" + state.element[0].attributes[1].nodeValue + "'/>&nbsp;&nbsp;" + state.text + "</td><td style='width:10%;' align='right'>" + "<img class='flag' style='margin-top: -6px;' src='assets/img/" + state.css + ".png'/>" + "</td></tr></table>";
    };

    var getUsersInfo = function() {
        $.ajax({
            type: "GET",
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            url: EvercamApi + "/users/" + localStorage.getItem("timelapseUserId") + ".json",
            beforeSend: function(xhrObj) {
                xhrObj.setRequestHeader("Authorization", localStorage.getItem("oAuthTokenType") + " " + localStorage.getItem("oAuthToken"));
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function(res) {
                loggedInUser = res.users[0];
                if (res.users[0].firstname == "" && res.users[0].lastname == "")
                    return;
                localStorage.setItem("timelapseUsername", res.users[0].firstname + " " + res.users[0].lastname);
                $("#displayUsername").html(res.users[0].firstname + " " + res.users[0].lastname)
            },
            error: function(xhrc, ajaxOptionsc, thrownErrorc) {
            }
        });
    };

    $('.commonLinks-icon').live('click', function (e) {
        var code = $(this).attr("data-val");
        var action = $(this).attr("data-action");
        
        if (action == 'r') {
            jConfirm("Are you sure? ", "Delete Timelapse", function (result) {
                if(result) RemoveTimelapse(code);
            });
        }
        else if (action == 'e') {
            if ($("#code" + code).css("display") == 'none')
                $("#code" + code).slideDown(1000);
            else
                $("#code" + code).slideUp(1000);
        }
        else if (action == 'd1') {
            SaveToDisk($(this).attr("data-url"), code);
        }
    });

    function SaveToDisk(fileURL, fileName) {
        // for non-IE
        if (!window.ActiveXObject) {
            var save = document.createElement('a');
            save.href = fileURL;
            save.target = '_blank';
            save.download = fileName || 'unknown';

            var event = document.createEvent('Event');
            event.initEvent('click', true, true);
            save.dispatchEvent(event);
            (window.URL || window.webkitURL).revokeObjectURL(save.href);
        }// for IE
        else if (!!window.ActiveXObject && document.execCommand) {
            var _window = window.open(fileURL, '_blank');
            _window.document.close();
            _window.document.execCommand('SaveAs', true, fileName || fileURL);
            _window.close();
        }
    }

    var RemoveTimelapse = function(code) {
        $("#tab" + code).fadeOut(1000, function() {
            $.ajax({
                type: "DELETE",
                url: timelapseApiUrl + "/" + code + "/users/" + localStorage.getItem("timelapseUserId"),
                /*beforeSend: function (xhrObj) {
                    xhrObj.setRequestHeader("Authorization", "Basic " + localStorage.getItem("oAuthToken"));
                },*/
                contentType: "application/x-www-form-urlencoded",//"text/plain; charset=utf-8",
                dataType: "json",
                success: function(res) {
                    $("#tab" + code).remove();
                    if ($("#divTimelapses").html() == "") {
                        $("#divTimelapses").html('');
                        $("#divLoadingTimelapse").html('You have not created any timelapses. <a href="javascript:;" class="newTimelapse">Click</a> to create one.');
                        $("#divLoadingTimelapse").slideDown();
                    }
                },
                error: function(xhrc, ajaxOptionsc, thrownErrorc) {
                }
            });
        });
    };

    var getUserLocalIp = function() {
        try {
            if (window.XMLHttpRequest) xmlhttp = new XMLHttpRequest();
            else xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");

            xmlhttp.open("GET", "http://api.hostip.info/get_html.php", false);
            xmlhttp.send();

            hostipInfo = xmlhttp.responseText.split("\n");

            for (i = 0; hostipInfo.length >= i; i++) {
                var ipAddress = hostipInfo[i].split(":");
                if (ipAddress[0] == "IP") return $("#user_local_Ip").val(ipAddress[1]);
            }
        } catch(e) {
        }
    };

    var handleFancyBox = function() {
        $('.fancybox-media')
            .attr('rel', 'media-gallery')
            .fancybox({
                openEffect: 'none',
                closeEffect: 'none',
                prevEffect: 'none',
                nextEffect: 'none',

                arrows: false,
                helpers: {
                    media: { },
                    buttons: { }
                }
            });
    };

    $("#ddlCameras0").live("change", function () {
        var cameraId = $(this).val();
        //$("#imgCamStatus").hide();
        loadSelectedCamImage(cameraId);
    });

    var loadSelectedCamImage = function (cameraId) {
        $("#imgPreview").hide();
        $("#imgPreviewLoader").show();
        $("#imgPreviewLoader").attr('src', 'assets/img/ajaxloader.gif');
        //$("#imgPreview").attr('src', EvercamProxy + cameraId + ".jpg");
        //$("#imgPreview").show();
        //$("#imgPreviewLoader").hide();
        //$("#imgPreviewLoader").attr('src', 'assets/img/cam-img.jpg');

        $.ajax({
            type: "GET",
            crossDomain: true,
            url: EvercamApi + "/cameras/" + cameraId + "/live.json",
            beforeSend: function (xhrObj) {
                xhrObj.setRequestHeader("Authorization", localStorage.getItem("oAuthTokenType") + " " + localStorage.getItem("oAuthToken"));
            },
            data: { with_data: true },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (res) {
                if (res.data == null || res.data == undefined) {
                    $("#imgPreviewLoader").attr('src', 'assets/img/cam-img.jpg');
                } else {
                    $("#imgPreview").attr('src', res.data);
                    $("#imgPreview").show();
                    $("#imgPreviewLoader").hide();
                    $("#imgPreviewLoader").attr('src', 'assets/img/cam-img.jpg');
                }
            },
            error: function (xhrc, ajaxOptionsc, thrownErrorc) {
                $("#imgPreviewLoader").attr('src', 'assets/img/cam-img.jpg');
            }
        });
    };

    var redirectHome = function() {
        $(".showlist").bind("click", function() {
            window.location = 'index.html';
        });
    };

    return {
        
        init: function () {
            handleFileupload();
            redirectHome();
            getUserLocalIp();
            handleLoginSection();
            handleTimelapsesCollapse();
            handleNewTimelapse();
            handleLogout();
            handleMyTimelapse();
            handleFancyBox();
        }

    };
}();