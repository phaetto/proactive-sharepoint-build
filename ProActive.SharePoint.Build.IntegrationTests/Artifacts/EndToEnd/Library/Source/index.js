return (function (dependency) {
    console.log("Awesome library function (No. 1)");
    console.log(dependency);

    return {
        log: function () {
            console.log("Hey (No. 1)");
        }
    }
})(arguments);