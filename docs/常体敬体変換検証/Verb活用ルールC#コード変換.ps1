### 語幹を取得する関数のSwitch式を作成する。

# Kuromojiによる形態素解析の結果、必ずしもBaseFormが取得できるわけではないので、
# 各活用形から語幹を逆算する必要がある。
# そこで次のような変換テーブルをそのまま実装したようなSwitch式が必要。
# 
# "カ変・クル" => token.InflectionForm switch
# {
#     "基本形" => (true, token.Surface),
#     "仮定形" => (true, $"{token.Surface.RemoveEnd("くれ")}"),
#     "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("くりゃ")}"),
#     "体言接続特殊" => (true, $"{token.Surface.RemoveEnd("くん")}"),
#     "体言接続特殊２" => (true, $"{token.Surface.RemoveEnd("く")}"),
#     "命令ｉ" => (true, $"{token.Surface.RemoveEnd("こい")}"),
#     "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("こよ")}"),
#     "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("こよ")}"),
#     "未然形" => (true, $"{token.Surface.RemoveEnd("こ")}"),
#     "連用形" => (true, $"{token.Surface.RemoveEnd("き")}"),
#     _ => (false, string.Empty)
# },

function 活用ルールから語幹取得コード生成 () {
    $dat = Import-Csv .\Verb活用ルール.csv -Encoding Default
    
    # 活用形名一覧取得。
    $form_names = $dat | Get-Member -MemberType NoteProperty | foreach {$_.Name} | where {$_ -ne "活用型"}

    foreach ($type in $dat) {
        "`"$($type.活用型)`" => token.InflectionForm switch"
        "{"

        # 1つ1つの活用形について活用語尾を末尾から除去したもの＝語幹を返すコードを生成する。
        foreach ($form in $form_names) {
            if ($type.$form -ne "") {
                if ($type.$form -eq "x") {
                    # 語幹と活用形の表層形が同じならRemoveEndしなくていい。
                    "`"$($form)`" => (true, $`"{token.Surface}`"),"
                } else {
                    "`"$($form)`" => (true, $`"{token.Surface.RemoveEnd(`"$($type.$form -replace 'x', '')`")}`"),"
                }
            }
        }

        "_ => (false, string.Empty)"
        "},"
    }

    "_ => (false, string.Empty)"
}

cls
活用ルールから語幹取得コード生成


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
