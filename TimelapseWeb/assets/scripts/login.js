var Login = function () {
    
    var createUserState = false;
    var exitRegister = false;
    var EvercamApi = "https://api.evercam.io/v1";
        
    var onBodyLoad = function () {
        if (sessionStorage.getItem("oAuthToken") != null && sessionStorage.getItem("oAuthToken") != undefined)
            window.location = 'index.html';
        $("#country").select2({
            placeholder: '<i class="icon-map-marker"></i>&nbsp;Select a Country',
            allowClear: true,
            formatResult: format,
            formatSelection: format,
            escapeMarkup: function (m) {
                return m;
            }
        });
    }

    var format = function (state) {
        if (!state.id) return state.text; // optgroup
        return "<img class='flag' src='assets/img/flags/" + state.id.toLowerCase() + ".png'/>&nbsp;&nbsp;" + state.text;
    }

    var clearForm = function () {
        $("#first_name").val("");
        $("#last_name").val("");
        $("#user_name").val("");
        $("#user_email").val("");
        $("#country").val("");
        $('.alert-error').slideUp();
        $('#LoaderRegister').hide();
        $(".font-size16").removeClass("font-color-red");
    }

    var handleRegisterUser = function () {
        $(".register_user").bind("click", function () {
            if (exitRegister) return;
            if (!createUserState) {
                createUserState = true;
                $("#divRegister").slideDown(900, function () {
                    $("#spnCamcel").fadeIn();
                });
            } else {
                if ($("#first_name").val() == "" && $("#last_name").val() == "" && $("#user_name").val() == "" && $("#user_email").val() == "" && $("#password").val() == "" && $("#country").val() == "") {
                    $('.alert-error').slideDown();
                    $('.alert-error span').html('Please enter required fields.');
                    $(".font-size16").addClass("font-color-red");
                    return
                }
                else {
                    $(".font-size16").removeClass("font-color-red");
                    var isReturn = false;

                    if ($("#first_name").val() == "") {
                        $('.alert-error').slideDown();
                        $('.alert-error span').html('Please enter required field: First Name.');
                        $("#spnReqFN").addClass("font-color-red");
                        isReturn = true;
                    }
                    if ($("#last_name").val() == "") {
                        $('.alert-error').slideDown();
                        $('.alert-error span').html('Please enter required field: Last Name.');
                        $("#spnReqLN").addClass("font-color-red");
                        isReturn = true;
                    }
                    if ($("#user_name").val() == "") {
                        $('.alert-error').slideDown();
                        $('.alert-error span').html('Please enter required field: Username.');
                        $("#spnReqUN").addClass("font-color-red");
                        isReturn = true;
                    }
                    if ($("#user_email").val() == "") {
                        $('.alert-error').slideDown();
                        $('.alert-error span').html('Please enter required field: Email.');
                        $("#spnReqEmail").addClass("font-color-red");
                        isReturn = true;
                    }
                    if ($("#password").val() == "") {
                        $('.alert-error').slideDown();
                        $('.alert-error span').html('Please enter required field: Password.');
                        $("#spnReqPass").addClass("font-color-red");
                        isReturn = true;
                    }
                    if ($("#country").val() == "") {
                        $('.alert-error').slideDown();
                        $('.alert-error span').html('Please enter required field: Country.');
                        $("#spnReqCountry").addClass("font-color-red");
                        isReturn = true;
                    }
                    if (isReturn) {
                        $('.alert-error').slideDown();
                        $('.alert-error span').html('Please enter required fields.');
                        return;
                    }

                    $(".font-size16").removeClass("font-color-red");
                    $('.alert-error').slideUp();
                    $('#LoaderRegister').css({
                        position: 'absolute',
                        top: ($('#divRegister').height() / 2) - 22,
                        'z-index': '5',
                        left: ($('#divRegister').width() / 2) - 22,
                    });
                    $('#LoaderRegister').show();

                    $.ajax({
                        type: 'POST',
                        crossDomain: true,
                        url: EvercamApi + '/users.json',
                        data: { firstname: $("#first_name").val(), lastname: $("#last_name").val(), username: $("#user_name").val(), email: $("#user_email").val(), password: $("#password").val(), country: $("#country").val().toLowerCase() },
                        dataType: 'json',
                        ContentType: 'application/json; charset=utf-8',
                        success: function (res) {
                            $('.alert-error').slideUp();
                            createUserState = false;
                            exitRegister = true;
                            $("#spnCamcel").fadeOut();
                            $('.register_user img').attr('src', 'assets/img/ecupg.png');
                            $("#divRegister").slideUp(900, function () {
                                $("#divSuccess").html('Thank you for registering, <b>' + res.users[0].username + '</b>. An email has been dispatched to <b>' + res.users[0].email + '</b> with details on how to activate your account.');
                                $("#divSuccess").fadeIn();
                                clearForm();
                            });
                        },
                        error: function (xhr, textStatus) {
                            //var msg = '<ul>';
                            //for (var i = 0; i < xhr.responseJSON.message.length; i++)
                            //    msg += '<li>' + xhr.responseJSON.message[i] + '</li>';
                            $('.alert-error').slideDown();
                            $('.alert-error span').html(xhr.responseJSON.message + ' ' + xhr.responseJSON.context);
                            $('#LoaderRegister').hide();
                        }
                    });
                }
            }
        });

        $(".cancel_register").bind("click", function () {
            createUserState = false;
            $("#spnCamcel").fadeOut();
            $("#divRegister").slideUp(900, function () {
                clearForm();
            });
        });
    }

    var getImageFromCamera = function () {
        $.ajax({
            type: 'GET',
            url: 'https://dashboard.evercam.io/v1/cameras/azharmalik354e6fca21f79/snapshot.jpg.json',
            beforeSend: function (xhrObj) {
                xhrObj.setRequestHeader("Authorization", sessionStorage.getItem("oAuthTokenType") + " 6731517ca0ff30248291cdf54392b9ad");
            },
            dataType: 'json',
            ContentType: 'application/x-www-form-urlencoded',
            success: function (res) {
                
            },
            error: function (xhr, textStatus) {

            }
        });
    }

    return {
        //main function to initiate the module
        init: function () {
            onBodyLoad();
            handleRegisterUser();
            //getImageFromCamera();
        }

    };

}();