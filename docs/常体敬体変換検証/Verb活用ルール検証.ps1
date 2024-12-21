
function Remove-EndString {
    param (
        [string]$TargetString,
        [string]$RemoveString
    )
    
    if ($TargetString.EndsWith($RemoveString)) {
        return $TargetString.Substring(0, $TargetString.Length - $RemoveString.Length)
    }
    
    return $TargetString
}

# Verb活用ルール
# "品詞細分類1","品詞細分類2","品詞細分類3","活用型","原形","読み"

function Verb活用ルール検証 {
    $dat = Import-Csv .\Verb分析.csv -Encoding Default
    $rule = @{} # 高速化のためHashtable。
    Import-Csv .\Verb活用ルール.csv -Encoding Default | foreach {
        $rule[$_.活用型] = $_
    }

    # 活用形名の一覧取得。
    $rule_property_set = $rule[@($rule.Keys)[0]] | Get-Member -MemberType NoteProperty | foreach {
        $_.Name
    } | where {$_ -ne "活用型"}

    foreach ($word in $dat) {
        # ルール引き当て
        $r = $rule[$word.活用型]
        
        # 語根特定
        $gokon = Remove-EndString $word.基本形 ($r.基本形 -replace "x", "")

        Write-Host -ForegroundColor Yellow $word.基本形

        foreach ($p in $rule_property_set) {
            if ($word.$p -ne "") {
                #Write-Host $p

                # パターンに語根を足して活用形を導き出す。
                # 合っていたらOK、間違っていたらルール間違いの可能性あり。
                if ($word.$p -eq ($r.$p -replace "x", $gokon)) {
                    # nop
                } else {
                    Write-Host -ForegroundColor Magenta "ERROR: 活用形「$($p)」のパターンでミスマッチ発生。"
                    Write-Host $word
                }
            }
        }
    }
}

#Verb活用ルール検証
