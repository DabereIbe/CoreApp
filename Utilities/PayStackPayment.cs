using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayStack.Net;
using CoreApp.Models;
using CoreApp.Data;

namespace CoreApp.Utilities
{
    public class PayStackPayment
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        public PaymentModel _paymentModel;

        public PayStackApi PayStack { get; set; }

        public string Message { get; set; }
        public string Reference { get; set; }
        public string AuthorizationUrl { get; set; }

        public PayStackPayment(IConfiguration configuration, ApplicationDbContext db)
        {
            _configuration = configuration;
            _db = db;
            _paymentModel = _configuration.GetSection("PayStack").Get<PaymentModel>();
            PayStack = new PayStackApi(_paymentModel.SecretKey);
        }

        public Task InitializePayment(int Amount, string Email, string Name, string Address)
        {
            var request = new TransactionInitializeRequest()
            {
                AmountInKobo = Amount * 100,
                Email = Email,
                Reference = Guid.NewGuid().ToString(),
                Currency = "NGN",
                CallbackUrl = "https://localhost:5001/Cart/Verify"
                //CallbackUrl = "http://www.watchstop.com/Cart/Verify"
            };

            TransactionInitializeResponse response = PayStack.Transactions.Initialize(request);
            if (response.Status)
            {
                Reference = request.Reference;
                var orderModel = new Orders()
                {
                    Name = Name,
                    Amount = Amount,
                    Email = Email,
                    Address = Address,
                    TransactionRef = Reference
                };
                _db.Orders.Add(orderModel);
                _db.SaveChanges();
                AuthorizationUrl = response.Data.AuthorizationUrl;
            }
            Message = response.Message;
            return Task.FromResult(AuthorizationUrl);
        }

        public Task VerifyPayment(string reference)
        {
            TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
            if (response.Data.Status == "success")
            {
                var order =  _db.Orders.FirstOrDefault(x => x.TransactionRef == reference);
                order.Status = true;
                _db.Orders.Update(order);
                _db.SaveChanges();
            }
            Message = response.Data.GatewayResponse;
            return Task.FromResult(Message);
        }
    }
}
