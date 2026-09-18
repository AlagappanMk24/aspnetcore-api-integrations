namespace FedEx.Shipment.Infrastructure.Configuration;

public sealed class FedExOptions
{
    public string BaseUrl{get;set;}="https://apis-sandbox.fedex.com";
    public string ClientId{get;set;}="";
    public string ClientSecret{get;set;}="";
    public string AccountNumber{get;set;}="";
}