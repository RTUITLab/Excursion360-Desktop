# Desktop viewer for 360 excursions

## CLI Options
Run executable file with `--firefox true` to force using firefox instead selecting on start.

## Build

### Windows x64
```bash
dotnet publish -r win-x64 -c Release --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```
