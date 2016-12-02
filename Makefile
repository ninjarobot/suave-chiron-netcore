all:
	cd SuaveChironNetcore && dotnet restore && dotnet build
	cd SuaveChironNetcoreTests && dotnet restore && dotnet build

check:
	cd SuaveChironNetcoreTests && dotnet test

test: check

clean:
	rm -rf SuaveChironNetcore/bin SuaveChironNetcore/obj
	rm -rf SuaveChironNetcoreTests/bin SuaveChironNetcoreTests/obj