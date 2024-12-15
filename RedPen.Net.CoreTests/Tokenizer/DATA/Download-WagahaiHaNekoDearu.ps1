# TLS 1.2��L����
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# �_�E�����[�h���URL
$url = "https://www.aozora.gr.jp/cards/000148/files/789_ruby_5639.zip"

# �_�E�����[�h����ZIP�t�@�C���̕ۑ���p�X
$downloadPath = Join-Path $pwd "downloaded_file.zip"

# �W�J��̃t�H���_�p�X
$extractPath = Join-Path $pwd "extracted_files"

try {
    Write-Host "�t�@�C�����_�E�����[�h���Ă��܂�..."
    
    # Invoke-WebRequest���g�p���ăt�@�C�����_�E�����[�h
    Invoke-WebRequest -Uri $url -OutFile $downloadPath -UseBasicParsing
    
    if (Test-Path $downloadPath) {
        Write-Host "�_�E�����[�h���������܂���"

        # �W�J��t�H���_�����݂��Ȃ��ꍇ�͍쐬
        if (-not (Test-Path -Path $extractPath)) {
            New-Item -ItemType Directory -Path $extractPath | Out-Null
        }

        # ZIP�t�@�C����W�J
        Write-Host "�t�@�C����W�J���Ă��܂�..."
        Expand-Archive -Path $downloadPath -DestinationPath $extractPath -Force
        Write-Host "�W�J���������܂���"

        # �_�E�����[�h����ZIP�t�@�C�����폜
        Remove-Item -Path $downloadPath
        Write-Host "�ꎞ�t�@�C�����폜���܂���"

        # �s�v�t�@�C���폜
        Move-Item -Path (Join-Path $extractPath "wagahaiwa_nekodearu.txt") -Destination $pwd
        Remove-Item -Path $extractPath
    }
    else {
        Write-Error "�t�@�C���̃_�E�����[�h�Ɏ��s���܂���"
    }
}
catch {
    Write-Error "�G���[���������܂���: $_"
    
    # �G���[�̏ڍ׏���\��
    Write-Host "�G���[�̏ڍ�:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        Write-Host "�X�e�[�^�X�R�[�h: $($_.Exception.Response.StatusCode.Value__)"
        Write-Host "�X�e�[�^�X�̐���: $($_.Exception.Response.StatusDescription)"
    }
}