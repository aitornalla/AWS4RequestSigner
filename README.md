# AWS4RequestSigner

## Table of contents
* [General info](#general-info)
* [IAws4RequestSigner service](#iaws4requestsigner-service)
* [Aws4SignSettings class](#aws4signsettings-class)
* [How to use the library in an ASP.NET Core project](#how-to-use-the-library-in-an-aspnet-core-project)
* [License](#license)

## General info
This project contains a .NET Standard 2.0 class library for signing HTTP requests using AWS4 algorithm. This class library is intended to be used in an ASP.NET Core project as a service injected by DI, but can be modified to be used in any type of project.

## IAws4RequestSigner service
The only exposed interface of the library is the `IAws4RequestSigner` interface that implements two methods. These two methods are used to add the AWS4 signature to a `HttpRequestMessage` object in a syncrhonous or asynchronous way:
```c#
public interface IAws4RequestSigner
{
    void SignRequest(ref HttpRequestMessage httpRequestMessage, Aws4SignSettings aws4SignSettings);
    Task SignRequestAsync(ref HttpRequestMessage httpRequestMessage, Aws4SignSettings aws4SignSettings);
}
```
Both methods accept a `HttpRequestMessage` object (passed as reference) and a `Aws4SignSettings` object.

## Aws4SignSettings class
This class is used by the service to generate the signature, providing needed information required by the signature algorithm:
```c#
public class Aws4SignSettings
{
    public string AccessKey { get; set; }
    public string Region { get; set; }
    public string SecretKey { get; set; }
    public string ServiceName { get; set; }
}
```

## How to use the library in an ASP.NET Core project
* Add the project *AWS4RequestSigner* to your solution and add a reference to this project.
* In the *Startup.cs* file of your project, add the service.
```c#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddAws4RequestSigner();
    ...
}
```
* Inject and use the service wherever you need it:
```c#
public class TestController : ControllerBase
{
    private readonly IAws4RequestSigner _aws4RequestSigner;

    public TestController(IAws4RequestSigner aws4RequestSigner)
    {
        _aws4RequestSigner = aws4RequestSigner;
    }

    ...
}
```
* Use the async or non-async *SignRequest* method from this service to sign the request.

## License
MIT License
