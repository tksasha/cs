.PHONY: default
default: Examples

%:
	dotnet run --project src/$@
