# NoitaMod

A work in progress C# modding api for Noita.

## Building

- Run DllExport.bat
- Build NoitaMod.sln and copy all binaries from NoitaMod/bin to the root of your Noita installation (folder with noita.exe)

## Usage

- Drop plugins to `Noita/NoitaMod/plugins/`.
- Run NoitaMod.exe

## Plugins

Plugins are loaded from `Noita/NoitaMod/plugins/*.dll` when NoitaMod is injected.  
For creating plugins, see [/samples](/samples).
