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
The bundled assets from `assets/` are embedded into the DLL and used by default for the cell background, snake head directions, snake body directions, and cargo images. `Backgrownd.png` is rendered once per grid cell. You can override the tiled cell image with `CellBackgroundImage`, and directional snake sprites with `SnakeHead*Image` / `SnakeBody*Image` properties.

Snake sprites are centered on logical cells and can be larger than the cell so adjacent segments visually touch. Tune this with `SnakeVisualScale`, `HorizontalSegmentOverlap`, `VerticalSegmentOverlap`, and `CargoVisualScale`.

## Build And Test

```powershell
dotnet msbuild SnakeTimeKiller.sln /t:Restore,Build /p:Configuration=Debug
.\tests\SnakeTimeKiller.Tests\bin\Debug\net481\SnakeTimeKiller.Tests.exe
```

Run the demo:

```powershell
.\samples\SnakeTimeKiller.Demo\bin\Debug\net481\SnakeTimeKiller.Demo.exe
```
