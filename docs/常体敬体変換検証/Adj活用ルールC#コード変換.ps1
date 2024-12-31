################################################################################
### �ꊲ���擾����֐���Switch�����쐬����B
################################################################################

# Kuromoji�ɂ��`�ԑf��͂̌��ʁA�K������BaseForm���擾�ł���킯�ł͂Ȃ��̂ŁA
# �e���p�`����ꊲ���t�Z����K�v������B

function ���p���[������ꊲ�擾�R�[�h���� () {
    $dat = Import-Csv .\Adj���p���[��.csv -Encoding Default
    
    # ���p�`���ꗗ�擾�B
    $form_names = $dat | Get-Member -MemberType NoteProperty | foreach {$_.Name} | where {$_ -ne "���p�^"}

    foreach ($type in $dat) {
        "`"$($type.���p�^)`" => token.InflectionForm switch"
        "{"

        # 1��1�̊��p�`�ɂ��Ċ��p����𖖔����珜���������́��ꊲ��Ԃ��R�[�h�𐶐�����B
        foreach ($form in $form_names) {
            if ($type.$form -ne "") {
                if ($type.$form -eq "x") {
                    # �ꊲ�Ɗ��p�`�̕\�w�`�������Ȃ�RemoveEnd���Ȃ��Ă����B
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
���p���[������ꊲ�擾�R�[�h����


################################################################################
### TokenElement���犈�p�`���擾����֐���Switch�����쐬����B
################################################################################

function ���p���[���\�ϊ� () {
    $dat = Import-Csv .\Adj���p���[��.csv -Encoding Default
    
    # ���p�`���ꗗ�擾�B
    $form_names = $dat | Get-Member -MemberType NoteProperty | foreach {$_.Name} | where {$_ -ne "���p�^"}

    foreach ($type in $dat) {
        "`"$($type.���p�^)`" => inflectionFormName switch"
        "{"

        foreach ($form in $form_names) {
            if ($type.$form -ne "") {
                if ($type.$form -eq "x") {
                    # �ꊲ�Ɗ��p�`�̕\�w�`�������Ȃ�RemoveEnd���Ȃ��Ă����B
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
���p���[���\�ϊ�
