using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Coyote.Tasks;

namespace TinyService
{
    using Document = Dictionary<string, string>;

    public class UserController
    {
        public async Task<ActionResult<User>> CreateUser(
            string userName,
            string email,
            string phoneNumber,
            string mailingAddress,
            string billingAddress)
        {
            var logger = new Logger(nameof(UserController));

            logger.Write($"Creating user {userName}, {email}, {phoneNumber}, {mailingAddress}, {billingAddress}");

            var db = new DatabaseProvider("CreateUser");

            var doc = new Document();
            doc[Constants.EmailAddress] = email;
            doc[Constants.PhoneNumber] = phoneNumber;
            doc[Constants.MailingAddress] = mailingAddress;
            doc[Constants.BillingAddress] = billingAddress;

            bool success = await db.AddDocumentIfNotExists(Constants.UserCollection, userName, doc);

            if (!success)
            {
                return new ActionResult<User>() { Success = false, Response = null };
            } 
            else
            {
                return new ActionResult<User>()
                {
                    Success = true,
                    Response = new User(userName, doc)
                };
            }
        }

        public async Task<ActionResult<User>> GetUser(string userName)
        {
            var logger = new Logger(nameof(UserController));

            logger.Write("Get user " + userName);

            var db = new DatabaseProvider("GetUser");
            var doc = await db.GetDocumentIfExists(Constants.UserCollection, userName);
            if (doc == null)
            {
                return new ActionResult<User>() { Success = false, Response = null };
            }

            return new ActionResult<User>() 
            { 
                Success = true, 
                Response = new User(userName, doc) 
            };
        }

        public async Task<ActionResult<Address>> UpdateUserAddress(string userName, string mailingAddress, string billingAddress)
        {
            var logger = new Logger(nameof(UserController));
            logger.Write($"Updating user address {userName} {mailingAddress} {billingAddress}");

            var db = new DatabaseProvider("UpdateUserAddress");

            var document = await db.GetDocumentIfExists(Constants.UserCollection, userName);
            if (document == null)
            {
                return new ActionResult<Address>() { Success = false, Response = null };
            }
            
            document[Constants.BillingAddress] = billingAddress;
            document[Constants.MailingAddress] = mailingAddress;
            //await db.UpdateDocument(Constants.UserCollection, userName, document);
            bool success = await db.UpdateDocumentIfExists(Constants.UserCollection, userName, document);
            
            return new ActionResult<Address>() 
            {
                Success = success, 
                Response = (success ? new Address(mailingAddress, billingAddress) : null)
            };
        }

        public async Task<ActionResult<User>> DeleteUser(string userName)
        {
            var logger = new Logger(nameof(UserController));
            logger.Write("Deleting user " + userName);

            var db = new DatabaseProvider("DeleteUser");
            var document = await db.GetDocumentIfExists(Constants.UserCollection, userName);
            //await db.DeleteDocument(Constants.UserCollection, userName);
            if (document == null)
            {
                return new ActionResult<User>() { Success = false, Response = null };
            }
            bool success = await db.DeleteDocumentIfExists(Constants.UserCollection, userName);
            return new ActionResult<User>()
            {
                Success = success, 
                Response = (success ? new User(userName, document) : null)
            };
        }
    }

    public class User
    {
        public string UserName;
        public string EmailAddress;
        public string PhoneNumber;
        public Address Address;

        public User()
        {
        }

        public User(string userName, Document doc)
        {
            this.UserName = userName;
            this.EmailAddress = doc[Constants.EmailAddress];
            this.PhoneNumber = doc[Constants.PhoneNumber];
            this.Address = new Address(doc[Constants.MailingAddress], doc[Constants.BillingAddress]);
        }

        public User(
            string userName,
            string emailAddress,
            string phoneNumber,
            string mailingAddress,
            string billingAddress)
        {
            this.UserName = userName;
            this.EmailAddress = emailAddress;
            this.PhoneNumber = phoneNumber;
            this.Address = new Address(mailingAddress, billingAddress);
        }
    }

    public class Address
    {
        public string MailingAddress;
        public string BillingAddress;

        public Address(string mailingAddress, string billingAddress)
        {
            this.MailingAddress = mailingAddress;
            this.BillingAddress = billingAddress;
        }
    }
}
