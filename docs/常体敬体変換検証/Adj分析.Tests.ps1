# New-Fixture .\ Adj分析
# Set-ExecutionPolicy -Scope Process -ExecutionPolicy RemoteSigned

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

Write-Host -ForegroundColor Yellow "便宜上PesterはWin11標準のVer 3.4.0を使用する。"

Describe "Adj分析" {
    It "Adj.csvの分析" {
        # 読み込み
        $dat = Import-Csv .\Adj.csv -Encoding Default

        # データの素性チェック
        $hinshi_grouped = $dat | group 品詞
        @($hinshi_grouped).Count | Should Be 1 # 形容詞のみ
        $hinshi_grouped.Name | Should Be "形容詞"
        Write-Host -ForegroundColor Cyan "品詞：$($hinshi_grouped.Name)"
        
        # 原形を軸にして品詞に関するアノテーションは一様であることを確認する。
        $genkei_grouped = $dat | group 原形
        @($genkei_grouped).Count | Should Not Be 1
        Write-Host -ForegroundColor Cyan "原形の数：$($genkei_grouped.Count)"

        $genkei_grouped | foreach {
            $_.Group | group 品詞細分類1 | foreach {
                # 「くさい」は自立／接尾どちらもあるので品詞細分類1でもgroupingする
                $_.Group | group 活用型 | foreach {
                    # 「ちっちゃい」には形容詞・アウオ段と形容詞・イ段の2つがある
                    
                    # [DEBUG]
                    #Write-Host "$($_.Group[0].原形)($($_.Name))"

                    @($_.Group | group 品詞).Count | Should Be 1
                    @($_.Group | group 品詞細分類1).Count | Should Be 1
                    @($_.Group | group 品詞細分類2).Count | Should Be 1
                    @($_.Group | group 品詞細分類3).Count | Should Be 1
                    @($_.Group | group 活用型).Count | Should Be 1
                }
            }
        }
    }
}
