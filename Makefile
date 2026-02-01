.PHONY: compile run

compile:
	dotnet build ./hello/Hello.csproj

run:
	dotnet run --project ./hello/Hello.csproj
