################################################################################
### �ꊲ���擾����֐���Switch�����쐬����B
################################################################################

# Kuromoji�ɂ��`�ԑf��͂̌��ʁA�K������BaseForm���擾�ł���킯�ł͂Ȃ��̂ŁA
# �e���p�`����ꊲ���t�Z����K�v������B
# �����Ŏ��̂悤�ȕϊ��e�[�u�������̂܂܎��������悤��Switch�����K�v�B
# 
# "�J�ρE�N��" => token.InflectionForm switch
# {
#     "��{�`" => (true, token.Surface),
#     "����`" => (true, $"{token.Surface.RemoveEnd("����")}"),
#     "����k��P" => (true, $"{token.Surface.RemoveEnd("�����")}"),
#     "�̌��ڑ�����" => (true, $"{token.Surface.RemoveEnd("����")}"),
#     "�̌��ڑ�����Q" => (true, $"{token.Surface.RemoveEnd("��")}"),
#     "���߂�" => (true, $"{token.Surface.RemoveEnd("����")}"),
#     "���߂���" => (true, $"{token.Surface.RemoveEnd("����")}"),
#     "���R�E�ڑ�" => (true, $"{token.Surface.RemoveEnd("����")}"),
#     "���R�`" => (true, $"{token.Surface.RemoveEnd("��")}"),
#     "�A�p�`" => (true, $"{token.Surface.RemoveEnd("��")}"),
#     _ => (false, string.Empty)
# },

function ���p���[������ꊲ�擾�R�[�h���� () {
    $dat = Import-Csv .\Verb���p���[��.csv -Encoding Default
    
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

# ���̂悤�Ȋ��p�ω��e�[�u����ǂݑւ����X�C�b�`������肽���B
# gokan�͌ꊲ��\��String�Ōꊲ�擾�֐����炠�炩���ߎ擾������̂Ƃ���B
# 
# "�J�ρE�N��" => type switch
# {
#     "��{�`" => (true, $"{gokan}����"),
#     "����`" => (true, $"{gokan}����"),
#     "����k��P" => (true, $"{gokan}�����"),
#     "�̌��ڑ�����" => (true, $"{gokan}����"),
#     "�̌��ڑ�����Q" => (true, $"{gokan}��"),
#     "���߂�" => (true, $"{gokan}����"),
#     "���߂���" => (true, $"{gokan}����"),
#     "���R�E�ڑ�" => (true, $"{gokan}����"),
#     "���R�`" => (true, $"{gokan}��"),
#     "�A�p�`" => (true, $"{gokan}��"),
#     _ => (false, string.Empty)
# },



function ���p���[���\�ϊ� () {
    $dat = Import-Csv .\Verb���p���[��.csv -Encoding Default
    
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
