.PHONY: default
default: console

.PHONY: graphql
graphql:
	@dotnet run --project src/Books.GraphQl

.PHONY: console
console:
	@dotnet run --project src/Sandbox.Console
