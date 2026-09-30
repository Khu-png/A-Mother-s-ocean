Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$art = Join-Path $root 'Assets/_Game/Resources/Art/GameplayUI'
[IO.Directory]::CreateDirectory($art) | Out-Null
function RoundedPath([float]$x,[float]$y,[float]$w,[float]$h,[float]$r) {
    $p = [Drawing.Drawing2D.GraphicsPath]::new()
    $d = 2*$r
    $p.AddArc($x,$y,$d,$d,180,90)
    $p.AddArc(($x+$w-$d),$y,$d,$d,270,90)
    $p.AddArc(($x+$w-$d),($y+$h-$d),$d,$d,0,90)
    $p.AddArc($x,($y+$h-$d),$d,$d,90,90)
    $p.CloseFigure()
    return $p
}
foreach ($icon in @('pause','reset')) {
    foreach ($pressed in @($false,$true)) {
        $bitmap = [Drawing.Bitmap]::new(280,280)
        $g = [Drawing.Graphics]::FromImage($bitmap)
        $g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $g.Clear([Drawing.Color]::Transparent)
        $dy = if ($pressed) { 14 } else { 0 }
        $base = RoundedPath 3 19 274 258 82
        $brush = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#134e70'))
        $g.FillPath($brush,$base); $brush.Dispose(); $base.Dispose()
        $rim = RoundedPath 3 (3+$dy) 274 258 82
        $brush = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#e9f4df'))
        $g.FillPath($brush,$rim); $brush.Dispose(); $rim.Dispose()
        $face = RoundedPath 14 (14+$dy) 252 236 71
        $top = if ($pressed) { '#38b1b8' } else { '#80e5dd' }
        $bottom = if ($pressed) { '#2889a1' } else { '#218ba6' }
        $gradient = [Drawing.Drawing2D.LinearGradientBrush]::new([Drawing.Point]::new(0,(14+$dy)),[Drawing.Point]::new(0,(250+$dy)),[Drawing.ColorTranslator]::FromHtml($top),[Drawing.ColorTranslator]::FromHtml($bottom))
        $g.FillPath($gradient,$face); $gradient.Dispose(); $face.Dispose()
        foreach ($shadow in @($true,$false)) {
            $offset = $dy + $(if ($shadow) { 6 } else { 0 })
            $color = [Drawing.ColorTranslator]::FromHtml($(if ($shadow) { '#197589' } else { '#fff7df' }))
            if ($icon -eq 'pause') {
                $brush = [Drawing.SolidBrush]::new($color)
                foreach ($x in @(90,155)) {
                    $bar = RoundedPath $x (74+$offset) 35 116 10
                    $g.FillPath($brush,$bar); $bar.Dispose()
                }
                $brush.Dispose()
            } else {
                $pen = [Drawing.Pen]::new($color,17)
                $pen.StartCap = [Drawing.Drawing2D.LineCap]::Round
                $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
                $g.DrawArc($pen,86,(78+$offset),108,108,220,305)
                $g.DrawLines($pen,[Drawing.PointF[]]@([Drawing.PointF]::new(87,(72+$offset)),[Drawing.PointF]::new(87,(107+$offset)),[Drawing.PointF]::new(122,(107+$offset))))
                $pen.Dispose()
            }
        }
        $suffix = if ($pressed) { 'pressed' } else { 'normal' }
        $bitmap.Save((Join-Path $art "$icon-$suffix.png"),[Drawing.Imaging.ImageFormat]::Png)
        $g.Dispose(); $bitmap.Dispose()
    }
}
