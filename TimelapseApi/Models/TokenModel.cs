using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimelapseApi.Models
{
    public class TokenUserModel
    {
        /// <example>4918bd3d77b7d257f564fcf8ebbd3839</example>
        public string access_token { get; set; }
        /// <example>c4203c3e</example>
        public string audience { get; set; }
        /// <example>evercam-user-id</example>
        public string userid { get; set; }
        /// <example>3599</example>
        public string expires_in { get; set; }
    }

    public class TokenUrlModel
    {
        /// <example>https://api_url/oauth2/tokeninfo?access_token={access_token}</example>
        public string token_endpoint { get; set; }
    }
}