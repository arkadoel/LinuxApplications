PROJECT_OR_SLN = ./Fer.MonitorConfig.csproj

.PHONY: all build rebuild clean run test publish

all: build

build:
	@echo "Compilando la solución..."
	dotnet build 

rebuild: clean build
	@echo "Reconstrucción completada."

clean:
	@echo "Limpiando los archivos de compilación..."
	dotnet clean $(PROJECT_OR_SLN)

run:
	@echo "Ejecutando el proyecto..."
	dotnet run --project $(PROJECT_OR_SLN) -v m

test:
	@echo "Ejecutando las pruebas..."
	dotnet test

publish:
	@echo "Publicando la aplicación para producción..."
	dotnet publish -c Release

snap:
	clear
	@echo "Publicando la aplicación para producción..."
	dotnet publish -c Release -r linux-x64 --self-contained true
	sudo snapcraft pack
