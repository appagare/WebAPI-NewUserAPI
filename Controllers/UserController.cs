using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NewUserAPI.Models;
using System.Configuration;


namespace NewUserAPI.Controllers
{
    public class UserController : ApiController
    {
        // GET: api/User  
        public IHttpActionResult Get()
        {



            ErrorResponse er = new Models.ErrorResponse();
            er.code = 400;
            er.developerMessage = "Only POST method allowed. Permanent failure. Do not retry.";
            er.message = er.developerMessage;
            er.moreInfo = "n.a.";
            er.property = "n.a.";
            er.status = 400;
            return Content(HttpStatusCode.BadRequest, er);
        }

        //// GET: api/User/5
        //public ErrorResponse Get(int id)
        //{
        //    // future; add proper authentication and allow user to request cart info
            
        //    // only return
        //    ErrorResponse er = new Models.ErrorResponse();
        //    er.code = 400;
        //    er.developerMessage = "Not presently implemented.Do not retry.";
        //    er.message = er.developerMessage;
        //    er.moreInfo = "";
        //    er.property = "";
        //    er.status = 403;
        //    return er;
        //}
        

        // POST: api/User
        public IHttpActionResult Post([FromBody]UserRequest req)
        {

            if (req.pid < 0)
            {
                ErrorResponse er = new Models.ErrorResponse();
                er.code = 400;
                er.developerMessage = "Missing parent ID (pid) parameter. Permanent failure. Do not retry.";
                er.property = "pid";
                er.status = 400;
                return Content(HttpStatusCode.BadRequest, er);
            }

            User u = new User(req.pid, ConfigurationManager.AppSettings["AccountStoreKey"]);

            /* required fields */
            u.ParentId = req.pid;
            u.PersonFirstname = req.Firstname;
            u.PersonLastname = req.Lastname;
            u.EmailAddress = req.Email;
            u.Username = req.Username;
            u.SetPassword(req.Password);

            
            if (!String.IsNullOrEmpty(req.CompanyName))
            {
                u.AccountName = req.CompanyName;
            }
            u.BillingAddress1 = req.BillingAddress1;
            u.BillingAddress2 = req.BillingAddress2;
            u.BillingCity  = req.BillingCity;
            u.BillingStateCode = req.BillingStateCode;
            u.BillingPostalCode = req.BillingPostalCode;
            u.BillingCountryCode = req.BillingCountryCode;

            u.ShippingAddress1 = req.ShippingAddress1;
            u.ShippingAddress2 = req.ShippingAddress2;
            u.ShippingCity = req.ShippingCity;
            u.ShippingStateCode = req.ShippingStateCode;
            u.ShippingPostalCode = req.ShippingPostalCode;
            u.ShippingCountryCode = req.ShippingCountryCode;

            u.PhoneNumber = req.PhoneNumber;
            u.Url = req.Url;
            
            if (u.Create() == true)
            {
                return Content(HttpStatusCode.OK, u.UserResponse);
            }
            else
            {
                // return the user model's error response
                return Content(HttpStatusCode.BadRequest, u.ErrorResponse);
            }
        }

        // PUT: api/User/5
        //public void Put(int id, [FromBody]string value)
        //{
        //    /* not implemented*/
        //    ErrorResponse er = new Models.ErrorResponse();
        //    er.code = 400;
        //    er.developerMessage = "Not implemented. Permanent failure. Do not retry.";
        //    er.message = er.developerMessage;
        //    er.moreInfo = "n.a.";
        //    er.property = "n.a.";
        //    er.status = 403;
        //    return Content(HttpStatusCode.BadRequest, er);
        //}

        //// DELETE: api/AIMUserAPI/5
        //public void Delete(int id)
        //{
        //    /* not implemented*/
        //    ErrorResponse er = new Models.ErrorResponse();
        //    er.code = 400;
        //    er.developerMessage = "Not implemented. Permanent failure. Do not retry.";
        //    er.message = er.developerMessage;
        //    er.moreInfo = "n.a.";
        //    er.property = "n.a.";
        //    er.status = 403;
        //    return Content(HttpStatusCode.BadRequest, er);
        //}
    }
}
