define("{{__GUID_ID__}}_{{__VERSION__}}", ["@microsoft/sp-webpart-base"], function (__microsoft_sp_webpart_base__) {
    var _super = __microsoft_sp_webpart_base__["BaseClientSideWebPart"];

    var __extends = (undefined && undefined.__extends) || (function () {
        var extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return function (d, b) {
            extendStatics(d, b);
            function __() { this.constructor = d; }
            d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
        };
    })();

    var webPart = /** @class */ (function () {
        function webPartClass() {
            return _super !== null && _super.apply(this, arguments) || this;
        }

        console.log("WEBPART: Loaded");

        __extends(webPartClass, _super);

        webPartClass.prototype.onDisplayModeChanged = function (oldDisplay) {
            console.log('WEBPART: onDisplayModeChanged / Old: ' + oldDisplay);
        }

        webPartClass.prototype.render = function () {
            this.domElement.innerHTML = "Test WebPart (Id: " + this.instanceId + ")"; // Important: must always have something

            if (this.renderedOnce) {
                // Special
            }

            console.log("WEBPART: InstanceID: " + this.instanceId);
            console.log("WEBPART: Render");
        };

        webPartClass.prototype.onCustomPropertyChange = function(property, newValue) {
            this.properties[property] = newValue;
            this.render();
        };

        return webPartClass;
    })();

    return {
        default: (webPart),
    }
});
