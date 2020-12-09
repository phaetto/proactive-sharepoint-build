# SharePoint SPFx Build tools

At the time of this writing SPFx components can be packed and scaffolded only by using nodejs.

This tool generates SPFx components ready to be uploaded to a SharePoint app site even on a server side web application.

## How to
Install the command line tool by using the following:

`dotnet tool install ProActive.SharePoint.Build.Console --global`

You can now generate an spfx:
- `proactive-sharepoint-build init --name MyWidget --webpart`
- _Edit your js in MyWidget/index.js_
- `proactive-sharepoint-build pack --source-folder MyWidget`
 
At this point file MyWidget.sppkg has been generated. Upload it to your tenant.

You can easily iterate on the above process to get the desired results.

Check the tool help by running `proactive-sharepoint-build help` or `proactive-sharepoint-build help <command>` for more details.

## License

The code is licensed under [Apache 2.0 license](LICENSE).