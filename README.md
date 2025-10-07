# Assignment3 

Study project, RUC 2025

This repository contains a TCP-based server and xUnit tests. The tests require the server to be
running on localhost:5000. 

### Commands to run in PowerShell

- Clean the repo:

```powershell
dotnet clean 
```

- Build the solution:

```powershell
dotnet build 
```

- Start the server (in a separate terminal) — this will block the terminal while the server runs:

```powershell
dotnet run --project Server
```

- Run the full test suite (integration tests expect the server to be running):

```powershell
dotnet test Assignment3TestSuite 
```


Notes
- Run `dotnet build` before `dotnet test --no-build` to ensure the server and test binaries are up-to-date.
- If you get socket errors (connection refused), ensure the server is running and listening on port 5000.
