using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Pagos.Designer.Interfaces.External.CustomHooks;
using Pagos.Designer.Interfaces.External.Messaging;
using Pagos.SpreadsheetWeb.Web.Api.Objects.Calculation;
using System;
using System.IO;
using System.Linq;

namespace AuthorizeNetExample
{
    public class Class1 : IAfterCalculation
    {

        private string apiLoginId = "your_api_login_id";
        private string transactionKey = "your_current_transaction_key";

        public ActionableResponse AfterCalculation(CalculationRequest request, CalculationResponse response)
        {
            var name = request.Inputs.FirstOrDefault(x => x.Ref == "iName");
            var lastName = request.Inputs.FirstOrDefault(x => x.Ref == "iSurname");
            var cardNumber = request.Inputs.FirstOrDefault(x => x.Ref == "iCardNumber");
            var year = request.Inputs.FirstOrDefault(x => x.Ref == "iYear");
            var month = request.Inputs.FirstOrDefault(x => x.Ref == "iMonth");
            var cvc = request.Inputs.FirstOrDefault(x => x.Ref == "iCVC");
            var amount = Convert.ToDecimal(request.Inputs.FirstOrDefault(x => x.Ref == "iAmount").Value[0][0].Value);
            var address = request.Inputs.FirstOrDefault(x => x.Ref == "iAddress");
            var city = request.Inputs.FirstOrDefault(x => x.Ref == "iCity");
            var zip = request.Inputs.FirstOrDefault(x => x.Ref == "iZip");

            var errRespo = new ActionableResponse();
            errRespo.Success = false;
            errRespo.ResponseAction = ResponseAction.Cancel;
            errRespo.Messages = new System.Collections.Generic.List<string>();

            try
            {
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = apiLoginId,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = transactionKey,
                };
                var creditCard = new creditCardType
                {
                    cardNumber = cardNumber.Value[0][0].Value,
                    expirationDate = month.Value[0][0].Value + year.Value[0][0].Value,
                    cardCode = cvc.Value[0][0].Value
                };
                var billingAddress = new customerAddressType
                {
                    firstName = name.Value[0][0].Value,
                    lastName = lastName.Value[0][0].Value,
                    address = address.Value[0][0].Value,
                    city = city.Value[0][0].Value,
                    zip = zip.Value[0][0].Value
                };
                var paymentType = new paymentType { Item = creditCard };

                // Add line Items
                var lineItems = new lineItemType[2];
                lineItems[0] = new lineItemType { itemId = "1", name = "t-shirt", quantity = 2, unitPrice = new Decimal(15.00) };
                lineItems[1] = new lineItemType { itemId = "2", name = "snowboard", quantity = 1, unitPrice = new Decimal(450.00) };
                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // charge the card

                    amount = amount,
                    payment = paymentType,
                    billTo = billingAddress,
                    lineItems = lineItems
                };

                var req = new createTransactionRequest { transactionRequest = transactionRequest };
                var controller = new createTransactionController(req);
                controller.Execute();

                var resp = controller.GetApiResponse();
                if (resp != null)
                {
                    if (resp.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (resp.transactionResponse.messages != null)
                        {

                            response.Outputs.FirstOrDefault(x => x.Ref == "oResponse").Value[0][0].Value
                                = "Successfully created transaction with Transaction ID: " + resp.transactionResponse.transId
                                + "<br />" + "Response Code: " + resp.transactionResponse.responseCode
                                + "<br />" + "Message Code: " + resp.transactionResponse.messages[0].code
                                + "<br />" + "Description: " + resp.transactionResponse.messages[0].description
                                + "<br />" + "Success, Auth Code : " + resp.transactionResponse.authCode;

                            return new ActionableResponse
                            {
                                Success = true
                            };

                        }
                        else
                        {

                            errRespo.Messages.Add("Failed Transaction.");

                            if (resp.transactionResponse.errors != null)
                            {
                                errRespo.Messages.Add("Error Code: " + resp.transactionResponse.errors[0].errorCode);
                                errRespo.Messages.Add("Error message: " + resp.transactionResponse.errors[0].errorText);
                            }
                            return errRespo;
                        }
                    }
                    else
                    {

                        errRespo.Messages.Add("Failed Transaction.");

                        if (resp.transactionResponse != null && resp.transactionResponse.errors != null)
                        {
                            errRespo.Messages.Add("Error Code: " + resp.transactionResponse.errors[0].errorCode);
                            errRespo.Messages.Add("Error message: " + resp.transactionResponse.errors[0].errorText);
                        }
                        else
                        {
                            errRespo.Messages.Add("Error Code: " + resp.messages.message[0].code);
                            errRespo.Messages.Add("Error message: " + resp.messages.message[0].text);
                        }
                        return errRespo;
                    }
                }
                else
                {
                    errRespo.Messages.Add("Null Response.");
                }
                return errRespo;
            }
            catch (Exception ex)
            {
                return new ActionableResponse
                {
                    Success = false,
                    Messages = new System.Collections.Generic.List<string>() { ex.Message },
                    ResponseAction = ResponseAction.Cancel
                };
            }

        }
    }
}
