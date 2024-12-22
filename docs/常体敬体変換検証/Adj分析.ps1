################################################################################
### 形容詞を集計する
################################################################################
# 「00_最初に実行.ps1」、仕様確認は「*分析.Tests.ps1」を使うこと。

# Set-ExecutionPolicy -Scope Process -ExecutionPolicy RemoteSigned
. .\Get-ResultObject.ps1

function Adj分析 {
    # 読み込み
    $dat = Import-Csv .\Adj.csv -Encoding Default
    $katuyou_set = $dat | select 活用形 -Unique | foreach {$_.活用形}

    # 原形を軸にして品詞に関するアノテーションは一様であることを確認する。
    $results = @()
    $genkei_grouped = $dat | group 原形
    $genkei_grouped | foreach {
        $_.Group | group 品詞細分類1 | foreach {
            $_.Group | group 活用型 | foreach {

                $o = Get-ResultObject -PropertySet $katuyou_set
                $o."品詞細分類1" = $_.Group[0].品詞細分類1
                $o."品詞細分類2" = $_.Group[0].品詞細分類2
                $o."品詞細分類3" = $_.Group[0].品詞細分類3
                $o."活用型" = $_.Group[0].活用型
                $o."原形" = $_.Group[0].原形
                $o."読み" = @($_.Group | foreach {$_.読み}) -join ","
                
                # 各活用形の記録
                $_.Group | foreach {
                    $o.($_.活用形) = $_.表層形
                }

                $results += $o
            }
        }
    }

    $results | Export-Csv -Encoding Default -NoTypeInformation .\Adj分析.csv
}


Adj分析

# 活用型ごとにカウント
Import-Csv .\Adj.csv -Encoding Default | group 活用型 | foreach {
    "$($_.Name)`t$(@($_.Group | group 原形).Count)"
}


