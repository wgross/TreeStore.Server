
# Invoke-AtContainer "$PSScriptRoot\..\src\TreeStoreFS" {
#     dotnet publish
# }

# $scriptBlock = {
#     Set-Location "D:\src\TreeStore.Server\src\TreeStoreFS\bin\Debug\net6.0\publish"
#     Import-Module ./TreeStoreFS.dll
# }

# Start-Process pwsh -Wait -ArgumentList @(
#     "-Command", "{ $($scriptBlock.ToString()) }"
# )

$profileDirectory = $PSScriptRoot

"D:\src\TreeStore.Server\src\TreeStoreFS\bin\Debug\net6.0\publish" | Invoke-AtContainer {
    
    dotnet-fs\Invoke-AtDotnetProjectItem -Csproj { $_ | dotnet-cli-publish\Invoke-DotNetPublish }

    $workingDirectory = $PWD

    #"D:\src\TreeStore.Server\submodules\TreeStore.ProviderCore\submodules\PowerShell\src\powershell-win-core\bin\Debug\net6.0"|Invoke-AtContainer {
    "D:\src\TreeStore.Server\submodules\TreeStore.ProviderCore\submodules\PowerShell\src\powershell-win-core\bin\Debug\net6.0\win7-x64"|Invoke-AtContainer {

        ./pwsh.exe -Interactive -NoProfile -NoExit -WorkingDirectory $workingDirectory -File $profileDirectory/run-init.ps1
    }
}

