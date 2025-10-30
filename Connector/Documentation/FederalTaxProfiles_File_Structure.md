# Federal Tax Profiles File Structure Documentation

## Overview
The Federal Tax Profiles functionality follows the same pattern as Workers, with separate files for API deserialization (DTOs) and internal data models.

## File Structure

### 1. FederalTaxProfile.cs
**Purpose**: API Response Models with JSON Serialization

This file contains all the classes that directly map to the ADP Payroll US Tax Profiles API JSON response structure. These classes include `[JsonPropertyName]` attributes to ensure proper deserialization from the ADP API.

**API Endpoint**: `GET /payroll/v1/workers/{aoid}/us-tax-profiles`

**Key Classes**:
- `USTaxProfilesResponse` - Root wrapper for API response
- `USTaxProfile` - Main tax profile entity (with JSON annotations)
- All supporting DTOs with JSON property mappings:
  - `TaxProfileWorkerDTO` - Worker reference in tax context
  - `TaxWorkerIDDTO` - Worker ID
  - `TaxWithholdingStatusDTO` - Tax withholding status
  - `FederalTaxWithholdingDTO` - Federal tax withholding details
  - `StateTaxWithholdingDTO` - State tax withholding details
  - `LocalTaxWithholdingDTO` - Local tax withholding details
  - `TaxAmountDTO` - Tax amount with currency
  - `TaxCodeValueDTO` - Generic code value

**Characteristics**:
- ? All classes have `[JsonPropertyName]` attributes
- ? Property names match ADP API exactly (camelCase)
- ? Used for API deserialization
- ? Nullable properties (?) for optional fields

### 2. FederalTaxProfilesDataObject.cs
**Purpose**: Internal Data Model for Xchange System

This file contains the `FederalTaxProfilesDataObject` class which is the internal representation used within the Xchange connector system.

**Key Class**:
- `FederalTaxProfilesDataObject` - Internal data model

**Supporting Classes** (without JSON annotations):
- `TaxProfileWorker` - Worker reference
- `TaxWorkerID` - Worker ID
- `TaxWithholdingStatus` - Tax withholding status
- `FederalTaxWithholding` - Federal tax withholding
- `StateTaxWithholding` - State tax withholding
- `LocalTaxWithholding` - Local tax withholding
- `TaxAmount` - Tax amount
- `TaxCodeValue` - Code value

**Characteristics**:
- ? No JSON property annotations
- ? Has `[PrimaryKey]` and `[Description]` attributes for Xchange schema generation
- ? Uses `[AllowNull]` attribute for nullable properties
- ? DateTime types instead of strings for dates
- ? Default values initialized (e.g., `= string.Empty`, `= new()`)

## Data Flow

```
ADP API Response (JSON)
         ?
 [API Client Call]
         ?
USTaxProfilesResponse (FederalTaxProfile.cs)
         ?
 [Mapping in FederalTaxProfilesDataReader]
         ?
FederalTaxProfilesDataObject (FederalTaxProfilesDataObject.cs)
         ?
Xchange System Processing
```

## Process Flow

### Step 1: Fetch Workers
```csharp
var workersResponse = await _apiClient.GetAllWorkersAsync(cancellationToken);
```

### Step 2: Loop Through Workers
```csharp
foreach (var worker in workersList)
{
    // Get tax profile for each worker
}
```

### Step 3: Fetch Tax Profile for Each Worker
```csharp
var taxProfileResponse = await _apiClient.GetWorkerTaxProfileAsync(
    worker.AssociateOID, 
    cancellationToken);
```

### Step 4: Map and Collect
```csharp
foreach (var taxProfile in taxProfileResponse.Data.UsTaxProfiles)
{
    var mappedProfile = MapToFederalTaxProfilesDataObject(taxProfile);
    federalTaxProfiles.Add(mappedProfile);
}
```

### Step 5: Yield Return All
```csharp
foreach (var dataObject in federalTaxProfiles)
{
    yield return dataObject;
}
```

## Usage in Code

### ApiClient.cs
```csharp
// New method added for tax profiles
public async Task<ApiResponse<USTaxProfilesResponse>> GetWorkerTaxProfileAsync(
    string associateOID,
    CancellationToken cancellationToken = default)
{
    // Enforces rate limiting
    // Calls: GET /payroll/v1/workers/{aoid}/us-tax-profiles
    // Deserializes JSON to USTaxProfilesResponse using JsonPropertyName attributes
}
```

### FederalTaxProfilesDataReader.cs
```csharp
public override async IAsyncEnumerable<FederalTaxProfilesDataObject> GetTypedDataAsync(...)
{
    // 1. Get all workers
    var workersResponse = await _apiClient.GetAllWorkersAsync(cancellationToken);
    
    // 2. Loop through each worker
    foreach (var worker in workersList)
    {
        // 3. Get tax profile for this worker
        var taxProfileResponse = await _apiClient.GetWorkerTaxProfileAsync(
            worker.AssociateOID, 
            cancellationToken);
        
        // 4. Map each tax profile
        foreach (var taxProfile in taxProfileResponse.Data.UsTaxProfiles)
        {
            var mappedProfile = MapToFederalTaxProfilesDataObject(taxProfile);
            federalTaxProfiles.Add(mappedProfile);
        }
    }
    
    // 5. Yield return all profiles
    foreach (var dataObject in federalTaxProfiles)
    {
        yield return dataObject;
    }
}
```

## Key Features

### Rate Limiting
- Enforces ADP's rate limit of 10 requests per second
- Automatically applied in `ApiClient.GetWorkerTaxProfileAsync()`

### Error Handling
- Logs warnings for individual worker failures
- Continues processing other workers
- Tracks success/failure counts
- Progress logging every 100 workers

### Mapping Logic
- Date string ? DateTime conversion
- Nullable DTO properties ? Non-nullable data objects with defaults
- Handles null/missing data gracefully
- Uses helper methods for reusable mapping

## Data Structure

### Federal Tax Withholding
Contains W-4 form information:
- Filing status (Single, Married, etc.)
- Withholding allowances
- Dependents quantity
- Additional withholding amounts
- Multiple jobs indicator
- Tax form code (W-4 2019/2020)

### State Tax Withholding
State-specific withholding:
- State code
- Filing status
- Withholding allowances
- Additional withholding (amount or percentage)
- Residency status

### Local Tax Withholding
Local jurisdiction withholding:
- Locality code
- Filing status
- Additional withholding
- Exemption codes

## Primary Key

The `FederalTaxProfilesDataObject` uses:
- **Primary Key**: `ProfileId` 
  - Derived from `ItemID` in API response
  - Falls back to `Guid.NewGuid().ToString()` if ItemID is null

## Important Notes

1. **One API call per worker** - Tax profiles are fetched individually for each worker
2. **Rate limiting is critical** - Enforced automatically in ApiClient
3. **Error resilience** - Failures for individual workers don't stop the entire process
4. **Multiple tax profiles** - A worker can have multiple tax profiles (returned as list)
5. **Date parsing** - Converts string dates to DateTime, defaults to DateTime.MinValue on failure

## File Locations

```
Connector/
??? App/
?   ??? v1/
?       ??? FederalTaxProfiles/
?           ??? FederalTaxProfile.cs              # API DTOs with JSON attributes
?           ??? FederalTaxProfilesDataObject.cs   # Internal Xchange model
?           ??? FederalTaxProfilesDataReader.cs   # Data reader with mapping logic
??? Client/
?   ??? ApiClient.cs                              # API client with GetWorkerTaxProfileAsync
??? Documentation/
    ??? FederalTaxProfiles_File_Structure.md      # This file
```

## Modification Guidelines

### When to modify FederalTaxProfile.cs:
- ADP API adds/removes/renames fields in tax profile endpoint
- Need to deserialize additional API properties
- API response structure changes

### When to modify FederalTaxProfilesDataObject.cs:
- Xchange system requirements change
- Need different schema descriptions
- Need to add alternate keys
- Internal data model needs to diverge from API model

### When to modify mapping in FederalTaxProfilesDataReader.cs:
- Property names differ between API and internal model
- Need data transformation during mapping
- Need validation or business logic during conversion
- Error handling requirements change

## Testing Considerations

1. **Test with workers that have no tax profiles** - Should handle gracefully
2. **Test with workers that have multiple tax profiles** - Should map all
3. **Test API failures** - Should continue processing other workers
4. **Test rate limiting** - Verify proper throttling
5. **Test large worker counts** - Verify progress logging and performance

## Performance Considerations

- **Network calls**: N+1 pattern (1 call for workers + N calls for tax profiles)
- **Rate limiting**: ~10 seconds per 100 workers (due to rate limiting)
- **Memory**: Collects all tax profiles in memory before yielding
- **Cancellation**: Supports cancellation token for graceful shutdown

## Example Response Structure

```json
{
  "usTaxProfiles": [
    {
      "itemID": "12345",
      "associateOID": "G3R8CTYAHF328GQH",
      "federalTaxWithholding": {
        "filingStatusCode": {
          "codeValue": "M",
          "shortName": "Married",
          "longName": "Married"
        },
        "withholdingAllowanceQuantity": 2,
        "dependentsQuantity": 1,
        "additionalWithholdingAmount": {
          "amountValue": 50.00,
          "currencyCode": "USD"
        },
        "taxFormCode": {
          "codeValue": "W4_2020"
        }
      },
      "stateTaxWithholdings": [...],
      "localTaxWithholdings": [...]
    }
  ]
}
```
