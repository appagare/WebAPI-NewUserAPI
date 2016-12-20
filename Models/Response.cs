using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewUserAPI.Models
{
    public class ErrorResponse
    {
        public int status { get; set; }
        public int code { get; set; }
        public string property { get; set; }
        public string message { get; set; }
        public string developerMessage { get; set; }
        public string moreInfo { get; set; }
    }

    public class UserResponse
    {
        public int ParentId { get; set; } /* parent accountid */
        public int AccountId { get; set; } /* customer accountid */
        public int UserId { get; set; } /* userid */

    }
}