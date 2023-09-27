using Braintree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Utility.Payment
{
    public interface IPaymentGateway
    {
        IBraintreeGateway CreateGateway();
        IBraintreeGateway GetGateway();
    }
}
