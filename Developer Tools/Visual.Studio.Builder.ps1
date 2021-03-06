function Build($solutionPath, $mode)
{
	# 2015 : 'C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com'
	$visualStudioPath =  'C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com'
	
	if (!(Test-Path $visualStudioPath))
	{
		for ($visualStudioVersion = 14; $visualStudioVersion -gt 10; --$visualStudioVersion)
		{
			$visualStudioPath = 'C:\Program Files (x86)\Microsoft Visual Studio ' + $visualStudioVersion + '.0\Common7\IDE\devenv.com'
			
			if (Test-Path $visualStudioPath)
			{
				break;
			}
		}
	}

	Write-Host ''
    Write-Host 'Building' $solutionPath 'in' $mode;
    $rebuildModeParameter = "/Rebuild " + $mode;

    $p = Start-Process -FilePath $visualStudioPath -ArgumentList $solutionPath,$rebuildModeParameter -PassThru -NoNewWindow
	$null = $p.WaitForExit(-1)
	
	Write-Host ''
    Write-Host '.NET Framework Build completed!'
	Write-Host ''
}

function BuildNetCore($solutionPath, $mode) {

	$initialPath = $PWD.Path
	cd $solutionPath
	
	$buildParameters = 'build -c ' + $mode

	$p = Start-Process 'dotnet' -ArgumentList $buildParameters -PassThru -NoNewWindow
	$null = $p.WaitForExit(-1)

	cd $initialPath

	Write-Host ''
    Write-Host '.NET Standard Build completed!'
	Write-Host ''
}