# ASP.NET Core Web API Serverless Application

This project shows how to run an ASP.NET Core Web API project as an AWS Lambda exposed through Amazon API Gateway. The NuGet package [Amazon.Lambda.AspNetCoreServer](https://www.nuget.org/packages/Amazon.Lambda.AspNetCoreServer) contains a Lambda function that is used to translate requests from API Gateway into the ASP.NET Core framework and then the responses from ASP.NET Core back to API Gateway.


For more information about how the Amazon.Lambda.AspNetCoreServer package works and how to extend its behavior view its [README](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.AspNetCoreServer/README.md) file in GitHub.


## .NET 6 configuration

### global.json

```json
"sdk": { "version": "6.0.405" }
```

### *.csproj 

```xml
<TargetFramework>net6.0</TargetFramework>
```

### aws-lambda-tools-defaults.json

```xml
"framework": "net6.0",
"function-runtime": "dotnet6",
"function-architecture": "arm64",
```

### Dockerfile

```Dockerfile
FROM public.ecr.aws/lambda/dotnet:6
```

### [AWS SAM Lambda Output](https://0s785ktk0i.execute-api.us-east-1.amazonaws.com)

```
AWS SAM WebAPI (.NET 6.0.10, amzn.2-arm64, Arm64, Arm64) on AWS Lambda with SAM

processor	: 0
BogoMIPS	: 243.75
Features	: fp asimd evtstrm aes pmull sha1 sha2 crc32 atomics fphp asimdhp cpuid asimdrdm lrcpc dcpop asimddp ssbs
CPU implementer	: 0x41
CPU architecture: 8
CPU variant	: 0x3
CPU part	: 0xd0c
CPU revision	: 1

processor	: 1
BogoMIPS	: 243.75
Features	: fp asimd evtstrm aes pmull sha1 sha2 crc32 atomics fphp asimdhp cpuid asimdrdm lrcpc dcpop asimddp ssbs
CPU implementer	: 0x41
CPU architecture: 8
CPU variant	: 0x3
CPU part	: 0xd0c
CPU revision	: 1
```

## .NET 7 configuration

### global.json

```json
"sdk": { "version": "7.0.102" }
```

### *.csproj 

```xml
<TargetFramework>net7.0</TargetFramework>
```

### aws-lambda-tools-defaults.json

```xml
"framework": "net7.0",
"function-runtime": "dotnet7",
"function-architecture": "arm64",
```

### Dockerfile

```Dockerfile
FROM public.ecr.aws/lambda/dotnet:7
```

### [AWS SAM Lambda Output](https://0s785ktk0i.execute-api.us-east-1.amazonaws.com)

```json
{ "message": "Internal Server Error" }
```

### AWS Log

```
RequestId: b5d40869-04bc-452b-8d82-ffa4134d670c Error: fork/exec /lambda-entrypoint.sh: exec format error
Runtime.InvalidEntrypoint
```

### Configuring for API Gateway HTTP API ###

API Gateway supports the original REST API and the new HTTP API. In addition HTTP API supports 2 different
payload formats. When using the 2.0 format the base class of `LambdaEntryPoint` must be `Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction`.
For the 1.0 payload format the base class is the same as REST API which is `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction`.
**Note:** when using the `AWS::Serverless::Function` CloudFormation resource with an event type of `HttpApi` the default payload
format is 2.0 so the base class of `LambdaEntryPoint` must be `Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction`.


### Configuring for Application Load Balancer ###

To configure this project to handle requests from an Application Load Balancer instead of API Gateway change
the base class of `LambdaEntryPoint` from `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction` to 
`Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction`.

### Project Files ###

* serverless.template - an AWS CloudFormation Serverless Application Model template file for declaring your Serverless functions and other AWS resources
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
* LambdaEntryPoint.cs - class that derives from **Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction**. The code in 
this file bootstraps the ASP.NET Core hosting framework. The Lambda function is defined in the base class.
Change the base class to **Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction** when using an 
Application Load Balancer.
* LocalEntryPoint.cs - for local development this contains the executable Main function which bootstraps the ASP.NET Core hosting framework with Kestrel, as for typical ASP.NET Core applications.
* Startup.cs - usual ASP.NET Core Startup class used to configure the services ASP.NET Core will use.
* web.config - used for local development.
* Controllers\ValuesController - example Web API controller

You may also have a test project depending on the options selected.

## Packaging as a Docker image.

This project is configured to package the Lambda function as a Docker image. The default configuration for the project and the Dockerfile is to build 
the .NET project on the host machine and then execute the `docker build` command which copies the .NET build artifacts from the host machine into 
the Docker image. 

The `--docker-host-build-output-dir` switch, which is set in the `aws-lambda-tools-defaults.json`, triggers the 
AWS .NET Lambda tooling to build the .NET project into the directory indicated by `--docker-host-build-output-dir`. The Dockerfile 
has a **COPY** command which copies the value from the directory pointed to by `--docker-host-build-output-dir` to the `/var/task` directory inside of the 
image.

Alternatively the Docker file could be written to use [multi-stage](https://docs.docker.com/develop/develop-images/multistage-build/) builds and 
have the .NET project built inside the container. Below is an example of building the .NET project inside the image.

```dockerfile
FROM public.ecr.aws/lambda/dotnet:7 AS base

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim as build
WORKDIR /src
COPY ["serverless_image_AspNetCoreWebAPI.csproj", "serverless_image_AspNetCoreWebAPI/"]
RUN dotnet restore "serverless_image_AspNetCoreWebAPI/serverless_image_AspNetCoreWebAPI.csproj"

WORKDIR "/src/serverless_image_AspNetCoreWebAPI"
COPY . .
RUN dotnet build "serverless_image_AspNetCoreWebAPI.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "serverless_image_AspNetCoreWebAPI.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .
```

## Here are some steps to follow from Visual Studio:

To deploy your Serverless application, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed application open the Stack View window by double-clicking the stack name shown beneath the AWS CloudFormation node in the AWS Explorer tree. The Stack View also displays the root URL to your published application.

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "serverless_image_AspNetCoreWebAPI/test/serverless_image_AspNetCoreWebAPI.Tests"
    dotnet test
```

Deploy application
```
    cd "serverless_image_AspNetCoreWebAPI/src/serverless_image_AspNetCoreWebAPI"
    dotnet lambda deploy-serverless
```
