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

        __extends(webPartClass, _super);

        webPartClass.prototype.render = function () {
            this.domElement.innerHTML = " "; // Important: must always have something

            (function (webPart) {
                try {
                    {{__CODE__}}
                } catch (e) { console.error(e); };
            })(this);
        };

        return webPartClass;
    })();

    return {
        default: (webPart),
    }
});
