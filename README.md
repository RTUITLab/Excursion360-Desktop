# Desktop viewer for 360 excursions

## CLI Options
Run exe file with `--firefox` tto force using firefox instead selecting on start.

## Build

### Windows
```bash
dotnet publish -r win-x64 -c Release --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```
