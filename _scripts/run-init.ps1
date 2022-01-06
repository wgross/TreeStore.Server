Import-Module ./TreeStoreFS.psd1 
New-PSDrive -Name data -PSProvider TreeStoreFS -Root "https://localhost:44371/"

Set-Location data:\
New-Item -Path ./c1 -ItemType category 
New-ItemProperty -Path .\c1\ -Name aLong -Type long

Set-Location data:\c1
New-Item -Path ./item1 -ItemType entity
# set long prperty with int
Set-ItemProperty -Path .\item1 -Name aLong -Value 10