Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$art = Join-Path $root 'Assets/_Game/Resources/Art/SettingsUI'
foreach ($state in @('on', 'off')) {
    $bitmap = [Drawing.Bitmap]::new(256,256)
    $g = [Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([Drawing.Color]::Transparent)
    $pen = [Drawing.Pen]::new([Drawing.Color]::White,18)
    $pen.StartCap = [Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $g.DrawLines($pen,[Drawing.PointF[]]@(
        [Drawing.PointF]::new(91,185),[Drawing.PointF]::new(91,70),
        [Drawing.PointF]::new(190,47),[Drawing.PointF]::new(190,162)))
    $g.FillEllipse([Drawing.Brushes]::White,49,172,51,35)
    $g.FillEllipse([Drawing.Brushes]::White,148,149,51,35)
    if ($state -eq 'off') {
        $g.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
        $gap = [Drawing.Pen]::new([Drawing.Color]::Transparent,30)
        $g.DrawLine($gap,49,43,210,211)
        $gap.Dispose()
        $g.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceOver
        $g.DrawLine($pen,49,43,210,211)
    }
    $bitmap.Save((Join-Path $art "music-$state.png"),[Drawing.Imaging.ImageFormat]::Png)
    $pen.Dispose(); $g.Dispose(); $bitmap.Dispose()
}
