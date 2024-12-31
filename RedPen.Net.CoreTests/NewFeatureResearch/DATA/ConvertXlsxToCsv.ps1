### ヘッダー追加済みのxlsxファイルから、PSスクリプトで処理するためのCSVファイルを生成する。

function Convert-XlsxToCsv ($name, [switch]$Force) {
    if ($Force -or (-not (Test-Path ".\$($name).csv"))) {
        # Excelアプリケーションを起動
        $excel = New-Object -ComObject Excel.Application
        $excel.Visible = $false
        $excel.DisplayAlerts = $false

        # ワークブックを開く
        $workbook = $excel.Workbooks.Open("$PWD\$($name).xlsx")

        # シートを選択
        $sheet = $workbook.Sheets.Item($name)

        # CSVとして保存
        # xlCSVUTF8	62	UTF8 CSV	*.csv
        $sheet.SaveAs("$PWD\$($name).csv", 62)

        # クリーンアップ
        $workbook.Close()
        $excel.Quit()
        [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel)
    }    
}

# 実行
$names = @("常体敬体変換総引き当て表")

foreach ($n in $names) {
    Convert-XlsxToCsv $n -Force
}
