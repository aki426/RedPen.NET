# ���ʏW�v�p�̃I�u�W�F�N�g
function Get-ResultObject {
    param(
        [Parameter(Mandatory)]
        $PropertySet
    )

    $o = [pscustomobject] @{
        �i���ו���1 = ""
        �i���ו���2 = ""
        �i���ו���3 = ""
        ���p�^ = ""
        ���` = ""
        �ǂ� = ""
    }

    foreach ($p in $PropertySet) {
        $o | Add-Member -MemberType NoteProperty -Name $p -Value ""
    }

    return $o
}
