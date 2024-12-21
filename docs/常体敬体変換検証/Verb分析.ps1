################################################################################
### 動詞を集計する
################################################################################
# 前提となるVerb.csvの作成と仕様確認はVerb分析.Tests.ps1を使うこと。

# 結果集計用のオブジェクト
function Get-ResultObject ($property_set) {
    $o = [pscustomobject] @{
        品詞細分類1 = ""
        品詞細分類2 = ""
        品詞細分類3 = ""
        活用型 = ""
        原形 = ""
        読み = ""
    }

    foreach ($p in $property_set) {
        $o | Add-Member -MemberType NoteProperty -Name $p -Value ""
    }

    return $o
}


# Verbに対して集計
function Verb分析 {
    # 読み込み
    $dat = Import-Csv .\Verb.csv -Encoding Default
    $katuyou_set = $dat | select 活用形 -Unique | foreach {$_.活用形}

    # 原形を軸にして品詞に関するアノテーションは一様であることを確認する。
    $results = @()
    $genkei_grouped = $dat | group 原形
    $genkei_grouped | foreach {
        $_.Group | group 品詞細分類1 | foreach {
            # 「つける」は自立／非自立どちらもあるので品詞細分類1でもgroupingする
            $_.Group | group 活用型 | foreach {
                # 「居（イ）る」は一段、「居（オ）る」は五段・ラ行で区別される

                $o = Get-ResultObject -property_set $katuyou_set
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

    $results | Export-Csv -Encoding Default -NoTypeInformation .\Verb分析.csv
}

# 処理実行
#Verb分析


# 動詞の数を確認したくなったためデバッグ出力コード追加。
#$genkei_grouped | foreach {
#    $_.Group | group 品詞細分類1 | foreach {
#        # 「つける」は自立／非自立どちらもあるので品詞細分類1でもgroupingする
#        $_.Group | group 活用型 | foreach {
#            $_.Name
#        }
#    }
#} | out-file ./genkei.txt

# 活用型ごとにカウント
#Import-Csv .\Verb.csv -Encoding Default | group 活用型 | foreach {
#    "$($_.Name)`t$(@($_.Group | group 原形).Count)"
#}