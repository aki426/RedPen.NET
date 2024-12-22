# 結果集計用のオブジェクト
function Get-ResultObject {
    param(
        [Parameter(Mandatory)]
        $PropertySet
    )

    $o = [pscustomobject] @{
        品詞細分類1 = ""
        品詞細分類2 = ""
        品詞細分類3 = ""
        活用型 = ""
        原形 = ""
        読み = ""
    }

    foreach ($p in $PropertySet) {
        $o | Add-Member -MemberType NoteProperty -Name $p -Value ""
    }

    return $o
}
