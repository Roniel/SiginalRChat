Step
1 -
    dotnet new webapp -o SignalRChat
    code -r SignalRChat
2 -

3 - 
    dotnet tool uninstall -g Microsoft.Web.LibraryManager.Cli
    dotnet tool install -g Microsoft.Web.LibraryManager.Cli
4 -

libman install @microsoft/signalr@latest -p unpkg -d wwwroot/js/signalr --files dist/browser/signalr.js


https://www.datacamp.com/pt/tutorial/deepseek-r1-ollama