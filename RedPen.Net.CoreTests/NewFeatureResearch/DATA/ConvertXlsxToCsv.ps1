### ヘッダー追加済みのxlsxファイルから、PSスクリプトで処理するためのCSVファイルを生成する。

function Convert-XlsxToCsv ($name, [switch]$Force) {
    # Excelアプリケーションを起動
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    
    try {
        # ワークブックを開く
        $workbook = $excel.Workbooks.Open("$PWD\$($name).xlsx")
        
        # すべてのシートを処理
        foreach ($sheet in $workbook.Sheets) {
            $sheetName = $sheet.Name
            $csvPath = ".\$($sheetName).csv"
            
            # Forceフラグがない場合は既存ファイルをスキップ
            if ($Force -or (-not (Test-Path $csvPath))) {
                Write-Host "Converting sheet: $sheetName"
                # xlCSVUTF8	62	UTF8 CSV	*.csv
                $sheet.SaveAs("$PWD\$($sheetName).csv", 62)
            }
        }
    }
    finally {
        # クリーンアップ
        if ($workbook) {
            $workbook.Close()
        }
        $excel.Quit()
        [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel)
    }
}

# 実行
$names = @("常体敬体変換総引き当て表")

foreach ($n in $names) {
    Convert-XlsxToCsv $n -Force
}
