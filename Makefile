default: run

run: shop

books:
	@dotnet run --project src/Books.GraphQl

console:
	@dotnet run --project src/Sandbox.Console

shop:
	@dotnet run --project src/Shop.GraphQl
