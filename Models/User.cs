using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace NewUserAPI.Models
{
    public class User
    {

        
        /* Account fields */
        public int ParentId { get; set; } /* parent accountid */
        public string TypeCode { get; set; } /* must be CUSTOMER */
        public int AccountStatusID { get; set; } /* must be 1 */
        public string AccountName { get; set; } /* company or first+last */
        [Url]
        public string Url { get; set; } /* URL */
        public DateTime? InActiveDate { get; set; }
        public string CustomTypeCode { get; set; } /* API */

        /* billing address fields */
        public string BillingAddressTypeCode { get; set; } /* BILLING */
        public string BillingAddress1 { get; set; } /*  */
        public string BillingAddress2 { get; set; } /*  */
        public string BillingCity { get; set; } /*  */
        public string BillingStateCode { get; set; } /*  */
        public string BillingPostalCode { get; set; } /*  */
        public string BillingCountryCode { get; set; } /*  */

        /* optional phone */
        public string PhoneTypeCode { get; set; } /* _BLANK */
        [Phone]
        public string PhoneNumber { get; set; } /*  */
        public string Ext { get; set; } /*  */

        /* optional email */
        public string EmailTypeCode { get; set; } /* _BLANK */
        public string EmailName { get; set; } /* person first+last */
        [EmailAddress]
        public string EmailAddress { get; set; } /*  */

        /* Person fields */
        public string PersonTypeCode { get; set; } /* _BLANK */
        public string PersonPrefix { get; set; } /*  */
        public string PersonFirstname { get; set; } /*  */
        public string PersonMI { get; set; } /*  */
        public string PersonLastname { get; set; } /*  */
        public string PersonSuffix { get; set; } /*  */

        /* shipping address fields */
        public string ShippingAddressTypeCode { get; set; } /* 'SHIPPING' or '' (i.e.- not _BLANK but actual '') means skip shipping address */
        public string ShippingAddress1 { get; set; } /*  */
        public string ShippingAddress2 { get; set; } /*  */
        public string ShippingCity { get; set; } /*  */
        public string ShippingStateCode { get; set; } /*  */
        public string ShippingPostalCode { get; set; } /*  */
        public string ShippingCountryCode { get; set; } /*  */

        public string Username { get; set; } /*  */

        // error response
        private ErrorResponse er = new ErrorResponse(); 
        public ErrorResponse ErrorResponse {
            get { return er; }
        }

        // success response
        private UserResponse ur = new UserResponse();
        public UserResponse UserResponse
        {
            get { return ur; }
        }

        //public string encryptedPassword { get; set; } /*  */
        //public string hashedPassword { get; set; } /*  */
        private string encryptedPassword = "";
        private string hashedPassword = "";
        private string connectString = "";

        /* constructors */
        public User(int MerchantId, string ConnectString)
        {
            ParentId = MerchantId;
            connectString = decryptPassword(ConnectString);
        }
        private string decryptPassword(string ConnectString)
        {
            int intStart = 0;
            int intEnd = 0;
            string EncryptedString = "";
            string DecryptedString = "";

            //parse the encrypted password from  the connection string, then decrypt it 
            //and update the connection string
            intStart = ConnectString.IndexOf("Password=", StringComparison.CurrentCultureIgnoreCase);

            if (intStart > 1)
            {
                intStart = intStart + "Password=".Length;
                intEnd = ConnectString.IndexOf(";", intStart);  
                if (intStart < intEnd)
                {
                    //ok to decrypt
                    EncryptedString = ConnectString.Substring(intStart, intEnd - intStart);
                    // new style
                    ASi.UtilityHelper.Utilities ut = new ASi.UtilityHelper.Utilities();
                    DecryptedString = ut.Decrypt(EncryptedString);
                    ConnectString = ConnectString.Replace(EncryptedString, DecryptedString);
                } 
                else
                {
                    throw new Exception("Invalid connection string (2)"); 
                }
            }
            else
            {
                throw new Exception("Invalid connection string (1)");
            }
            return ConnectString;
        }

        public void SetPassword(string password)
        {
            /* 
             * password encryption here 
             * should use SecureString but can't without changing GenerateHash and Encrypt methods of UtilityHelper
             */
            ASi.UtilityHelper.Utilities ut = new ASi.UtilityHelper.Utilities();
            hashedPassword = ut.GenerateHash(password);
            encryptedPassword = ut.Encrypt(password);
            
        }

        public bool Create()
        {

            bool ret = false;

            // initial responses
            ur.AccountId = 0;
            er.code = 400;
            er.status = 400;
            er.property = "";
            er.message = "";
            er.developerMessage = "";
            
            // create user
            try
            {
                if (!checkProperties())
                {
                    throw new Exception(er.message);
                }

                /* hard-coded fields */
                AccountStatusID = 1;
                TypeCode = Constants.ACCOUNT_TYPECODE_CUSTOMER ;
                CustomTypeCode = Constants.CUSTOM_TYPECODE_API;
                BillingAddressTypeCode = Constants.ADDRESS_TYPECODE_BILLING;
                PhoneTypeCode = Constants.BLANK;
                EmailTypeCode = Constants.BLANK;
                PersonTypeCode = Constants.BLANK;
                ShippingAddressTypeCode = Constants.ADDRESS_TYPECODE_SHIPPING;

                /* derived values */
                EmailName = PersonFirstname + " " + PersonLastname;
                if (String.IsNullOrEmpty(AccountName))
                {
                    AccountName = EmailName;
                }
                setAddressProperties();


                // check username
                try
                {
                    //SqlParameter prm = new SqlParameter("@ReturnVal",  System.Data.SqlDbType.Int);
                    //prm.Direction = System.Data.ParameterDirection.ReturnValue;

                    //ASi.DataAccess.SqlHelper.ExecuteNonQuery(connectString, "UsernameExists",
                    //new SqlParameter("@AccountID", ParentId),
                    //new SqlParameter("@Username", Username),
                    //new SqlParameter("@UserID", 0),
                    //prm);

                    //int id = (int)prm.Value;

                    int id = 0;
                    using (SqlConnection conn = new SqlConnection(connectString))
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "UsernameExists";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AccountID", ParentId);
                        cmd.Parameters.AddWithValue("@Username", Username);
                        cmd.Parameters.AddWithValue("@UserID", 0);

                        var prm = cmd.Parameters.Add("@ReturnVal", System.Data.SqlDbType.Int);
                        prm.Direction = System.Data.ParameterDirection.ReturnValue;

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        if (((int)prm.Value != 0))
                        {
                            id = (int)prm.Value;
                        }
                    }

                    //'make sure we find a returnvalue or ID from scalar

                    if (id > 0)
                    {
                        //username exists; bail
                        er.message = "Please choose a different username.";
                        er.developerMessage = "Duplicate username attempt [" + Username + "]";
                        return ret;
                    }
                } catch (Exception ex)
                {
                    // shouldn't happen but some error occurred checking username validity; bail
                    er.message = "" + ex.Message;
                    er.developerMessage = "Exception checking username validity. " + ex.Message;
                    return ret; 
                }


                // call SP to create user
                try
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    dt = ASi.DataAccess.SqlHelper.ExecuteDataset(connectString, 
                        System.Data.CommandType.StoredProcedure, "APICreateUserAccount",
                        new SqlParameter("@ParentID", ParentId),
                        new SqlParameter("@TypeCode", TypeCode),
                        new SqlParameter("@AccountStatusID", AccountStatusID),
                        new SqlParameter("@AccountName", AccountName),
                        new SqlParameter("@URL", Url),
                        new SqlParameter("@InActiveDate", DBNull.Value),
                        new SqlParameter("@CustomTypeCode", CustomTypeCode),
                        new SqlParameter("@BillingAddressTypeCode", BillingAddressTypeCode),
                        new SqlParameter("@BillingAddress1", BillingAddress1),
                        new SqlParameter("@BillingAddress2", BillingAddress2),
                        new SqlParameter("@BillingCity", BillingCity),
                        new SqlParameter("@BillingStateCode", BillingStateCode),
                        new SqlParameter("@BillingPostalCode", BillingPostalCode),
                        new SqlParameter("@BillingCountryCode", BillingCountryCode),
                        new SqlParameter("@PhoneTypeCode", PhoneTypeCode),
                        new SqlParameter("@PhoneNumber", PhoneNumber),
                        new SqlParameter("@Ext", Ext),
                        new SqlParameter("@EmailTypeCode", EmailTypeCode),
                        new SqlParameter("@EmailName", EmailName),
                        new SqlParameter("@EmailAddress", EmailAddress),
                        new SqlParameter("@PersonTypeCode", PersonTypeCode),
                        new SqlParameter("@PersonPrefix", PersonPrefix),
                        new SqlParameter("@PersonFirstName", PersonFirstname),
                        new SqlParameter("@PersonMI", PersonMI),
                        new SqlParameter("@PersonLastName", PersonLastname),
                        new SqlParameter("@PersonSuffix", PersonSuffix),
                        new SqlParameter("@ShippingAddressTypeCode", ShippingAddressTypeCode),
                        new SqlParameter("@ShippingAddress1", ShippingAddress1),
                        new SqlParameter("@ShippingAddress2", ShippingAddress2),
                        new SqlParameter("@ShippingCity", ShippingCity),
                        new SqlParameter("@ShippingStateCode", ShippingStateCode),
                        new SqlParameter("@ShippingPostalCode", ShippingPostalCode),
                        new SqlParameter("@ShippingCountryCode", ShippingCountryCode),
                        new SqlParameter("@Username", Username),
                        new SqlParameter("@EncryptedPassword", encryptedPassword),
                        new SqlParameter("@HashedPassword", hashedPassword)).Tables[0];

                    if ((dt.Rows.Count > 0 ) && ((int)dt.Rows[0][Constants.CUSTOMERID_ORDINAL] > 0) && ((int)dt.Rows[0][Constants.USERID_ORDINAL] > 0))
                    {
                        // OK
                        //r[0][0] = customerid is row 1, col 0
                        //r[0][8] = userid is row 1, col 8
                        ur.ParentId = ParentId;
                        ur.AccountId = (int)dt.Rows[0][Constants.CUSTOMERID_ORDINAL];
                        ur.UserId = (int)dt.Rows[0][8Constants.USERID_ORDINAL];
                        ret = true;
                    }
                    else
                    {
                        // no exception, but no user info; unknown response
                        er.message = "Unrecognized response creating account.";
                        er.developerMessage = "Possible error creating user. Unknown result. Count=" + dt.Rows.Count.ToString() + " CID=" + dt.Rows[0][0].ToString() + " UID=" + dt.Rows[0][8].ToString();
                    }
                    
                }
                catch (Exception ex)
                {
                    // exception calling APICreateUserAccount
                    er.message = "" + ex.Message;
                    er.developerMessage = "Exception checking creating user. " + ex.Message;
                }
            }
            catch(Exception ex)
            {
                if (er.message == "")
                {
                    er.message = "Unable to create user.";
                }
                er.developerMessage = "Outer exception:" + ex.Message;
            }
            return ret;
        }

        private void setAddressProperties()
        {
            if ((BillingAddress1 != "") && (BillingCity !="") && (BillingStateCode !="") && (BillingPostalCode != "") &&
               (ShippingAddress1 == "") && (ShippingCity == "") && (ShippingStateCode == "") && (ShippingPostalCode == ""))
            {
                //assign billing to shipping 
                ShippingAddress1 = BillingAddress1;
                ShippingAddress2 = BillingAddress2;
                ShippingCity = BillingCity;
                ShippingStateCode = BillingStateCode;
                ShippingPostalCode = BillingPostalCode;
                ShippingCountryCode = BillingCountryCode;
            }
        }
        private bool checkProperties()
        {
            if (ParentId < 1)
            {
                er.property += "pid,";
            }
            if (String.IsNullOrEmpty(PersonFirstname))
            {
                er.property += "firstname,";
            }
            if (String.IsNullOrEmpty(PersonLastname))
            {
                er.property += "lastname,";
            }
            if (String.IsNullOrEmpty(EmailAddress))
            {
                er.property += "email,";
            }
            if (String.IsNullOrEmpty(Username))
            {
                er.property += "username,";
            }
            if (String.IsNullOrEmpty(encryptedPassword))
            {
                er.property += "password";
            }
            
            if (er.property !="")
            {
                er.message = "One or more required parameters are missing or invalid [" + er.property + "]";
                er.developerMessage = er.message;
                return false;
            }

            // else, continue clearing nulls for db
            if (String.IsNullOrEmpty(BillingAddress1))
            {
                BillingAddress1 = "";
            }
            if (String.IsNullOrEmpty(BillingAddress2))
            {
                BillingAddress2 = "";
            }
            if (String.IsNullOrEmpty(BillingCity))
            {
                BillingCity = "";
            }
            if (String.IsNullOrEmpty(BillingStateCode))
            {
                BillingStateCode = "";
            }
            if (String.IsNullOrEmpty(BillingPostalCode))
            {
                BillingPostalCode = "";
            }
            if (String.IsNullOrEmpty(BillingCountryCode))
            {
                BillingCountryCode = "";
            }


            if (String.IsNullOrEmpty(ShippingAddress1))
            {
                ShippingAddress1 = "";
            }
            if (String.IsNullOrEmpty(ShippingAddress2))
            {
                ShippingAddress2 = "";
            }

            if (String.IsNullOrEmpty(ShippingCity))
            {
                ShippingCity = "";
            }
            if (String.IsNullOrEmpty(ShippingStateCode))
            {
                ShippingStateCode = "";
            }
            if (String.IsNullOrEmpty(ShippingPostalCode))
            {
                ShippingPostalCode = "";
            }
            if (String.IsNullOrEmpty(ShippingCountryCode))
            {
                ShippingCountryCode = "";
            }
            if (String.IsNullOrEmpty(PhoneNumber))
            {
                PhoneNumber = "";
                Ext = "";
            }
            if (String.IsNullOrEmpty(Url))
            {
                Url = "";
            }
            if (String.IsNullOrEmpty(PersonPrefix))
            {
                PersonPrefix = "";
            }
            if (String.IsNullOrEmpty(PersonSuffix))
            {
                PersonSuffix = "";
            }
            if (String.IsNullOrEmpty(PersonMI))
            {
                PersonMI = "";
            }

            return true;
        }
    }

    public class UserRequest
    {
        [Required]
        public int pid { get; set; } /* parent accountid */
        public string CompanyName { get; set; } /* company or first+last */
        [Url]
        public string Url { get; set; } /* URL */
        /* billing address fields */
        public string BillingAddress1 { get; set; } /*  */
        public string BillingAddress2 { get; set; } /*  */
        public string BillingCity { get; set; } /*  */
        public string BillingStateCode { get; set; } /*  */
        public string BillingPostalCode { get; set; } /*  */
        public string BillingCountryCode { get; set; } /*  */

        /* optional phone */
        [Phone]
        public string PhoneNumber { get; set; } /*  */
        public string Ext { get; set; } /*  */

        [Required]
        [EmailAddress]
        public string Email { get; set; } /*  */
        [Required]
        public string Firstname { get; set; } /*  */
        public string MI { get; set; } /*  */
        [Required]
        public string Lastname { get; set; } /*  */
        
        /* shipping address fields */
        public string ShippingAddress1 { get; set; } /*  */
        public string ShippingAddress2 { get; set; } /*  */
        public string ShippingCity { get; set; } /*  */
        public string ShippingStateCode { get; set; } /*  */
        public string ShippingPostalCode { get; set; } /*  */
        public string ShippingCountryCode { get; set; } /*  */
        [Required]
        [StringLength(8)]
        public string Username { get; set; } /*  */
        [Required]
        [StringLength(8)]
        public string Password { get; set; } /*  */
    }

    public static class Constants
    {
        public const string BLANK = "_BLANK";
        public const string ACCOUNT_TYPECODE_CUSTOMER = "CUSTOMER";
        public const string ACCOUNT_TYPECODE_MERCHANT = "MERCHANT"; //not used
        public const string ADDRESS_TYPECODE_BILLING = "BILLING";
        public const string ADDRESS_TYPECODE_SHIPPING= "SHIPPING";
        public const string CUSTOM_TYPECODE_API= "API";
        public const int CUSTOMERID_ORDINAL = 0;
        public const int USERID_ORDINAL = 8;
        
    }


}