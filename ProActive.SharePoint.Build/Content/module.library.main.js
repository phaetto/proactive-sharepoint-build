define("{{__GUID_ID__}}_{{__VERSION__}}", [{{__LIBRARIES__}}], function () {
    try {
        {{__CODE__}}
    } catch (e) { console.error("ProActive.SharePoint.Build/Library: JS code error"); console.error(e); throw e; };
});