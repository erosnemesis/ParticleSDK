var target = Argument("target", "Default");
var solutionFiles = GetFiles("ParticleSDK.sln");
var outputDirectory = "Build";
var buildVersion = "1.2.3-beta-4";

Task("AppVeyorUpdate")
	.Does(() =>
{
	if(AppVeyor.IsRunningOnAppVeyor)
	{
		Information("Building on AppVeyor");
		buildVersion = AppVeyor.Environment.Build.Version;
		Information("Build Version is {0}", buildVersion);
	}
	else
	{
		Information("Not building on AppVeyor");
	}
});

Task("CleanUp")
	.Does(()=>
{
	if(System.IO.Directory.Exists(outputDirectory))
	{
		System.IO.Directory.Delete(outputDirectory, true);
	}
	
	System.IO.Directory.CreateDirectory(outputDirectory);
});

Task("UpdateAssemblyVersion")
	.Does(() =>
{
	var fixedVersionString = buildVersion.Replace("-beta-", ".").Replace("-alpha-",".");
	
	if(fixedVersionString.Split('.').Length == 3){
		fixedVersionString += ".0";
	}
	
	CreateAssemblyInfo("Particle\\Properties\\AssemblyVersion.cs", new AssemblyInfoSettings
	{
		Version = fixedVersionString,
		FileVersion = fixedVersionString
	});
});

Task("Build")
	.IsDependentOn("CleanUp")
	.IsDependentOn("AppVeyorUpdate")
	.IsDependentOn("UpdateAssemblyVersion")
	.Does(()=>
{
	foreach(var file in solutionFiles)
	{
		Information("Restoring {0}", file);
		NuGetRestore(file);	
		Information("Building {0}", file);
		MSBuild(file, settings => settings
			.WithProperty("OutputPath", String.Format("..\\{0}\\", outputDirectory))
			.SetPlatformTarget(PlatformTarget.MSIL)
			.SetConfiguration("Release"));
	}
});

Task("SignAssembly")
	.IsDependentOn("Build")
	.Does(()=>
{
	if(AppVeyor.IsRunningOnAppVeyor)
	{
		Sign(String.Format("{0}\\Particle.dll", outputDirectory), new SignToolSignSettings()
		{
			TimeStampUri = new Uri("http://timestamp.digicert.com"),
			CertPath = "pfx\\ParticleNET.pfx",
			Password = EnvironmentVariable("PFXPassword")
		});
	}
	else
	{
		Sign(String.Format("{0}\\Particle.dll", outputDirectory), new SignToolSignSettings()
		{
			TimeStampUri = new Uri("http://timestamp.digicert.com"),
			CertPath = "pfx\\local.pfx",
			Password = "localtest"
		});
	}
});

Task("NUnitTests")
	.Does(() =>
{
	NUnit3(GetFiles(String.Format("{0}\\*Tests*.dll", outputDirectory)), new NUnit3Settings()
	{
		Results = String.Format("{0}\\TestResults.xml", outputDirectory)
	});
});

Task("NuGetPack")
	.IsDependentOn("AppVeyorUpdate")
	.Does(() =>
{
	var local = buildVersion;
	var index = 0;
	if((index = local.IndexOf("-beta-")) > -1)
	{
		var version = long.Parse(local.Substring(index + 6));
		local = String.Format("{0}{1:0000}", local.Substring(0, index+6), version);
	}
	if((index = local.IndexOf("-alpha-")) > -1)
	{
		var version = long.Parse(local.Substring(index + 7));
		local = String.Format("{0}{1:0000}", local.Substring(0, index+7), version);
	}
	StringBuilder releaseNotes = new StringBuilder();
	releaseNotes.AppendLine(local);
	releaseNotes.AppendLine(System.IO.File.ReadAllText("CurrentReleaseNotes.txt"));
	releaseNotes.AppendLine();
	releaseNotes.AppendLine(System.IO.File.ReadAllText("PreviousReleaseNotes.txt"));
	NuGetPack("nuspec\\ParticleNET.ParticleSDK.nuspec", new NuGetPackSettings()
	{
		Version = local,
		OutputDirectory = outputDirectory,
		ReleaseNotes = new []{releaseNotes.ToString()}
	});
});

Task("AppVeyorArtifact")
	.Does(() =>
{
	if(AppVeyor.IsRunningOnAppVeyor)
	{
		var files = GetFiles(String.Format("{0}\\ParticleNET.*.nupkg"));
		foreach(var f in files)
		{
			AppVeyor.UploadArtifact(f);
		}
	}
});

Task("Default")
	.IsDependentOn("Build")
	.IsDependentOn("SignAssembly")
	.IsDependentOn("NUnitTests")
	.IsDependentOn("NuGetPack");

RunTarget(target);