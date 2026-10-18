using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
public class SaleServiceSoapClient : ClientBase<SaleServiceSoap>, SaleServiceSoap
{
    public enum EndpointConfiguration
    {
        SaleServiceSoap,
        SaleServiceSoap12
    }

    public SaleServiceSoapClient(EndpointConfiguration endpointConfiguration)
        : base(GetBindingForEndpoint(endpointConfiguration), GetEndpointAddress(endpointConfiguration))
    {
        Endpoint.Name = endpointConfiguration.ToString();
    }

    public SaleServiceSoapClient(EndpointConfiguration endpointConfiguration, string remoteAddress)
        : base(GetBindingForEndpoint(endpointConfiguration), new EndpointAddress(remoteAddress))
    {
        Endpoint.Name = endpointConfiguration.ToString();
    }

    public SaleServiceSoapClient(EndpointConfiguration endpointConfiguration, EndpointAddress remoteAddress)
        : base(GetBindingForEndpoint(endpointConfiguration), remoteAddress)
    {
        Endpoint.Name = endpointConfiguration.ToString();
    }

    public SaleServiceSoapClient(Binding binding, EndpointAddress remoteAddress)
        : base(binding, remoteAddress)
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    Task<SalePaymentRequestResponse> SaleServiceSoap.SalePaymentRequestAsync(SalePaymentRequestRequest request)
    {
        return Channel.SalePaymentRequestAsync(request);
    }

    public Task<SalePaymentRequestResponse> SalePaymentRequestAsync(ClientSaleRequestData requestData)
    {
        SalePaymentRequestRequest salePaymentRequestRequest = new SalePaymentRequestRequest();
        salePaymentRequestRequest.Body = new SalePaymentRequestRequestBody();
        salePaymentRequestRequest.Body.requestData = requestData;
        return ((SaleServiceSoap)this).SalePaymentRequestAsync(salePaymentRequestRequest);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    Task<SalePaymentWithIdResponse> SaleServiceSoap.SalePaymentWithIdAsync(SalePaymentWithIdRequest request)
    {
        return Channel.SalePaymentWithIdAsync(request);
    }

    public Task<SalePaymentWithIdResponse> SalePaymentWithIdAsync(ClientSaleRequestData requestData)
    {
        SalePaymentWithIdRequest salePaymentWithIdRequest = new SalePaymentWithIdRequest();
        salePaymentWithIdRequest.Body = new SalePaymentWithIdRequestBody();
        salePaymentWithIdRequest.Body.requestData = requestData;
        return ((SaleServiceSoap)this).SalePaymentWithIdAsync(salePaymentWithIdRequest);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    Task<UDSalePaymentRequestResponse> SaleServiceSoap.UDSalePaymentRequestAsync(UDSalePaymentRequestRequest request)
    {
        return Channel.UDSalePaymentRequestAsync(request);
    }

    public Task<UDSalePaymentRequestResponse> UDSalePaymentRequestAsync(ClientSaleRequestData requestData)
    {
        UDSalePaymentRequestRequest uDSalePaymentRequestRequest = new UDSalePaymentRequestRequest();
        uDSalePaymentRequestRequest.Body = new UDSalePaymentRequestRequestBody();
        uDSalePaymentRequestRequest.Body.requestData = requestData;
        return ((SaleServiceSoap)this).UDSalePaymentRequestAsync(uDSalePaymentRequestRequest);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    Task<SalePaymentWithDiscountResponse> SaleServiceSoap.SalePaymentWithDiscountAsync(SalePaymentWithDiscountRequest request)
    {
        return Channel.SalePaymentWithDiscountAsync(request);
    }

    public Task<SalePaymentWithDiscountResponse> SalePaymentWithDiscountAsync(ClientSaleDiscountRequestData requestData)
    {
        SalePaymentWithDiscountRequest salePaymentWithDiscountRequest = new SalePaymentWithDiscountRequest();
        salePaymentWithDiscountRequest.Body = new SalePaymentWithDiscountRequestBody();
        salePaymentWithDiscountRequest.Body.requestData = requestData;
        return ((SaleServiceSoap)this).SalePaymentWithDiscountAsync(salePaymentWithDiscountRequest);
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
            case EndpointConfiguration.SaleServiceSoap:
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
            case EndpointConfiguration.SaleServiceSoap12:
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
            EndpointConfiguration.SaleServiceSoap => new EndpointAddress("https://pec.shaparak.ir/NewIPGServices/Sale/SaleService.asmx"),
            EndpointConfiguration.SaleServiceSoap12 => new EndpointAddress("https://pec.shaparak.ir/NewIPGServices/Sale/SaleService.asmx"),
            _ => throw new InvalidOperationException($"Could not find endpoint with name '{endpointConfiguration}'."),
        };
    }
}
