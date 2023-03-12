# Icarus Server Query

A command line program that queries an Icarus dedicated server for information.

## Releases

Releases can be found [here](https://github.com/CrystalFerrai/IcarusServerQuery/releases). There is no installer, just unzip the contents to a location on your hard drive.

You will need to have the .NET Runtime 6.0 x64 installed. You can find the latest .NET 6 downloads [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0). Look for ".NET Runtime" or ".NET Desktop Runtime" (which includes .NET Runtime). Download and install the x64 version for your OS.

## How to Use

Open a command prompt (cmd) wherever you downloaded IcarusServerQuery and run the following command, substituting your server ip and query port.
```
IcarusServerQuery YourServerIP:YourServerQueryPort
```

Example input:
```
IcarusServerQuery 192.168.1.50:27015
```

Example output:
```
Server: SomeTestServer
Ping: 0.14ms
Version: 1.2.41.108522
QueryPort: 27015
Port: 17777
Status: Open World
Prospect: OpenWorld_Styx
Save: Test
Difficulty: Medium
Hard Core: No

Associated characters: 2
  CRYSTAL
  SARA

Online Players: 1/8
  CRYSTAL - 01:07:58
```

## How to Build

If you want to build from source, follow these steps.
1. Clone the repo, including submodules.
    ```
    git clone --recursive https://github.com/CrystalFerrai/IcarusServerQuery.git
    ```
2. Open the file `IcarusServerQuery.sln` in Visual Studio.
3. Build the solution.

## Support

This is just one of my many free time projects. No support or documentation is offered beyond this readme.
