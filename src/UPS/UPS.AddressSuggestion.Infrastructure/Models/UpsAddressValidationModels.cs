using System.Text.Json.Serialization;

namespace UPS.AddressSuggestion.Infrastructure.Models;

public sealed class UpsXavRequestWrapper
{
    [JsonPropertyName("XAVRequest")]
    public UpsXavRequest XavRequest { get; set; } = new();
}

public sealed class UpsXavRequest
{
    [JsonPropertyName("AddressKeyFormat")]
    public UpsAddressKeyFormat AddressKeyFormat { get; set; } = new();
}

public sealed class UpsAddressKeyFormat
{
    public string? ConsigneeName { get; set; }
    public string? AttentionName { get; set; }
    public IReadOnlyCollection<string>? AddressLine { get; set; }
    public string? Region { get; set; }
    public string? PoliticalDivision2 { get; set; }
    public string? PoliticalDivision1 { get; set; }
    public string? PostcodePrimaryLow { get; set; }
    public string? PostcodeExtendedLow { get; set; }
    public string? Urbanization { get; set; }
    public string CountryCode { get; set; } = "US";
}

public sealed class UpsXavResponseWrapper
{
    [JsonPropertyName("XAVResponse")]
    public UpsXavResponse XavResponse { get; set; } = new();
}

public sealed class UpsXavResponse
{
    public UpsResponse Response { get; set; } = new();
    public string? ValidAddressIndicator { get; set; }
    public string? AmbiguousAddressIndicator { get; set; }
    public string? NoCandidatesIndicator { get; set; }
    public UpsAddressClassification? AddressClassification { get; set; }
    public List<UpsCandidate> Candidate { get; set; } = [];
}

public sealed class UpsResponse
{
    public UpsResponseStatus ResponseStatus { get; set; } = new();
    public List<UpsAlert> Alert { get; set; } = [];
    public UpsTransactionReference? TransactionReference { get; set; }
}

public sealed class UpsResponseStatus
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class UpsAlert
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class UpsTransactionReference
{
    public string? CustomerContext { get; set; }
}

public sealed class UpsAddressClassification
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class UpsCandidate
{
    public UpsAddressClassification? AddressClassification { get; set; }
    public UpsAddressKeyFormat AddressKeyFormat { get; set; } = new();
}

public sealed class UpsErrorResponse
{
    public UpsCommonErrorResponse? Response { get; set; }
}

public sealed class UpsCommonErrorResponse
{
    public List<UpsErrorMessage> Errors { get; set; } = [];
}

public sealed class UpsErrorMessage
{
    public string? Code { get; set; }
    public string? Message { get; set; }
}
