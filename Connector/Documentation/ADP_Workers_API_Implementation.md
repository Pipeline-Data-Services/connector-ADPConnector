# ADP Workers API Implementation Guide

## Overview
This implementation follows ADP's best practices for the `/hr/v2/workers` API endpoint, including proper pagination and rate limiting as specified in the ADP API documentation.

## Key Features Implemented

### 1. **Proper HTTP Method**
- ? Uses **GET** method (not POST) as per ADP API specification
- Endpoint: `GET /hr/v2/workers`

### 2. **ADP Pagination**
The implementation uses ADP's OData-style pagination:
- **Query Parameters:**
  - `$skip`: Number of records to skip (for pagination)
  - `$top`: Number of records to return per page (max 100)

**Example Requests:**
```
GET /hr/v2/workers?$skip=0&$top=100    // First page
GET /hr/v2/workers?$skip=100&$top=100  // Second page
GET /hr/v2/workers?$skip=200&$top=100  // Third page
```

### 3. **Rate Limiting**
Implements ADP's recommended rate limiting of **maximum 10 requests per second**:
- Enforces 100ms minimum delay between requests
- Uses semaphore-based thread-safe rate limiting
- Automatically throttles requests across all concurrent calls

### 4. **Automatic Pagination Loop**
The `GetAllWorkersAsync` method automatically:
- Fetches all pages until no more data is available
- Detects end of data when returned records < requested page size
- Aggregates all workers into a single collection
- Handles cancellation tokens properly

### 5. **Error Handling**
- Comprehensive exception handling for HTTP and JSON errors
- Returns partial data if an error occurs mid-pagination
- Includes detailed error messages with context (skip/top values)
- Proper logging at all levels

## Code Structure

### ApiClient.cs

#### Main Methods:

**`GetAllWorkersAsync(CancellationToken cancellationToken)`**
- Public method to fetch all workers with pagination
- Returns: `ApiResponse<IEnumerable<Worker>>`
- Handles: Automatic pagination, rate limiting, error aggregation

**`GetWorkersPageAsync(int skip, int top, CancellationToken cancellationToken)`**
- Private method to fetch a single page
- Returns: `ApiResponse<WorkersResponse>`
- Handles: Single API call with proper error handling

**`EnforceRateLimitAsync(CancellationToken cancellationToken)`**
- Private method to enforce 10 req/sec limit
- Thread-safe using SemaphoreSlim
- Calculates and applies necessary delays

#### Configuration Constants:
```csharp
private const int MinMillisecondsBetweenRequests = 100; // 10 requests per second
private const int DefaultPageSize = 100;                // ADP recommended page size
private const int MaxPageSize = 100;                    // ADP maximum page size
```

### WorkersDataReader.cs

**`GetTypedDataAsync(DataObjectCacheWriteArguments? dataObjectRunArguments, CancellationToken cancellationToken)`**
- Uses `GetAllWorkersAsync` to fetch all workers
- Yields workers one at a time for streaming processing
- Includes comprehensive logging
- Detects and warns about duplicate AssociateOIDs

## Usage Example

```csharp
// In WorkersDataReader
public override async IAsyncEnumerable<Worker> GetTypedDataAsync(
    DataObjectCacheWriteArguments? dataObjectRunArguments, 
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    // Fetch all workers with automatic pagination and rate limiting
    var response = await _apiClient.GetAllWorkersAsync(cancellationToken);

    if (!response.IsSuccessful)
    {
        throw new Exception($"Failed to retrieve workers. StatusCode: {response.StatusCode}");
    }

    foreach (var worker in response.Data)
    {
        yield return worker;
    }
}
```

## ADP API Compliance

### ? API Guidelines Followed:

1. **HTTP Method**: GET (not POST)
2. **Pagination**: OData-style `$skip` and `$top` parameters
3. **Rate Limiting**: Maximum 10 requests per second
4. **Page Size**: Respects ADP's maximum of 100 records per page
5. **End Detection**: Stops when returned records < requested page size
6. **Error Handling**: Graceful degradation with partial data return

### ADP API Response Structure:

```json
{
  "workers": [
    {
      "associateOID": "G3349K45KM6D9WH7",
      "workerID": {
        "idValue": "12345"
      },
      "person": {
        "legalName": {
          "givenName": "John",
          "familyName1": "Doe"
        }
      },
      "workAssignments": [...]
    }
  ]
}
```

## Performance Considerations

### Rate Limiting Impact:
- **Maximum throughput**: 10 requests/second
- **Per-page records**: 100 workers
- **Theoretical max**: 1,000 workers/second
- **For 10,000 workers**: ~10 seconds minimum

### Optimization Tips:
1. The rate limiter is shared across all ApiClient instances
2. Cancellation tokens allow early termination
3. Streaming via `IAsyncEnumerable` prevents memory overflow
4. Partial results returned on error allow data recovery

## Error Scenarios

### Handled Gracefully:
- ? HTTP request failures
- ? JSON deserialization errors
- ? Rate limit violations (automatic delay)
- ? Cancellation requests
- ? Partial page returns

### Error Response Example:
```csharp
if (!response.IsSuccessful)
{
    // Returns:
    // - IsSuccessful: false
    // - StatusCode: HTTP status code
    // - Data: Workers collected before error (if any)
    // - RawResult: Response stream for debugging
}
```

## Logging

The implementation logs:
- ? Start of worker fetch operation
- ? Successful completion with record count
- ? HTTP exceptions with details
- ? API exceptions with status codes
- ? Duplicate AssociateOID warnings
- ? Unexpected errors

## Migration from Legacy Code

### Before:
```csharp
// Old implementation (POST method, no pagination, no rate limiting)
var response = await _httpClient.PostAsJsonAsync("hr/v2/workers", cancellationToken);
```

### After:
```csharp
// New implementation (GET method, auto-pagination, rate limiting)
var response = await _apiClient.GetAllWorkersAsync(cancellationToken);
```

The old `GetWorkers<T>` method is marked as `[Obsolete]` but still works for backwards compatibility.

## Testing Recommendations

1. **Unit Tests**: Mock HttpClient to test pagination logic
2. **Integration Tests**: Use ADP sandbox environment
3. **Load Tests**: Verify rate limiting works under concurrent load
4. **Error Tests**: Test partial data recovery on failures

## Future Enhancements

Potential improvements:
- [ ] Add configurable page size (currently fixed at 100)
- [ ] Support additional ADP query parameters (`$filter`, `$select`)
- [ ] Add retry logic with exponential backoff
- [ ] Implement response caching
- [ ] Add metrics/telemetry for API performance
- [ ] Support for incremental sync using `$filter` on dates

## References

- [ADP Workers API Documentation](https://developers.adp.com/apis/api-explorer/hcm-offrg-ent/hcm-offrg-ent-hr-workers-v2-workers)
- [OData Query Parameters](http://www.odata.org/getting-started/basic-tutorial/)
- [ADP API Rate Limiting Guidelines](https://developers.adp.com/articles/guide/api-guidelines)
