# Federal Tax Profiles Quick Reference

## Quick Start

### API Endpoint
```
GET /payroll/v1/workers/{aoid}/us-tax-profiles
```

### Primary Key
```csharp
ProfileId (string) - From ItemID or generated GUID
```

### Main Classes

| Class | Purpose | Location |
|-------|---------|----------|
| `USTaxProfilesResponse` | API response wrapper | FederalTaxProfile.cs |
| `USTaxProfile` | API DTO with JSON attrs | FederalTaxProfile.cs |
| `FederalTaxProfilesDataObject` | Xchange internal model | FederalTaxProfilesDataObject.cs |
| `FederalTaxProfilesDataReader` | Data orchestrator | FederalTaxProfilesDataReader.cs |

## Key Properties

### Federal Tax Withholding
```csharp
FilingStatusCode         // Single, Married, etc.
WithholdingAllowanceQuantity
DependentsQuantity
OtherIncomeAmount
DeductionsAmount
AdditionalWithholdingAmount
ExemptionCode
TaxFormCode             // W4_2019, W4_2020
MultipleJobsIndicator
HighestPayingPositionIndicator
```

### State Tax Withholding
```csharp
StateCode
FilingStatusCode
WithholdingAllowanceQuantity
AdditionalWithholdingAmount
AdditionalWithholdingPercentage
ResidencyStatusCode
```

### Local Tax Withholding
```csharp
LocalityCode
FilingStatusCode
AdditionalWithholdingAmount
ResidencyStatusCode
```

## Data Flow Diagram

```
Workers API
    ?
[For each worker]
    ?
Tax Profile API (/payroll/v1/workers/{aoid}/us-tax-profiles)
    ?
USTaxProfile (DTO)
    ?
Map to FederalTaxProfilesDataObject
    ?
Yield to Xchange
```

## Code Examples

### Fetch Tax Profile for Single Worker
```csharp
var response = await _apiClient.GetWorkerTaxProfileAsync(
    "G3R8CTYAHF328GQH",  // associateOID
    cancellationToken
);

if (response.IsSuccessful)
{
    foreach (var profile in response.Data.UsTaxProfiles)
    {
        // Process profile
    }
}
```

### Mapping Example
```csharp
var dataObject = new FederalTaxProfilesDataObject
{
    ProfileId = taxProfile.ItemID ?? Guid.NewGuid().ToString(),
    AssociateOID = taxProfile.AssociateOID,
    FederalTaxWithholding = MapFederalTaxWithholding(taxProfile.FederalTaxWithholding)
};
```

## Common Scenarios

### Scenario 1: Worker has no tax profile
```csharp
// API returns empty array
// Reader logs warning and continues to next worker
```

### Scenario 2: Worker has multiple tax profiles
```csharp
// API returns array with multiple profiles
// Reader maps each profile separately
// All profiles are yielded
```

### Scenario 3: API call fails for a worker
```csharp
// Error is logged with worker AssociateOID
// Failure count is incremented
// Processing continues with next worker
```

## Logging Examples

```csharp
// Success
LogInformation("Retrieved 2 tax profile(s) for worker G3R8CTYAHF328GQH")

// Warning
LogWarning("Failed to retrieve tax profile for worker G3R8CTYAHF328GQH. StatusCode: 404")

// Progress
LogInformation("Progress: Processed 100/500 workers. Success: 95, Failures: 5")

// Complete
LogInformation("Completed fetching tax profiles. Total: 500, Success: 480, Failures: 20")
```

## Error Handling

### Caught Exceptions
- `HttpRequestException` - Network/HTTP errors
- `ApiException` - API-specific errors (with StatusCode)
- `Exception` - General unexpected errors

### Recovery Strategy
- Log error with context (AssociateOID, StatusCode)
- Increment failure counter
- Continue processing next worker
- Return all successfully fetched profiles

## Performance Metrics

| Workers | Est. Time | API Calls | Rate Limit Impact |
|---------|-----------|-----------|-------------------|
| 10      | ~2 sec    | 11        | Minimal           |
| 100     | ~15 sec   | 101       | Moderate          |
| 1,000   | ~150 sec  | 1,001     | Significant       |
| 10,000  | ~25 min   | 10,001    | Very High         |

*Rate limit: 10 requests/second (enforced automatically)*

## Testing Checklist

- [ ] Worker with no tax profile
- [ ] Worker with single tax profile
- [ ] Worker with multiple tax profiles
- [ ] API returns 404 for worker
- [ ] API returns 500 error
- [ ] Network timeout
- [ ] Cancellation during processing
- [ ] Large worker count (>1000)
- [ ] Date parsing (valid/invalid dates)
- [ ] Null/missing fields in response

## Troubleshooting

### Issue: No profiles returned
**Check:**
- Workers exist in the system
- Workers have tax profiles configured
- API credentials have payroll permissions
- Endpoint URL is correct

### Issue: Slow performance
**Check:**
- Rate limiting is working (expected)
- Network latency to ADP API
- Large worker count (normal for N+1 pattern)

### Issue: Mapping errors
**Check:**
- API response structure matches DTOs
- Null handling in mapping methods
- Date format compatibility

## Related Files

```
Connector/App/v1/FederalTaxProfiles/
??? FederalTaxProfile.cs                    # API DTOs
??? FederalTaxProfilesDataObject.cs         # Internal model
??? FederalTaxProfilesDataReader.cs         # Orchestrator

Connector/Client/
??? ApiClient.cs                             # API methods

Connector/Documentation/
??? FederalTaxProfiles_File_Structure.md         # Architecture
??? FederalTaxProfiles_Implementation_Summary.md # Implementation
??? FederalTaxProfiles_Quick_Reference.md        # This file
```

## Important Notes

?? **One API call per worker** - This is by design (ADP API structure)  
?? **Rate limiting is enforced** - Expect ~10 seconds per 100 workers  
?? **Workers fetched first** - Then tax profiles fetched individually  
?? **Failures are isolated** - One worker failure doesn't stop others  
?? **Profiles are collected** - Then yielded (not streamed immediately)  

## Support

For questions or issues:
1. Check logs for detailed error messages
2. Review API response structure
3. Verify worker has tax profiles in ADP
4. Check API credentials and permissions
