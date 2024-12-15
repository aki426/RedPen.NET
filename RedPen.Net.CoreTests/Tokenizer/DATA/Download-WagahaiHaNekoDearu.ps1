# TLS 1.2を有効化
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# ダウンロード先のURL
$url = "https://www.aozora.gr.jp/cards/000148/files/789_ruby_5639.zip"

# ダウンロードしたZIPファイルの保存先パス
$downloadPath = Join-Path $pwd "downloaded_file.zip"

# 展開先のフォルダパス
$extractPath = Join-Path $pwd "extracted_files"

try {
    Write-Host "ファイルをダウンロードしています..."
    
    # Invoke-WebRequestを使用してファイルをダウンロード
    Invoke-WebRequest -Uri $url -OutFile $downloadPath -UseBasicParsing
    
    if (Test-Path $downloadPath) {
        Write-Host "ダウンロードが完了しました"

        # 展開先フォルダが存在しない場合は作成
        if (-not (Test-Path -Path $extractPath)) {
            New-Item -ItemType Directory -Path $extractPath | Out-Null
        }

        # ZIPファイルを展開
        Write-Host "ファイルを展開しています..."
        Expand-Archive -Path $downloadPath -DestinationPath $extractPath -Force
        Write-Host "展開が完了しました"

        # ダウンロードしたZIPファイルを削除
        Remove-Item -Path $downloadPath
        Write-Host "一時ファイルを削除しました"

        # 不要ファイル削除
        Move-Item -Path (Join-Path $extractPath "wagahaiwa_nekodearu.txt") -Destination $pwd
        Remove-Item -Path $extractPath
    }
    else {
        Write-Error "ファイルのダウンロードに失敗しました"
    }
}
catch {
    Write-Error "エラーが発生しました: $_"
    
    # エラーの詳細情報を表示
    Write-Host "エラーの詳細:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        Write-Host "ステータスコード: $($_.Exception.Response.StatusCode.Value__)"
        Write-Host "ステータスの説明: $($_.Exception.Response.StatusDescription)"
    }
}