/*!
 * Evercam JavaScript Library v0.2.0
 * http://www.evercam.io/
 *
 * Copyright 2014 Evercam.io
 * Released under the MIT license
 *
 * Date: 2014-02-27
 */
(function(window, $) {

  "use strict";

  function url(base, ext) {
    if (ext === undefined) {
      ext = '';
    } else {
      ext = '/' + ext;
    }
    return window.Evercam.apiUrl + '/' + base + ext;
  }

  window.Evercam = {

    apiUrl: 'https://dashboard.evercam.io/v1',
    refresh: 0,

    setApiUrl: function(url) {
      this.apiUrl = url;
    },

    setBasicAuth: function(username, password,token) {
      $.ajaxSetup({
        headers: {
            'Authorization': 'bearer ' + token//'Basic ' + base64Encode(username + ":" + password)
        }
      });
    },

    setOAuth2: function(token) {
      $.ajaxSetup({
        headers: {
          'Authorization': 'Bearer ' + token
        }
      });
    },

    Model: {
      base: 'models',

      all: function() {
        return $.getJSON(url(this.base)).then(function(data) {
          return data.vendors;
        });
      },

      by_vendor: function (vid) {
        return $.getJSON(url(this.base, vid)).then(function(data) {
          return data.vendors[0];
        });
      },

      by_model: function (vid, mid) {
        return $.getJSON(url(this.base, vid + '/' + mid)).then(function(data) {
          return data.models[0];
        });
      }
    },

    Vendor: {
      base: 'vendors',

      all: function () {
        return $.getJSON(url(this.base)).then(function(data) {
          return data.vendors;
        });
      },

      by_mac: function (mac) {
        return $.getJSON(url(this.base, mac)).then(function(data) {
          return data.vendors;
        });
      }
    },

    Camera: function (name) {
      this.data = null;
      this.timestamp = 0;
      this.name = name;
    },

    User: function (login) {
      this.data = null;
      this.login = login;
    }

  };

  // USER DEFINITION
  // ====

  Evercam.User.base = 'users';

  Evercam.User.create = function (params) {
    return $.post(url(this.base), params).then(function(data) {
      return data.users[0];
    });
  };

  Evercam.User.cameras = function (uid) {
    return $.getJSON(url(this.base, uid + '/cameras')).then(function(data) {
      return data.cameras;
    });
  };

  Evercam.User.by_login = function (login) {
    var user = new Evercam.User(login);
    return $.getJSON(url(this.base, login)).then(function (data) {
      user.data = data.users[0];
      return user;
    });
  };

  Evercam.User.prototype.update = function (fields) {
    var self = this,
      newdata = self.data;
    if (fields !== undefined) {
      newdata = {};
      $.each(fields, function(i, val) {
        newdata[val] = self.data[val];
      });
    }
    return $.ajax({
      type: 'PATCH',
      url: url(self.base, self.login),
      dataType: 'json',
      data: newdata
    });
  };

  // CAMERA DEFINITION
  // =======================

  Evercam.Camera.base = 'cameras';

  Evercam.Camera.by_id = function (id) {
    var camera = new Evercam.Camera(id);
    return $.getJSON(url(this.base, id)).then(function (data) {
      camera.data = data.cameras[0];
      return camera;
    });
  };

  Evercam.Camera.remove = function (id) {
    return $.ajax({
      type: 'DELETE',
      url: url(this.base, id),
      dataType: 'json',
      data: {}
    });
  };

  Evercam.Camera.create = function (params) {
    return $.ajax({
      type: 'POST',
      url: url(this.base),
      dataType: 'json',
      data: params
    });
  };

  Evercam.Camera.snapshotUrl = function (id) {
    return url(this.base, id + '/snapshot.jpg');
  };

  Evercam.Camera.prototype.update = function (fields) {
    var self = this,
      newdata = self.data;
    if (fields !== undefined) {
      newdata = {};
      $.each(fields, function(i, val) {
        newdata[val] = self.data[val];
      });
    }
    return $.ajax({
      type: 'PATCH',
      url: url(this.base, self.data.id),
      dataType: 'json',
      data: newdata
    });
  };

  function base64Encode(str) {
    var CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    var out = "", i = 0, len = str.length, c1, c2, c3;
    while (i < len) {
      c1 = str.charCodeAt(i++) & 0xff;
      if (i == len) {
        out += CHARS.charAt(c1 >> 2);
        out += CHARS.charAt((c1 & 0x3) << 4);
        out += "==";
        break;
      }
      c2 = str.charCodeAt(i++);
      if (i == len) {
        out += CHARS.charAt(c1 >> 2);
        out += CHARS.charAt(((c1 & 0x3)<< 4) | ((c2 & 0xF0) >> 4));
        out += CHARS.charAt((c2 & 0xF) << 2);
        out += "=";
        break;
      }
      c3 = str.charCodeAt(i++);
      out += CHARS.charAt(c1 >> 2);
      out += CHARS.charAt(((c1 & 0x3) << 4) | ((c2 & 0xF0) >> 4));
      out += CHARS.charAt(((c2 & 0xF) << 2) | ((c3 & 0xC0) >> 6));
      out += CHARS.charAt(c3 & 0x3F);
    }
    return out;
  }

  // EVERCAM PLUGIN DEFINITION
  // =======================

  $.fn.camera = function(opts) {

    // override defaults
    var settings = $.extend({
        refresh: Evercam.refresh
      }, opts),
      $img = $(this),
      watcher = null,
      url = Evercam.Camera.snapshotUrl(settings.name),
      updateImage = function() {
        if (settings.refresh > 0) {
          watcher = setTimeout(updateImage, settings.refresh);
        }
        $("<img />").attr('src', url)
          .load(function() {
            if (!this.complete || this.naturalWidth === undefined || this.naturalWidth === 0) {
              console.log('broken image!');
            } else {
              $img.attr('src', url);
            }
          });
      };

    // check img auto refresh
    $img.on('abort', function() {
      clearTimeout(watcher);
    });

    updateImage();

    return this;

  };

  function enableEvercam() {
    $.each($('img[evercam]'), function(i, e) {
      var $img = $(e),
        name = $img.attr('evercam'),
        refresh = Number($img.attr('refresh'));

      // ensure number
      if (isNaN(refresh)) {
        refresh = Evercam.refresh;
      }

      if (name === undefined) {
        throw "Camera name can't be empty";
      }

      $img.camera({
        name: name,
        refresh: refresh
      });
    });
  }

  $(window).load(enableEvercam);

}(window, jQuery));
