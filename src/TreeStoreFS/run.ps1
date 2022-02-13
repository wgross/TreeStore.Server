# starts a pwsh and runs the init.ps1 in the new shell.
pwsh.exe -Interactive -NoExit -WorkingDirectory $PSScriptRoot -File $PSScriptRoot/init.ps1