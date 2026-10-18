using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
public class ConfirmServiceSoapClient : ClientBase<ConfirmServiceSoap>, ConfirmServiceSoap
{
    public enum EndpointConfiguration
    {
        ConfirmServiceSoap,
        ConfirmServiceSoap12
    }

    public ConfirmServiceSoapClient(EndpointConfiguration endpointConfiguration)
        : base(GetBindingForEndpoint(endpointConfiguration), GetEndpointAddress(endpointConfiguration))
    {
        Endpoint.Name = endpointConfiguration.ToString();
    }

    public ConfirmServiceSoapClient(EndpointConfiguration endpointConfiguration, string remoteAddress)
        : base(GetBindingForEndpoint(endpointConfiguration), new EndpointAddress(remoteAddress))
    {
        Endpoint.Name = endpointConfiguration.ToString();
    }

    public ConfirmServiceSoapClient(EndpointConfiguration endpointConfiguration, EndpointAddress remoteAddress)
        : base(GetBindingForEndpoint(endpointConfiguration), remoteAddress)
    {
        Endpoint.Name = endpointConfiguration.ToString();
    }

    public ConfirmServiceSoapClient(Binding binding, EndpointAddress remoteAddress)
        : base(binding, remoteAddress)
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    Task<ConfirmPaymentResponse> ConfirmServiceSoap.ConfirmPaymentAsync(ConfirmPaymentRequest request)
    {
        return Channel.ConfirmPaymentAsync(request);
    }

    public Task<ConfirmPaymentResponse> ConfirmPaymentAsync(ClientConfirmRequestData requestData)
    {
        ConfirmPaymentRequest confirmPaymentRequest = new ConfirmPaymentRequest();
        confirmPaymentRequest.Body = new ConfirmPaymentRequestBody();
        confirmPaymentRequest.Body.requestData = requestData;
        return ((ConfirmServiceSoap)this).ConfirmPaymentAsync(confirmPaymentRequest);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    Task<ConfirmPaymentWithAddDataResponse> ConfirmServiceSoap.ConfirmPaymentWithAddDataAsync(ConfirmPaymentWithAddDataRequest request)
    {
        return Channel.ConfirmPaymentWithAddDataAsync(request);
    }

    public Task<ConfirmPaymentWithAddDataResponse> ConfirmPaymentWithAddDataAsync(ClientConfirmRequestData requestData)
    {
        ConfirmPaymentWithAddDataRequest confirmPaymentWithAddDataRequest = new ConfirmPaymentWithAddDataRequest();
        confirmPaymentWithAddDataRequest.Body = new ConfirmPaymentWithAddDataRequestBody();
        confirmPaymentWithAddDataRequest.Body.requestData = requestData;
        return ((ConfirmServiceSoap)this).ConfirmPaymentWithAddDataAsync(confirmPaymentWithAddDataRequest);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    Task<ConfirmPaymentWithAmountResponse> ConfirmServiceSoap.ConfirmPaymentWithAmountAsync(ConfirmPaymentWithAmountRequest request)
    {
        return Channel.ConfirmPaymentWithAmountAsync(request);
    }

    public Task<ConfirmPaymentWithAmountResponse> ConfirmPaymentWithAmountAsync(ClientConfirmWithAmountRequestData requestData)
    {
        ConfirmPaymentWithAmountRequest confirmPaymentWithAmountRequest = new ConfirmPaymentWithAmountRequest();
        confirmPaymentWithAmountRequest.Body = new ConfirmPaymentWithAmountRequestBody();
        confirmPaymentWithAmountRequest.Body.requestData = requestData;
        return ((ConfirmServiceSoap)this).ConfirmPaymentWithAmountAsync(confirmPaymentWithAmountRequest);
    }

    public virtual Task OpenAsync()
    {
        return Task.Factory.FromAsync(((ICommunicationObject)this).BeginOpen((AsyncCallback)null, (object)null), ((ICommunicationObject)this).EndOpen);
    }

    public virtual Task CloseAsync()
    {
        return Task.Factory.FromAsync(((ICommunicationObject)this).BeginClose((AsyncCallback)null, (object)null), ((ICommunicationObject)this).EndClose);
    }

    private static Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
    {
        switch (endpointConfiguration)
        {
            case EndpointConfiguration.ConfirmServiceSoap:
                return new BasicHttpBinding
                {
                    MaxBufferSize = int.MaxValue,
                    ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                    MaxReceivedMessageSize = 2147483647L,
                    AllowCookies = true,
                    Security =
                {
                    Mode = BasicHttpSecurityMode.Transport
                }
                };
            case EndpointConfiguration.ConfirmServiceSoap12:
                {
                    CustomBinding customBinding = new CustomBinding();
                    TextMessageEncodingBindingElement item = new TextMessageEncodingBindingElement
                    {
                        MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.None)
                    };
                    customBinding.Elements.Add(item);
                    HttpsTransportBindingElement item2 = new HttpsTransportBindingElement
                    {
                        AllowCookies = true,
                        MaxBufferSize = int.MaxValue,
                        MaxReceivedMessageSize = 2147483647L
                    };
                    customBinding.Elements.Add(item2);
                    return customBinding;
                }
            default:
                throw new InvalidOperationException($"Could not find endpoint with name '{endpointConfiguration}'.");
        }
    }

    private static EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
    {
        return endpointConfiguration switch
        {
            EndpointConfiguration.ConfirmServiceSoap => new EndpointAddress("https://pec.shaparak.ir/NewIPGServices/Confirm/ConfirmService.asmx"),
            EndpointConfiguration.ConfirmServiceSoap12 => new EndpointAddress("https://pec.shaparak.ir/NewIPGServices/Confirm/ConfirmService.asmx"),
            _ => throw new InvalidOperationException($"Could not find endpoint with name '{endpointConfiguration}'."),
        };
    }
}
