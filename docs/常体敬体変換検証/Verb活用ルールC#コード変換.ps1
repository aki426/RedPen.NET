################################################################################
### 語幹を取得する関数のSwitch式を作成する。
################################################################################

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
                    "`"$($form)`" => (true, token.Surface),"
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


################################################################################
### TokenElementから活用形を取得する関数のSwitch式を作成する。
################################################################################

# 次のような活用変化テーブルを読み替えたスイッチ式を作りたい。
# gokanは語幹を表すStringで語幹取得関数からあらかじめ取得するものとする。
# 
# "カ変・クル" => type switch
# {
#     "基本形" => (true, $"{gokan}くる"),
#     "仮定形" => (true, $"{gokan}くれ"),
#     "仮定縮約１" => (true, $"{gokan}くりゃ"),
#     "体言接続特殊" => (true, $"{gokan}くん"),
#     "体言接続特殊２" => (true, $"{gokan}く"),
#     "命令ｉ" => (true, $"{gokan}こい"),
#     "命令ｙｏ" => (true, $"{gokan}こよ"),
#     "未然ウ接続" => (true, $"{gokan}こよ"),
#     "未然形" => (true, $"{gokan}こ"),
#     "連用形" => (true, $"{gokan}き"),
#     _ => (false, string.Empty)
# },



function 活用ルール表変換 () {
    $dat = Import-Csv .\Verb活用ルール.csv -Encoding Default
    
    # 活用形名一覧取得。
    $form_names = $dat | Get-Member -MemberType NoteProperty | foreach {$_.Name} | where {$_ -ne "活用型"}

    foreach ($type in $dat) {
        "`"$($type.活用型)`" => inflectionFormName switch"
        "{"

        foreach ($form in $form_names) {
            if ($type.$form -ne "") {
                if ($type.$form -eq "x") {
                    # 語幹と活用形の表層形が同じならRemoveEndしなくていい。
                    "`"$($form)`" => (true, gokan),"
                } else {
                    "`"$($form)`" => (true, $`"{gokan}$($type.$form -replace 'x', '')`"),"
                }
            }
        }

        "_ => (false, string.Empty)"
        "},"
    }

    "_ => (false, string.Empty)"
}

cls
活用ルール表変換
