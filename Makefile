.PHONY: compile build-hello build-helloweb build-hellowebenterprise build-async build-threadcontrol hello helloweb hellowebenterprise async threadcontrol hellowebup

compile:
	dotnet build ./dotnetc-ref.sln

build-hello:
	dotnet build ./hello/Hello.csproj

build-helloweb:
	dotnet build ./helloweb/Helloweb.csproj

build-hellowebenterprise:
	dotnet build ./hellowebenterprise/HellowebEnterprise.csproj

build-async:
	dotnet build ./async/Async.csproj

build-threadcontrol:
	dotnet build ./threadcontrol/ThreadControl.csproj

hello:
	dotnet run --project ./hello/Hello.csproj

helloweb:
	dotnet run --project ./helloweb/Helloweb.csproj

hellowebup:
	dotnet run --project ./helloweb/Helloweb.csproj

hellowebenterprise:
	dotnet run --project ./hellowebenterprise/HellowebEnterprise.csproj

async:
	dotnet run --project ./async/Async.csproj

threadcontrol:
	dotnet run --project ./threadcontrol/ThreadControl.csproj
