
### ���̂悤�Ȋ��p�ω��e�[�u����ǂݑւ����X�C�b�`������肽���B

# "�ܒi�E���s������" => type switch
# {
#     "��{�`" => (true, token.BaseForm),
#     "���R�`" => (true, $"{token.BaseForm.RemoveEnd("��")}��"),
#     "���R�E�ڑ�" => (true, $"{token.BaseForm.RemoveEnd("��")}��"),
#     "�A�p�`" => (true, $"{token.BaseForm.RemoveEnd("��")}��"),
#     "�A�p�^�ڑ�" => (true, $"{token.BaseForm.RemoveEnd("��")}��"),
#     "����`" => (true, $"{token.BaseForm.RemoveEnd("��")}��"),
#     "���߂�" => (true, $"{token.BaseForm.RemoveEnd("��")}��"),
#     _ => (false, string.Empty)
# },


function ���p���[���\�ϊ� () {
    $dat = Import-Csv .\Verb���p���[��.csv -Encoding Default
    
    $form_names = $dat | Get-Member -MemberType NoteProperty | foreach {$_.Name} | where {$_ -ne "���p�^" -and $_ -ne "��{�`"}

    foreach ($type in $dat) {
        "`"$($type.���p�^)`" => type switch"
        "{"

        "`"��{�``" => (true, token.BaseForm),"

        $base_suffix = $type.��{�` -replace "x", ""
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
���p���[���\�ϊ�
