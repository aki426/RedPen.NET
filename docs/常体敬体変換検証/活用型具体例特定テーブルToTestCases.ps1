### �܂����p�^��̗����e�[�u��.xlsx��CSV�o�͂���B

function CSV�t�@�C���Q�֕ϊ� () {
    # Excel�A�v���P�[�V�����̃C���X�^���X���쐬
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false

    # Excel�t�@�C�����J��
    $workbook = $excel.Workbooks.Open("$PWD\�������p�^��̗����e�[�u��.xlsx")

    # �o�̓t�H���_�̍쐬�i���݂��Ȃ��ꍇ�j
    $outputPath = "$PWD\TestCases"
    if (!(Test-Path -Path $outputPath)) {
        New-Item -ItemType Directory -Path $outputPath
    }

    # �e�V�[�g������
    foreach ($sheet in $workbook.Sheets) {
        $csvPath = Join-Path $outputPath "$($sheet.Name).csv"
        $sheet.SaveAs($csvPath, [Microsoft.Office.Interop.Excel.XlFileFormat]::xlCSV)
    }

    # �N���[���A�b�v
    $workbook.Close()
    $excel.Quit()
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($workbook) | Out-Null
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
}

CSV�t�@�C���Q�֕ϊ�

function �e�X�g�P�[�X�擾 () {
    $total_counter = 0

    ls .\TestCases | foreach {
        $dat = Import-Csv $_.FullName -Encoding Default

        $results_genzai = @()
        $results_kako = @()

        foreach ($katuyo in $dat | where {$_.���� -ne ""}) {
            # ���ݎ���
            $types = @($katuyo.���݊��p -split "�E")
            $genzai = @($katuyo.���� -split "�E")

            $counter = 0
            foreach ($surface in $genzai) {
                $total_counter++
                $results_genzai += "[InlineData(`"$($total_counter.ToString('000'))`", `"$($surface)`", `"����`", `"$($katuyo.���p�^)`", `"$($types[$counter])`")]`n"

                if ($types.Count -gt 1 -and $counter -lt ($types.Count - 1)) {
                    $counter++
                }
            }

            # �ߋ�����
            $types = @($katuyo.�ߋ����p -split "�E")
            $kako = @($katuyo.�ߋ� -split "�E")

            $counter = 0
            foreach ($surface in $kako) {
                $total_counter++
                $results_kako += "[InlineData(`"$($total_counter.ToString('000'))`", `"$($surface)`", `"����`", `"$($katuyo.���p�^)`", `"$($types[$counter])`")]`n"

                if ($types.Count -gt 1 -and $counter -lt ($types.Count - 1)) {
                    $counter++
                }
            }
        }

        Write-Host -ForegroundColor Cyan "// $($_.Name)"
        Write-Host -ForegroundColor Cyan "// ���ݎ���"
        Write-Host $results_genzai

        Write-Host -ForegroundColor Cyan "// �ߋ�����"
        Write-Host $results_kako
    }
}

cls
�e�X�g�P�[�X�擾
