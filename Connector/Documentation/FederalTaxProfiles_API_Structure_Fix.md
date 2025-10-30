# Federal Tax Profiles API Structure Fix

## Issue
The JSON deserialization was failing because the actual ADP API response structure didn't match our DTOs.

### Error Message
```
System.Text.Json.JsonException: 'The JSON value could not be converted to System.Collections.Generic.List`1[Connector.App.v1.FederalTaxProfiles.USTaxProfileWrapper]'
```

## Root Cause
The ADP API returns `usTaxProfiles` as a **single object**, not an array of objects.

## Actual JSON Response Structure
```json
{
  "usTaxProfiles": {
    "payrollFileNumber": "1153",
    "payrollGroupCode": {
      "codeValue": "P7N",
      "longName": "P7N"
    },
    "itemID": "9200875313239_327",
    "usFederalTaxInstruction": {
      "federalIncomeTaxInstruction": {
        "taxWithholdingStatus": {
          "statusCode": {
            "codeValue": "",
            "shortName": "nonExempt"
          },
          "effectiveDate": "06/12/2023"
        },
        "taxFilingStatusCode": {
          "codeValue": "J",
          "longName": "Married filing jointly or Qualifying surviving spouse"
        },
        "taxWithholdingAllowanceQuantity": 0
      },
      "socialSecurityTaxInstruction": {...},
      "medicareTaxInstruction": {...},
      "federalUnemploymentTaxInstruction": {...}
    }
  }
}
```

## Changes Made

### 1. FederalTaxProfile.cs - Complete Restructure

**Before:**
```csharp
public class USTaxProfilesResponse
{
    [JsonPropertyName("usTaxProfiles")]
    public List<USTaxProfileWrapper> UsTaxProfiles { get; set; } = new();
}
```

**After:**
```csharp
public class USTaxProfilesResponse
{
    [JsonPropertyName("usTaxProfiles")]
    public USTaxProfile? UsTaxProfiles { get; set; }  // Single object, not a list
}
```

**New Structure Matches ADP API:**
- `USTaxProfile` - Main tax profile (single object)
- `USFederalTaxInstructionDTO` - Federal tax details
  - `FederalIncomeTaxInstructionDTO`
  - `TaxInstructionDTO` (for Social Security, Medicare, etc.)
  - `Form1099InstructionDTO`
- `USStateTaxInstructionDTO` - State tax details (array)
- `USLocalTaxInstructionDTO` - Local tax details (array)

### 2. FederalTaxProfilesDataObject.cs - Updated Internal Model

Restructured to match new API structure:
- Removed old generic withholding structures
- Added specific instruction types:
  - `USFederalTaxInstruction`
  - `FederalIncomeTaxInstruction`
  - `TaxInstruction`
  - `Form1099Instruction`
  - `USStateTaxInstruction`
  - `StateIncomeTaxInstruction`
  - `USLocalTaxInstruction`
  - `LocalIncomeTaxInstruction`

### 3. FederalTaxProfilesDataReader.cs - Simplified Processing

**Before:**
```csharp
// Attempted to loop through array
foreach (var taxProfileWrapper in taxProfileResponse.Data.UsTaxProfiles)
{
    if (taxProfileWrapper?.UsTaxProfile != null)
    {
        var mappedProfile = MapToFederalTaxProfilesDataObject(taxProfileWrapper.UsTaxProfile);
        // ...
    }
}
```

**After:**
```csharp
// Process single tax profile
if (taxProfileResponse.IsSuccessful && taxProfileResponse.Data?.UsTaxProfiles != null)
{
    var mappedProfile = MapToFederalTaxProfilesDataObject(
        taxProfileResponse.Data.UsTaxProfiles, 
        worker.AssociateOID);
    federalTaxProfiles.Add(mappedProfile);
    successCount++;
}
```

**Added Complete Mapping Methods:**
- `MapUSFederalTaxInstruction`
- `MapFederalIncomeTaxInstruction`
- `MapTaxInstruction`
- `MapForm1099Instruction`
- `MapTaxAllowance`
- `MapUSStateTaxInstruction`
- `MapStateIncomeTaxInstruction`
- `MapUSLocalTaxInstruction`
- `MapLocalIncomeTaxInstruction`
- `MapTaxWithholdingStatus`
- `MapTaxAmount`
- `MapTaxCodeValue`

### 4. ApiClient.cs - Enhanced Error Logging

Added better error handling to show actual JSON response when deserialization fails:
```csharp
catch (JsonException jsonEx)
{
    throw new ApiException(
        $"Failed to deserialize tax profile response for worker {associateOID}. Response: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}", 
        jsonEx)
    {
        StatusCode = (int)response.StatusCode
    };
}
```

## Key Differences from Initial Design

| Aspect | Initial Design | Actual API |
|--------|---------------|------------|
| Response Structure | Array of tax profiles | Single tax profile object |
| Federal Tax | Generic `FederalTaxWithholding` | Nested `usFederalTaxInstruction` with multiple sub-instructions |
| State/Local Tax | Generic withholding lists | Specific instruction types with nested structures |
| Worker Association | Not stored in tax profile | Must pass AssociateOID separately |

## Testing Results

? Build successful  
? JSON structure matches API response  
? All mapping methods implemented  
? Proper null handling  
? Error logging enhanced  

## Data Flow

```
GET /payroll/v1/workers/{aoid}/us-tax-profiles
          ?
USTaxProfilesResponse
    ? UsTaxProfiles (single object)
        ? USFederalTaxInstruction
            ? FederalIncomeTaxInstruction
            ? SocialSecurityTaxInstruction
            ? MedicareTaxInstruction
            ? FederalUnemploymentTaxInstruction
            ? Form1099Instruction
        ? USStateTaxInstructions (array)
        ? USLocalTaxInstructions (array)
          ?
FederalTaxProfilesDataObject (internal model)
```

## Important Notes

1. **One tax profile per worker** - API returns a single object, not an array
2. **Associate OID not in response** - Must be passed from worker context
3. **Nested structures** - Tax instructions are deeply nested, not flat
4. **Empty code values** - Some code values may be empty strings (e.g., `codeValue: ""`)
5. **Date format** - Dates are strings like "06/12/2023"

## Migration Impact

This is a breaking change from the initial implementation, but necessary to match the actual API response structure. Any code depending on the old structure will need to be updated.

## Files Changed

1. `Connector/App/v1/FederalTaxProfiles/FederalTaxProfile.cs` - Complete restructure
2. `Connector/App/v1/FederalTaxProfiles/FederalTaxProfilesDataObject.cs` - Updated internal model
3. `Connector/App/v1/FederalTaxProfiles/FederalTaxProfilesDataReader.cs` - Simplified processing, added mapping methods
4. `Connector/Client/ApiClient.cs` - Enhanced error logging

## Next Steps

1. Test with actual ADP API to verify complete response handling
2. Verify all nested structures deserialize correctly
3. Test with workers that have state/local tax instructions
4. Validate edge cases (missing fields, null values, etc.)
