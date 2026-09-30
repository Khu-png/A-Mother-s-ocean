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
$g.FillPath([Drawing.Brushes]::Honeydew,$rim)
$face = RoundRect 7 7 498 146 39
$gradient = [Drawing.Drawing2D.LinearGradientBrush]::new(
    [Drawing.Point]::new(0,7),[Drawing.Point]::new(0,153),
    [Drawing.ColorTranslator]::FromHtml('#90e57b'),[Drawing.ColorTranslator]::FromHtml('#35aa55'))
$g.FillPath($gradient,$face)
$highlight = RoundRect 24 17 464 35 17
$shine = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(45,255,255,255))
$g.FillPath($shine,$highlight)
$bitmap.Save((Join-Path $art 'tutorial-button.png'),[Drawing.Imaging.ImageFormat]::Png)
$shine.Dispose(); $highlight.Dispose(); $gradient.Dispose(); $face.Dispose(); $rim.Dispose()
$g.Dispose(); $bitmap.Dispose()
$bitmap = [Drawing.Bitmap]::new(128,128)
$g = [Drawing.Graphics]::FromImage($bitmap)
$g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([Drawing.Color]::Transparent)
$pen = [Drawing.Pen]::new([Drawing.Color]::White,8)
$pen.LineJoin = [Drawing.Drawing2D.LineJoin]::Round
$pen.StartCap = [Drawing.Drawing2D.LineCap]::Round
$pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
$cuff = RoundRect 13 58 22 52 4
$g.DrawPath($pen,$cuff)
$thumb = [Drawing.Drawing2D.GraphicsPath]::new()
$thumb.AddLines([Drawing.Point[]]@(
    [Drawing.Point]::new(35,64),[Drawing.Point]::new(53,45),
    [Drawing.Point]::new(63,17),[Drawing.Point]::new(73,17),
    [Drawing.Point]::new(79,30),[Drawing.Point]::new(75,52),
    [Drawing.Point]::new(108,52),[Drawing.Point]::new(115,61),
    [Drawing.Point]::new(105,102),[Drawing.Point]::new(96,110),
    [Drawing.Point]::new(54,110),[Drawing.Point]::new(35,100)))
$thumb.CloseFigure()
$g.DrawPath($pen,$thumb)
$bitmap.Save((Join-Path $art 'thumb-up-icon.png'),[Drawing.Imaging.ImageFormat]::Png)
$thumb.Dispose(); $cuff.Dispose(); $pen.Dispose(); $g.Dispose(); $bitmap.Dispose()
