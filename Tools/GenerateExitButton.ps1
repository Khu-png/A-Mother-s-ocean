Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$art = Join-Path $root 'Assets/_Game/Resources/Art/SettingsUI'
function RoundRect([float]$x,[float]$y,[float]$w,[float]$h,[float]$radius) {
    $path = [Drawing.Drawing2D.GraphicsPath]::new()
    $d = $radius * 2
    $path.AddArc($x,$y,$d,$d,180,90)
    $path.AddArc(($x+$w-$d),$y,$d,$d,270,90)
    $path.AddArc(($x+$w-$d),($y+$h-$d),$d,$d,0,90)
    $path.AddArc($x,($y+$h-$d),$d,$d,90,90)
    $path.CloseFigure()
    return $path
}
$bitmap = [Drawing.Bitmap]::new(512,160)
$g = [Drawing.Graphics]::FromImage($bitmap)
$g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([Drawing.Color]::Transparent)
$rim = RoundRect 2 2 508 156 44
$g.FillPath([Drawing.Brushes]::Ivory,$rim)
$face = RoundRect 7 7 498 146 39
$gradient = [Drawing.Drawing2D.LinearGradientBrush]::new(
    [Drawing.Point]::new(0,7),[Drawing.Point]::new(0,153),
    [Drawing.ColorTranslator]::FromHtml('#ff8a89'),[Drawing.ColorTranslator]::FromHtml('#df3c52'))
$g.FillPath($gradient,$face)
$highlight = RoundRect 24 17 464 35 17
$shine = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(45,255,255,255))
$g.FillPath($shine,$highlight)
$bitmap.Save((Join-Path $art 'exit-button.png'),[Drawing.Imaging.ImageFormat]::Png)
$shine.Dispose(); $highlight.Dispose(); $gradient.Dispose(); $face.Dispose(); $rim.Dispose()
$g.Dispose(); $bitmap.Dispose()