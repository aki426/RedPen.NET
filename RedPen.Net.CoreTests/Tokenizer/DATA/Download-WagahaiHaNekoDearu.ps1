# TLS 1.2��L����
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# �_�E�����[�h���URL
$url = "https://www.cl.ecei.tohoku.ac.jp/nlp100/data/neko.txt"

# �_�E�����[�h�����t�@�C���̕ۑ���p�X
$downloadPath = Join-Path $pwd "neko.txt"

try {
    Write-Host "�t�@�C�����_�E�����[�h���Ă��܂�..."
    
    # Invoke-WebRequest���g�p���ăt�@�C�����_�E�����[�h
    Invoke-WebRequest -Uri $url -OutFile $downloadPath -UseBasicParsing
    
    if (Test-Path $downloadPath) {
        Write-Host "�_�E�����[�h���������܂���"
        Write-Host "�ۑ���: $downloadPath"
    }
    else {
        Write-Error "�t�@�C���̃_�E�����[�h�Ɏ��s���܂���"
    }
}
catch {
    Write-Error "�G���[���������܂���: $_"
    Write-Host "�G���[�̏ڍ�:" -ForegroundColor Red
    Write-Host $_.Exception.Message
}
