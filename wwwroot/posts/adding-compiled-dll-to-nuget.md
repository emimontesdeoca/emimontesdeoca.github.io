Just the other day I found a bug about a `NullReferenceException` on a missing library that should have been there since it comes from a nuget package we have uploaded.

Basically, I compiled a library class that referenced a few projects. On the build phase it would add the `dll` files to the `bin` folder but when `packaging` it as a nuget package and installing them in another solution, the `dll` files that I referenced and should be copied into the bin folder, are not there.

### How to solve it

Solving this issue is pretty simple, you have to update the `nuspec` file and for each of the libraries that you want to copy, add the `file` in the `files` part.

```xml
<?xml version="1.0" encoding="utf-8"?>
<package >
  <metadata>
    <id>$id$</id>
    <version>$version$</version>
    <title>$title$</title>
    <authors>$author$</authors>
    <owners>$author$</owners>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <license type="expression">MIT</license>
    <projectUrl>$projectUrl$</projectUrl>
    <iconUrl>$projectIconUrl$</iconUrl>
    <description>$description$</description>
    <releaseNotes></releaseNotes>
    <copyright>$copyright$</copyright>
    <tags></tags>
  </metadata>
  <files>
        <file src="bin\Release\MyFile1.dll" target="lib\net47" />
        <file src="bin\Release\MyFile2.dll" target="lib\net47" />
        <file src="bin\Release\MyFile3.dll" target="lib\net47" />
        <file src="bin\Release\MyFile4.dll" target="lib\net47" />
   </files>
</package>
```