################################################################################
### 語幹を取得する関数のSwitch式を作成する。
################################################################################

# Kuromojiによる形態素解析の結果、必ずしもBaseFormが取得できるわけではないので、
# 各活用形から語幹を逆算する必要がある。

function 活用ルールから語幹取得コード生成 () {
    $dat = Import-Csv .\Adj活用ルール.csv -Encoding Default
    
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

function 活用ルール表変換 () {
    $dat = Import-Csv .\Adj活用ルール.csv -Encoding Default
    
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
