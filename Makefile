default: examples

books:
	@dotnet run --project src/Books.GraphQl

console:
	@dotnet run --project src/Sandbox.Console

shop:
	@dotnet run --project src/Shop.GraphQl

examples:
	dotnet run --project src/Examples
