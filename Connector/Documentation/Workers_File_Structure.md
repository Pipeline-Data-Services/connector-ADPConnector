# Workers File Structure Documentation

## Overview
The Workers functionality is split across two files to separate concerns between API deserialization and internal data models.

## File Structure

### 1. Workers.cs
**Purpose**: API Response Models with JSON Serialization

This file contains all the classes that directly map to the ADP API JSON response structure. These classes include `[JsonPropertyName]` attributes to ensure proper deserialization from the ADP API.

**Key Classes**:
- `WorkersResponse` - Root wrapper for API response
- `Worker` - Main worker entity (with JSON annotations)
- All supporting DTOs with JSON property mappings:
  - `WorkerID`
  - `Person`
  - `LegalName`
  - `PreferredName`
  - `Address`
  - `Communication`
  - `PhoneNumber`
  - `Email`
  - `BusinessCommunication`
  - `WorkAssignment`
  - `WorkerStatus`
  - `AssignmentStatus`
  - `OrganizationalUnit`
  - `OccupationalClassification`
  - `WageLawCoverage`
  - `BaseRemuneration`
  - `AdditionalRemuneration`
  - `Amount`
  - `StandardHours`
  - `WorkLocation`
  - `ReportsTo`
  - `ReportsToWorkerName`
  - `CodeValue`
  - And more...

**Characteristics**:
- ? All classes have `[JsonPropertyName]` attributes
- ? Property names match ADP API exactly (camelCase)
- ? Used for API deserialization
- ? `Worker` class has `[PrimaryKey]` attribute for tracking

### 2. WorkersDataObject.cs
**Purpose**: Internal Data Model for Xchange System

This file contains the `WorkersDataObject` class which is the internal representation used within the Xchange connector system.

**Key Class**:
- `WorkersDataObject` - Internal data model

**Characteristics**:
- ? No JSON property annotations
- ? Has `[PrimaryKey]` and `[Description]` attributes for Xchange schema generation
- ? Reuses the same supporting classes from Workers.cs
- ? Used in data reader yield returns

## Data Flow

```
ADP API Response (JSON)
         ?
    [Deserialization]
         ?
   Worker (Workers.cs)
         ?
   [Mapping in WorkersDataReader]
         ?
WorkersDataObject (WorkersDataObject.cs)
         ?
  Xchange System Processing
```

## Usage in Code

### ApiClient.cs
```csharp
// Returns Worker objects (from Workers.cs)
public async Task<ApiResponse<IEnumerable<Worker>>> GetAllWorkersAsync(...)
{
    // Deserializes JSON to Worker using JsonPropertyName attributes
    var workersResponse = JsonSerializer.Deserialize<WorkersResponse>(...);
    return workersResponse.Workers;
}
```

### WorkersDataReader.cs
```csharp
// Yields WorkersDataObject (from WorkersDataObject.cs)
public override async IAsyncEnumerable<WorkersDataObject> GetTypedDataAsync(...)
{
    // Get Worker objects from API
    var response = await _apiClient.GetAllWorkersAsync(cancellationToken);
    
    // Map to WorkersDataObject for internal use
    foreach (var worker in response.Data)
    {
        yield return MapToWorkersDataObject(worker);
    }
}

private WorkersDataObject MapToWorkersDataObject(Worker worker)
{
    // Simple property mapping since both share the same structure
    return new WorkersDataObject
    {
        AssociateOID = worker.AssociateOID,
        WorkerID = worker.WorkerID,
        Person = worker.Person,
        BusinessCommunication = worker.BusinessCommunication,
        WorkAssignments = worker.WorkAssignments
    };
}
```

## Why Two Separate Classes?

### Separation of Concerns
1. **Workers.cs (Worker)**: 
   - Knows about ADP API JSON structure
   - Tightly coupled to external API
   - Changes when API changes

2. **WorkersDataObject.cs (WorkersDataObject)**:
   - Knows about Xchange system requirements
   - Independent of external API structure
   - Changes when Xchange schema requirements change

### Benefits
- ? **Flexibility**: Can modify internal model without affecting API deserialization
- ? **Clarity**: Clear distinction between external API and internal data
- ? **Maintainability**: API changes don't directly impact internal processing
- ? **Testing**: Can test API deserialization and internal logic separately
- ? **Schema Generation**: Xchange attributes only on internal model

## Shared Classes

Both `Worker` and `WorkersDataObject` share the same supporting classes:
- `WorkerID`
- `Person`
- `BusinessCommunication`
- `WorkAssignment`
- `CodeValue`
- And all other DTOs...

These are defined in `Workers.cs` and referenced by both models since they have the same structure.

## Important Notes

1. **Don't duplicate class definitions** - All DTOs should only be defined once in Workers.cs
2. **JSON attributes only in Workers.cs** - Only Worker and its related classes need JsonPropertyName
3. **Xchange attributes only in WorkersDataObject.cs** - Only WorkersDataObject needs PrimaryKey and Description
4. **Mapping is lightweight** - Since both models share the same property structure, mapping is simple property assignment

## File Locations

```
Connector/
??? App/
?   ??? v1/
?       ??? Workers/
?           ??? Workers.cs              # API models with JSON attributes
?           ??? WorkersDataObject.cs    # Internal Xchange model
?           ??? WorkersDataReader.cs    # Data reader with mapping logic
??? Client/
    ??? ApiClient.cs                    # API client returning Worker objects
```

## Modification Guidelines

### When to modify Workers.cs:
- ADP API adds/removes/renames fields
- Need to deserialize additional API properties
- API response structure changes

### When to modify WorkersDataObject.cs:
- Xchange system requirements change
- Need different schema descriptions
- Need to add alternate keys
- Internal data model needs to diverge from API model

### When to modify mapping in WorkersDataReader.cs:
- Property names differ between API and internal model
- Need data transformation during mapping
- Need validation or business logic during conversion
