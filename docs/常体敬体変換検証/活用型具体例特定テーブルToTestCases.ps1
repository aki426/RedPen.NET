### まず活用型具体例特定テーブル.xlsxをCSV出力する。

function CSVファイル群へ変換 () {
    # Excelアプリケーションのインスタンスを作成
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false

    # Excelファイルを開く
    $workbook = $excel.Workbooks.Open("$PWD\動詞活用型具体例特定テーブル.xlsx")

    # 出力フォルダの作成（存在しない場合）
    $outputPath = "$PWD\TestCases"
    if (!(Test-Path -Path $outputPath)) {
        New-Item -ItemType Directory -Path $outputPath
    }

    # 各シートを処理
    foreach ($sheet in $workbook.Sheets) {
        $csvPath = Join-Path $outputPath "$($sheet.Name).csv"
        $sheet.SaveAs($csvPath, [Microsoft.Office.Interop.Excel.XlFileFormat]::xlCSV)
    }

    # クリーンアップ
    $workbook.Close()
    $excel.Quit()
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($workbook) | Out-Null
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
}

CSVファイル群へ変換

function テストケース取得 () {
    $total_counter = 0

    ls .\TestCases | foreach {
        $dat = Import-Csv $_.FullName -Encoding Default

        $results_genzai = @()
        $results_kako = @()

        foreach ($katuyo in $dat | where {$_.現在 -ne ""}) {
            # 現在時制
            $types = @($katuyo.現在活用 -split "・")
            $genzai = @($katuyo.現在 -split "・")

            $counter = 0
            foreach ($surface in $genzai) {
                $total_counter++
                $results_genzai += "[InlineData(`"$($total_counter.ToString('000'))`", `"$($surface)`", `"動詞`", `"$($katuyo.活用型)`", `"$($types[$counter])`")]`n"

                if ($types.Count -gt 1 -and $counter -lt ($types.Count - 1)) {
                    $counter++
                }
            }

            # 過去時制
            $types = @($katuyo.過去活用 -split "・")
            $kako = @($katuyo.過去 -split "・")

            $counter = 0
            foreach ($surface in $kako) {
                $total_counter++
                $results_kako += "[InlineData(`"$($total_counter.ToString('000'))`", `"$($surface)`", `"動詞`", `"$($katuyo.活用型)`", `"$($types[$counter])`")]`n"

                if ($types.Count -gt 1 -and $counter -lt ($types.Count - 1)) {
                    $counter++
                }
            }
        }

        Write-Host -ForegroundColor Cyan "// $($_.Name)"
        Write-Host -ForegroundColor Cyan "// 現在時制"
        Write-Host $results_genzai

        Write-Host -ForegroundColor Cyan "// 過去時制"
        Write-Host $results_kako
    }
}

cls
テストケース取得
