function validateEmail(email) {
    var reg = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/;
    var address = document.getElementById(email).value;
    //remove all white space from value before validating 
    var addresstrimed = address.replace(/ /gi, '');
    if (reg.test(addresstrimed) == false) {
        return false;
    }
    else
        return true;
}

///////////////////////////////////////////////////////////////////
function Querystring(qs) { // optionally pass a querystring to parse
    this.params = {};

    if (qs == null) qs = location.search.substring(1, location.search.length);
    if (qs.length == 0) return;

    // Turn <plus> back to <space>
    // See: http://www.w3.org/TR/REC-html40/interact/forms.html#h-17.13.4.1
    qs = qs.replace(/\+/g, ' ');
    var args = qs.split('&'); // parse out name/value pairs separated via &

    // split out each name=value pair
    for (var i = 0; i < args.length; i++) {
        var pair = args[i].split('=');
        var name = decodeURIComponent(pair[0]);

        var value = (pair.length == 2)
			? decodeURIComponent(pair[1])
			: name;

        this.params[name] = value;
    }
}

Querystring.prototype.get = function (key, default_) {
    var value = this.params[key];
    return (value != null) ? value : default_;
}

Querystring.prototype.contains = function (key) {
    var value = this.params[key];
    return (value != null);
}

//QUERY STRING

function DeleteConform() {
    if (!confirm("Are you sure to Delete? "))
        return false;
}

function FormatNumTo2(n) {
    if (n < 10)
        return "0" + n;
    else
        return n;
}

function getFullMonth(month) {
    var fmonth = new Array(12);
    fmonth[0] = "January";
    fmonth[1] = "February";
    fmonth[2] = "March";
    fmonth[3] = "April";
    fmonth[4] = "May";
    fmonth[5] = "June";
    fmonth[6] = "July";
    fmonth[7] = "August";
    fmonth[8] = "September";
    fmonth[9] = "October";
    fmonth[10] = "November";
    fmonth[11] = "December";
    return fmonth[month];
}