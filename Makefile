.PHONY: compile run hellowebup

compile:
	dotnet build ./hello/Hello.csproj

hello:
	dotnet run --project ./hello/Hello.csproj

hellowebup:
	dotnet run --project ./helloweb/Helloweb.csproj
