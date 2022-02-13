 
 Import-Module $PSScriptRoot/TreeStoreFS.psd1

 New-PSDrive -Name data -PSProvider TreeStoreFS -Root https://localhost:5001
 Set-Location -Path data:\

 New-Item -Name test # create category
 New-ItemProperty -Path ./test -Name destination -PropertyType string

 Set-Location -Path data:\test
 New-Item -Name file -ItemType entity
 Set-ItemProperty -Path ./file -Name destination -Value "destination"
