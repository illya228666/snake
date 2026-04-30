# SnakeTimeKiller

Embeddable WPF snake game for .NET Framework 4.8.1.

## Projects

- `src/SnakeTimeKiller` - WPF class library with `SnakeGameControl` and reusable game engine.
- `samples/SnakeTimeKiller.Demo` - small WPF host for manual checks.
- `tests/SnakeTimeKiller.Tests` - no-package console tests for the engine.

## Embed In WPF

Add a reference to `SnakeTimeKiller.dll`, then place the control in XAML:

```xml
<Window
    xmlns:snake="clr-namespace:SnakeTimeKiller;assembly=SnakeTimeKiller">
    <snake:SnakeGameControl
        Rows="16"
        Columns="24"
        TickInterval="0:0:0.120"
        AutoStart="True"
        SnakeHeadImage="{Binding SnakeHeadAsset}"
        SnakeBodyImage="{Binding SnakeBodyAsset}"
        CargoLowImage="{Binding CargoLowAsset}"
        CargoMediumImage="{Binding CargoMediumAsset}"
        CargoHighImage="{Binding CargoHighAsset}" />
</Window>
```

Image properties are optional. When they are not set, the control uses brush-based fallback visuals.

## Build And Test

```powershell
dotnet msbuild SnakeTimeKiller.sln /t:Restore,Build /p:Configuration=Debug
.\tests\SnakeTimeKiller.Tests\bin\Debug\net481\SnakeTimeKiller.Tests.exe
```

Run the demo:

```powershell
.\samples\SnakeTimeKiller.Demo\bin\Debug\net481\SnakeTimeKiller.Demo.exe
```
