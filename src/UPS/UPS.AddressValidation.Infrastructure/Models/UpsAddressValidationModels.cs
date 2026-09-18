using System.Text.Json.Serialization;

namespace UPS.AddressValidation.Infrastructure.Models;

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
    [JsonPropertyName("ConsigneeName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ConsigneeName { get; set; }

    [JsonPropertyName("AttentionName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AttentionName { get; set; }

    [JsonPropertyName("AddressLine")]
    public List<string> AddressLine { get; set; } = [];

    [JsonPropertyName("Region")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Region { get; set; }

    [JsonPropertyName("PoliticalDivision2")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PoliticalDivision2 { get; set; }

    [JsonPropertyName("PoliticalDivision1")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PoliticalDivision1 { get; set; }

    [JsonPropertyName("PostcodePrimaryLow")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PostcodePrimaryLow { get; set; }

    [JsonPropertyName("PostcodeExtendedLow")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PostcodeExtendedLow { get; set; }

    [JsonPropertyName("Urbanization")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Urbanization { get; set; }

    [JsonPropertyName("CountryCode")]
    public string CountryCode { get; set; } = string.Empty;
}

public sealed class UpsXavResponseWrapper
{
    [JsonPropertyName("XAVResponse")]
    public UpsXavResponse XavResponse { get; set; } = new();
}

public sealed class UpsXavResponse
{
    [JsonPropertyName("Response")]
    public UpsResponse Response { get; set; } = new();

    [JsonPropertyName("ValidAddressIndicator")]
    public string? ValidAddressIndicator { get; set; }

    [JsonPropertyName("AmbiguousAddressIndicator")]
    public string? AmbiguousAddressIndicator { get; set; }

    [JsonPropertyName("NoCandidatesIndicator")]
    public string? NoCandidatesIndicator { get; set; }

    [JsonPropertyName("AddressClassification")]
    public UpsClassification? AddressClassification { get; set; }

    [JsonPropertyName("Candidate")]
    public List<UpsCandidate> Candidate { get; set; } = [];
}

public sealed class UpsResponse
{
    [JsonPropertyName("ResponseStatus")]
    public UpsResponseStatus ResponseStatus { get; set; } = new();

    [JsonPropertyName("Alert")]
    public List<UpsAlert> Alert { get; set; } = [];

    [JsonPropertyName("TransactionReference")]
    public UpsTransactionReference? TransactionReference { get; set; }
}

public sealed class UpsResponseStatus
{
    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }
}

public sealed class UpsAlert
{
    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }
}

public sealed class UpsTransactionReference
{
    [JsonPropertyName("CustomerContext")]
    public string? CustomerContext { get; set; }
}

public sealed class UpsClassification
{
    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }
}

public sealed class UpsCandidate
{
    [JsonPropertyName("AddressClassification")]
    public UpsClassification? AddressClassification { get; set; }

    [JsonPropertyName("AddressKeyFormat")]
    public UpsCandidateAddressKeyFormat AddressKeyFormat { get; set; } = new();
}

public sealed class UpsCandidateAddressKeyFormat
{
    [JsonPropertyName("ConsigneeName")]
    public string? ConsigneeName { get; set; }

    [JsonPropertyName("AttentionName")]
    public string? AttentionName { get; set; }

    [JsonPropertyName("AddressLine")]
    public List<string> AddressLine { get; set; } = [];

    [JsonPropertyName("Region")]
    public string? Region { get; set; }

    [JsonPropertyName("PoliticalDivision2")]
    public string? PoliticalDivision2 { get; set; }

    [JsonPropertyName("PoliticalDivision1")]
    public string? PoliticalDivision1 { get; set; }

    [JsonPropertyName("PostcodePrimaryLow")]
    public string? PostcodePrimaryLow { get; set; }

    [JsonPropertyName("PostcodeExtendedLow")]
    public string? PostcodeExtendedLow { get; set; }

    [JsonPropertyName("Urbanization")]
    public string? Urbanization { get; set; }

    [JsonPropertyName("CountryCode")]
    public string? CountryCode { get; set; }
}

public sealed class UpsErrorResponse
{
    [JsonPropertyName("response")]
    public UpsCommonErrorResponse? Response { get; set; }
}

public sealed class UpsCommonErrorResponse
{
    [JsonPropertyName("errors")]
    public List<UpsErrorMessage> Errors { get; set; } = [];
}

public sealed class UpsErrorMessage
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
