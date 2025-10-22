To run this repository as an executable file on a MAC machine run the following command
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true

To run this repository as an executable file on a windows machine run the following command
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
