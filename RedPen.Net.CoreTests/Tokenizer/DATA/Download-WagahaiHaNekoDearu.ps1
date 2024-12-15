# TLS 1.2を有効化
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# ダウンロード先のURL
$url = "https://www.cl.ecei.tohoku.ac.jp/nlp100/data/neko.txt"

# ダウンロードしたファイルの保存先パス
$downloadPath = Join-Path $pwd "neko.txt"

try {
    Write-Host "ファイルをダウンロードしています..."
    
    # Invoke-WebRequestを使用してファイルをダウンロード
    Invoke-WebRequest -Uri $url -OutFile $downloadPath -UseBasicParsing
    
    if (Test-Path $downloadPath) {
        Write-Host "ダウンロードが完了しました"
        Write-Host "保存先: $downloadPath"
    }
    else {
        Write-Error "ファイルのダウンロードに失敗しました"
    }
}
catch {
    Write-Error "エラーが発生しました: $_"
    Write-Host "エラーの詳細:" -ForegroundColor Red
    Write-Host $_.Exception.Message
}
