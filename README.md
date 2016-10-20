# AppLauncher
Automatic application update and launcher

# Configuration
AppLauncher.exe.config <br>
<ul>
<li>AppArguments: Arguments to be passed to application</li>
<li>VersionFolder: Current application version folder</li>
<li>KeepLastVersions: How many versions must be keeped</li>
<li>AppName: Application name</li>
<li>ExecFile: Executable to be called after update</li>
<li>LogDebug: Write log file inside the folder %TEMP%</li>
<li>IgnoreExtensions: Extensions to be ignored during the update process</li>
<li>MultiInstance: Allow application multi instances</li>
<li>MultiInstanceMessage: Message to show when multi instance is not allowed</li>
</ul>
  
# Install
Just copy the AppLauncher.exe and AppLauncher.exe.config to a network folder and out your application release in the same folder. <br>
Ex:  <br>
<dl>
<dt>MyApplication</dt>
<dd>AppLauncher.exe</dd>
<dd>AppLauncher.exe.config</dd>
<dd>MyCurrentRelease <i>(This one goes to the VersionFolder parameter)</i></dd>
    <dl>
    <dd>    MyApplication.exe <i>(This one goes to the ExecFile parameter)</i></dd>
    <dd>    ...</dd>
    </dl>
</dl>
