Import-Module ./TreeStoreFS.Dll 
New-PSDrive -Name data -PSProvider TreeStoreFS -Root "https://localhost:44371/"
