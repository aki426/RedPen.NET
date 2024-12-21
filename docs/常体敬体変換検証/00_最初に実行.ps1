### ヘッダー追加済みのxlsxファイルから、PSスクリプトで処理するためのCSVファイルを生成する。

function Convert-IPADICXlsxToCsv ($name) {
    if (-not (Test-Path ".\$($name).csv")) {
        # Excelアプリケーションを起動
        $excel = New-Object -ComObject Excel.Application
        $excel.Visible = $false
        $excel.DisplayAlerts = $false

        # ワークブックを開く
        $workbook = $excel.Workbooks.Open("$PWD\$($name).xlsx")

        # Verbシートを選択
        $sheet = $workbook.Sheets.Item($name)

        # CSVとして保存
        $sheet.SaveAs("$PWD\$($name).csv", 6) # 6はCSVフォーマットを示す

        # クリーンアップ
        $workbook.Close()
        $excel.Quit()
        [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel)
    }    
}

# 実行
$names = @("Verb", "Auxil", "Adj", "Noun.adjv")

foreach ($n in $names) {
    Convert-IPADICXlsxToCsv $n
}
