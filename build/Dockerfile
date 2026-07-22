# Stub only — no active build instructions yet.
#
# Intended multi-stage build once source/ is scaffolded:
#   1. Build stage:   mcr.microsoft.com/dotnet/sdk:10.0
#                      - restore and publish the Api project
#   2. Runtime stage: mcr.microsoft.com/dotnet/aspnet:10.0
#                      - copy published output from the build stage
#                      - EXPOSE 8080
#                      - ENTRYPOINT running the Api project's DLL
#
# Do not add active instructions until the Visual Studio solution
# and .csproj projects exist under source/.
