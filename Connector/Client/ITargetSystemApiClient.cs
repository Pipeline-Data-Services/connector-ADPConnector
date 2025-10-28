namespace Connector.Client;

/// <summary>
/// This interface to used for the xchange CLI tool to identify the class name of the target system's client.
/// Knowing the class name allows for generated code to set up things like dependency injection.
/// If the CLI cannot determine the class name of the client then it will forgo some aspects of code generation.
/// This interface does not need to define any methods, it has no functional purposes outside of code generation.
/// </summary>
public interface ITargetSystemApiClient
{
    
}