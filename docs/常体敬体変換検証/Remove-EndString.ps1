
function Remove-EndString {
    param (
        [string]$TargetString,
        [string]$RemoveString
    )
    
    if ($TargetString.EndsWith($RemoveString)) {
        return $TargetString.Substring(0, $TargetString.Length - $RemoveString.Length)
    }
    
    return $TargetString
}
