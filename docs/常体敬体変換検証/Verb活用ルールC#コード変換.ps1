
### 次のような活用変化テーブルを読み替えたスイッチ式を作りたい。

# "五段・ワ行促音便" => type switch
# {
#     "基本形" => (true, token.BaseForm),
#     "未然形" => (true, $"{token.BaseForm.RemoveEnd("う")}わ"),
#     "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("う")}お"),
#     "連用形" => (true, $"{token.BaseForm.RemoveEnd("う")}い"),
#     "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("う")}っ"),
#     "仮定形" => (true, $"{token.BaseForm.RemoveEnd("う")}え"),
#     "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("う")}え"),
#     _ => (false, string.Empty)
# },


function 活用ルール表変換 () {
    $dat = Import-Csv .\Verb活用ルール.csv -Encoding Default
    
    $form_names = $dat | Get-Member -MemberType NoteProperty | foreach {$_.Name} | where {$_ -ne "活用型" -and $_ -ne "基本形"}

    foreach ($type in $dat) {
        "`"$($type.活用型)`" => type switch"
        "{"

        "`"基本形`" => (true, token.BaseForm),"

        $base_suffix = $type.基本形 -replace "x", ""
        foreach ($form in $form_names) {
            if ($type.$form -ne "") {
                "`"$($form)`" => (true, $`"{token.BaseForm.RemoveEnd(`"$($base_suffix)`")}$($type.$form -replace 'x', '')`"),"
            }
        }

        "_ => (false, string.Empty)"
        "},"
    }

    "_ => (false, string.Empty)"
}

cls
活用ルール表変換
