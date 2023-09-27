using Braintree;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Utility.Payment
{
    public class PaymentGateway : IPaymentGateway
    {
        public PaymentSettings _options;
        private IBraintreeGateway _paymentGateway;

        public PaymentGateway(IOptions<PaymentSettings> options)
        {
            _options = options.Value;
        }

        public IBraintreeGateway CreateGateway()
        {
            return new BraintreeGateway(_options.Environment, _options.MerchantId, _options.PublicKey, _options.PrivateKey);
        }

        public IBraintreeGateway GetGateway()
        {
            if(_paymentGateway == null ) 
            {
                _paymentGateway = CreateGateway();
            }

            return _paymentGateway;
        }
    }
}
